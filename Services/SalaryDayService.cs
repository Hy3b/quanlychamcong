using MySql.Data.MySqlClient;
using QuanLyChamCong.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Services
{
    internal class SalaryDayService
    {
        // Hàm cập nhật lương, trả về true nếu thành công
        public async Task<bool> UpdateDailySalary(string nhanVienId, decimal phuCap, decimal luongTangCa, decimal truThue)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync(); // Mở kết nối bất đồng bộ

                    string query = @"UPDATE luong_ngay 
                                     SET phu_cap = @phuCap, 
                                         luong_tang_ca = @tangCa,
                                         tru_thue = @thue,
                                         thuc_linh_ngay = ( @phuCap*500 + @tangCa) - @thue
                                     WHERE nhan_vien_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@phuCap", phuCap);
                        cmd.Parameters.AddWithValue("@tangCa", luongTangCa);
                        cmd.Parameters.AddWithValue("@thue", truThue);
                        cmd.Parameters.AddWithValue("@id", nhanVienId);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi tại đây nếu cần
                    throw new Exception("Lỗi từ Service: " + ex.Message);
                }
            }
        }

        //==== Hàm này hiển thị lương ngày  SalaryDayViewMOdel
        public async Task<List<SalaryDay>> GetDailySalaries(string searchText, DateTime? selectedDate)
        {
            List<SalaryDay> list = new List<SalaryDay>();
            string connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    string query = @"
                        SELECT 
                            ln.*, 
                            nv.ho_ten,
                            cc.phut_tang_ca
                        FROM luong_ngay ln
                        JOIN nhan_vien nv ON ln.nhan_vien_id = nv.id
                        
                        -- LEFT JOIN để nếu quên chấm công vẫn hiện bảng lương (nhưng phút = 0)
                        LEFT JOIN cham_cong cc ON ln.nhan_vien_id = cc.nhan_vien_id 
                                               AND DATE(cc.gio_vao) = DATE(ln.ngay_tinh_luong)

                        WHERE (@key IS NULL OR @key = '' 
                               OR nv.ho_ten LIKE @search 
                               OR ln.nhan_vien_id LIKE @search)
                          AND (@date IS NULL OR DATE(ln.ngay_tinh_luong) = DATE(@date))";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@key", searchText);
                        cmd.Parameters.AddWithValue("@search", $"%{searchText}%");
                        cmd.Parameters.AddWithValue("@date", selectedDate);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                SalaryDay emp = new SalaryDay();

                                emp.NhanVienId = reader["nhan_vien_id"].ToString();
                                emp.HoTen = reader["ho_ten"].ToString();

                                if (reader["luong_co_ban_ngay"] != DBNull.Value)
                                    emp.LuongCoBanNgay = Convert.ToDecimal(reader["luong_co_ban_ngay"]);

                                if (reader["phu_cap"] != DBNull.Value)
                                    emp.PhuCap = Convert.ToDecimal(reader["phu_cap"]);

                                if (reader["phut_tang_ca"] != DBNull.Value)
                                {
                                    decimal phut = Convert.ToDecimal(reader["phut_tang_ca"]);
                                    emp.PhutTangCa = phut;       // <--- Gán vào đây
                                    emp.LuongTangCa = phut * 200;
                                }

                                if (reader["tru_thue"] != DBNull.Value)
                                    emp.TruThue = Convert.ToDecimal(reader["tru_thue"]);

                                if (reader["thuc_linh_ngay"] != DBNull.Value)
                                    emp.ThucLinhNgay = Convert.ToDecimal(reader["thuc_linh_ngay"]);

                                // Giữ nguyên logic của bạn: TrangThai gán bằng ngày tính lương
                                emp.TrangThai = reader["ngay_tinh_luong"].ToString();

                                list.Add(emp);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi lấy dữ liệu: " + ex.Message);
                }
            }
            return list;
        }
    }
}
