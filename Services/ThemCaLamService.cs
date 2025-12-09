using MySql.Data.MySqlClient;
using QuanLyChamCong.Helpers;
using QuanLyChamCong.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace QuanLyChamCong.Services
{
    public class ThemCaLamService
    {
        private readonly string _connectionString;
        private NhanVienChonModel nhanVien { get; set; }
        public ThemCaLamService()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
        }

        public List<NhanVienChonModel> LayDanhSachNhanVien(int DoanhNghiepID, int? IDTimca = null)
        {
            var danhSach = new List<NhanVienChonModel>();
            string query = "";

            // TRƯỜNG HỢP 1: Lấy danh sách nhân viên cụ thể trong 1 ca
            if (IDTimca.HasValue)
            {
                query = @"SELECT nv.id, nv.ho_ten
                          FROM nhan_vien nv
                          JOIN phan_cong_ca pcc ON pcc.nhan_vien_id = nv.id
                          JOIN tai_khoan tk ON tk.id = nv.tai_khoan_id
                          WHERE nv.doanh_nghiep_id = @DoanhNghiepId
                            AND pcc.ca_id = @IDTimca
                            AND tk.trang_thai = 'active'";
            }
            // TRƯỜNG HỢP 2: Lấy tất cả nhân viên của doanh nghiệp (Không quan tâm ca)
            else
            {
                query = @"SELECT nv.id, nv.ho_ten
                          FROM nhan_vien nv
                          JOIN tai_khoan tk ON tk.id = nv.tai_khoan_id
                          WHERE nv.doanh_nghiep_id = @DoanhNghiepId
                AND tk.trang_thai = 'active'";
            }
            using (var conn = new MySqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@DoanhNghiepID", DoanhNghiepID);
                    cmd.Parameters.AddWithValue("@IDTimca", IDTimca.HasValue ? (object)IDTimca.Value : DBNull.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var nv = new NhanVienChonModel
                            {
                                Id = reader.GetInt32("id"),
                                HoTen = reader.GetString("ho_ten"),
                                IsSelected = false // Mặc định chưa chọn
                            };
                            danhSach.Add(nv);
                        }
                    }
                }
            }
            return danhSach;
        }
        public async Task<bool> ThemCaMoiAsync(CaLamModel caMoi, IEnumerable<NhanVienChonModel> nhanVienDaChon)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // 1. Bắt đầu Transaction (Quan trọng)
                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        // ---------------------------------------------------------
                        // BƯỚC 1: Insert vào bảng ca_lam và lấy ID vừa tạo
                        // ---------------------------------------------------------
                        string sqlCaLam = @"
                        INSERT INTO ca_lam (doanh_nghiep_id, ngay_lam, gio_bat_dau, gio_ket_thuc, loai_ca )
                        VALUES (@dnId, @ngay, @bd, @kt, @loaiCa);
                        SELECT LAST_INSERT_ID();"; // Lệnh này lấy ID tự tăng vừa insert

                        long newCaId = 0;

                        using (var cmdCa = new MySqlCommand(sqlCaLam, conn))
                        {
                            // Gán Transaction cho command
                            cmdCa.Transaction = transaction;

                            // Truyền tham số
                            cmdCa.Parameters.AddWithValue("@dnId", DoanhNghiep.CurrentID);
                            cmdCa.Parameters.AddWithValue("@loaiCa", caMoi.LoaiCa);
                            cmdCa.Parameters.AddWithValue("@ngay", caMoi.NgayLam); // Format: yyyy-MM-dd
                            cmdCa.Parameters.AddWithValue("@bd", caMoi.GioBatDau); // Format: HH:mm:ss
                            cmdCa.Parameters.AddWithValue("@kt", caMoi.GioKetThuc);

                            // ExecuteScalarAsync dùng để lấy giá trị đầu tiên trả về (chính là ID)
                            var result = await cmdCa.ExecuteScalarAsync();
                            newCaId = Convert.ToInt64(result);
                        }

                        // ---------------------------------------------------------
                        // BƯỚC 2: Insert danh sách nhân viên vào bảng phan_cong_ca
                        // ---------------------------------------------------------
                        if (nhanVienDaChon != null && nhanVienDaChon.Any())
                        {
                            string sqlPhanCong = @"INSERT INTO phan_cong_ca (ca_id, nhan_vien_id) VALUES (@caId, @nvId)";

                            using (var cmdPC = new MySqlCommand(sqlPhanCong, conn))
                            {
                                cmdPC.Transaction = transaction;

                                // Thêm tham số @caId (giống nhau cho mọi nhân viên)
                                cmdPC.Parameters.AddWithValue("@caId", newCaId);
                                // Tạo placeholder cho @nvId
                                cmdPC.Parameters.Add("@nvId", MySqlDbType.Int32);

                                foreach (var nv in nhanVienDaChon)
                                {
                                    // Chỉ cần cập nhật giá trị cho @nvId
                                    cmdPC.Parameters["@nvId"].Value = nv.Id;
                                    await cmdPC.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        // 3. Nếu mọi thứ OK, Commit Transaction (Lưu chính thức vào DB)
                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        // 4. Có lỗi xảy ra -> Rollback (Hủy bỏ mọi thay đổi nãy giờ)
                        await transaction.RollbackAsync();

                        // Log lỗi ra console hoặc file log
                        Console.WriteLine("Lỗi thêm ca: " + ex.Message);
                        return false; // Hoặc throw ex nếu muốn tầng trên xử lý
                    }
                }
            }
        }
    }
}
