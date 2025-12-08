using MySql.Data.MySqlClient;
using QuanLyChamCong.Models;
using QuanLyChamCong.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyChamCong.Helpers;
namespace QuanLyChamCong.Services
{
    class CaLamService
    {
        private readonly string _connectionString;
        public CaLamService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
        }
        public async Task<List<CaLamModel>> LayDanhSachCaAsync()
        {
            var danhSach = new List<CaLamModel>();
            string query = @"
                        SELECT 
                            cl.id, 
                            cl.loai_ca, 
                            cl.ngay_lam, 
                            cl.gio_bat_dau, 
                            cl.gio_ket_thuc, 
                            COUNT(pcc.nhan_vien_id) AS so_luong_nv
                        FROM ca_lam cl
                        LEFT JOIN phan_cong_ca pcc ON cl.id = pcc.ca_id
                        where cl.doanh_nghiep_id = @DoanhNghiepId
                        GROUP BY 
                            cl.id, cl.loai_ca, cl.ngay_lam, 
                            cl.gio_bat_dau, cl.gio_ket_thuc
                        ORDER BY cl.ngay_lam DESC, cl.gio_bat_dau ASC;";
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DoanhNghiepId", DoanhNghiep.CurrentID); // Thay 1 bằng ID doanh nghiệp thực tế
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var ca = new CaLamModel();


                            ca.CaId = reader.GetInt32("id");

                            // 2. Map các cột cơ bản
                            ca.LoaiCa = reader.GetString("loai_ca"); // "Ca sáng"
                            ca.NgayLam = reader.GetDateTime("ngay_lam");
                            ca.GioBatDau = (TimeSpan)reader["gio_bat_dau"];
                            ca.GioKetThuc = (TimeSpan)reader["gio_ket_thuc"];

                            // 3. Lấy số lượng nhân viên đã đếm được
                            ca.SoNhanVien = reader.GetInt32("so_luong_nv");

                            // 4. Tính toán Trạng Thái (Vì DB không có cột status tổng của Ca)


                            danhSach.Add(ca);
                        }
                    }
                }

                return danhSach;
            }
        }
        public async Task<bool> SuaCaAsync(CaLamModel caSua, IEnumerable<NhanVienChonModel> nhanVienDaChon)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // Bắt đầu Transaction
                using (var trans = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // ---------------------------------------------------------
                        // BƯỚC 1: Cập nhật thông tin bảng ca_lam (Giờ, Ngày)
                        // ---------------------------------------------------------
                        string sqlUpdateCa = @"
                    UPDATE ca_lam 
                    SET ngay_lam = @ngay, 
                        gio_bat_dau = @bd, 
                        gio_ket_thuc = @kt
                    WHERE id = @id AND doanh_nghiep_id = @dnId";

                        using (var cmdCa = new MySqlCommand(sqlUpdateCa, conn, trans))
                        {
                            cmdCa.Parameters.AddWithValue("@ngay", caSua.NgayLam);
                            cmdCa.Parameters.AddWithValue("@bd", caSua.GioBatDau);
                            cmdCa.Parameters.AddWithValue("@kt", caSua.GioKetThuc);
                            cmdCa.Parameters.AddWithValue("@id", caSua.CaId);
                            cmdCa.Parameters.AddWithValue("@dnId", DoanhNghiep.CurrentID); // Bảo mật: check đúng DN

                            await cmdCa.ExecuteNonQueryAsync();
                        }

                        // ---------------------------------------------------------
                        // BƯỚC 2: Cập nhật nhân viên (Xóa cũ -> Thêm mới)
                        // ---------------------------------------------------------

                        // 2.1 Xóa toàn bộ nhân viên cũ trong ca này
                        string sqlDeleteCu = "DELETE FROM phan_cong_ca WHERE ca_id = @caId";
                        using (var cmdDel = new MySqlCommand(sqlDeleteCu, conn, trans))
                        {
                            cmdDel.Parameters.AddWithValue("@caId", caSua.CaId);
                            await cmdDel.ExecuteNonQueryAsync();
                        }

                        // 2.2 Thêm lại danh sách nhân viên mới (nếu có chọn ai đó)
                        if (nhanVienDaChon != null && nhanVienDaChon.Any())
                        {
                            string sqlInsertMoi = "INSERT INTO phan_cong_ca (ca_id, nhan_vien_id) VALUES (@caId, @nvId)";
                            using (var cmdIns = new MySqlCommand(sqlInsertMoi, conn, trans))
                            {
                                cmdIns.Parameters.AddWithValue("@caId", caSua.CaId);
                                cmdIns.Parameters.Add("@nvId", MySqlDbType.Int32);

                                foreach (var nv in nhanVienDaChon)
                                {
                                    // Chỉ cần insert những người được chọn (IsSelected == true)
                                    // Lưu ý: Ở tầng ViewModel bạn nên lọc list này trước khi truyền vào đây
                                    cmdIns.Parameters["@nvId"].Value = nv.Id;
                                    await cmdIns.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        // ---------------------------------------------------------
                        // BƯỚC 3: Hoàn tất
                        // ---------------------------------------------------------
                        await trans.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();
                        // Log lỗi: Console.WriteLine(ex.Message);
                        return false;
                    }
                }
            }
        }
        public async Task<bool> XoaCaAsync(int caId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var trans = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // BƯỚC 1: Xóa phân công ca TRƯỚC (Xóa con)
                        // Phải xóa những thằng đang tham chiếu đến ca này trước
                        string sqlDeletePhanCong = "DELETE FROM phan_cong_ca WHERE ca_id = @caId";
                        using (var cmd = new MySqlCommand(sqlDeletePhanCong, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@caId", caId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // BƯỚC 2: Xóa ca làm SAU (Xóa cha)
                        // Giờ bảng phân công sạch rồi, xóa ca mới được
                        string sqlDeleteCa = "DELETE FROM ca_lam WHERE id = @id";
                        using (var cmd = new MySqlCommand(sqlDeleteCa, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@id", caId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        await trans.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await trans.RollbackAsync();
                        // Console.WriteLine("Lỗi xóa ca: " + ex.Message);
                        return false;
                    }
                }
            }
        }
    }
}

                // Bắt đầu Transaction
