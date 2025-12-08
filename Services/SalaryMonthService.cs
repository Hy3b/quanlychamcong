using MySql.Data.MySqlClient;
using QuanLyChamCong.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ClosedXML.Excel.XLPredefinedFormat;

namespace QuanLyChamCong.Services
{
    // ... (các using giữ nguyên)

    public class SalaryMonthService
    {
        // Thêm tham số selectedMonth vào hàm
        public async Task<List<MonthlySalary>> GetMonthlyHistory(System.DateTime? selectedMonth)
        {
            List<MonthlySalary> list = new List<MonthlySalary>();
            string connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                    // --- SỬA CÂU SQL TẠI ĐÂY ---
                    // 1. Dùng LEFT JOIN (để lấy cả nhân viên đã xóa/sai ID)
                    // 2. Dùng DATE_FORMAT và LIKE để so sánh ngày tháng an toàn nhất
                    string query = @"
                        SELECT lt.*, IFNULL(nv.ho_ten, 'Nhân viên đã xóa') as ho_ten 
                        FROM luong_thang lt
                        LEFT JOIN nhan_vien nv ON lt.nhan_vien_id = nv.id 
                        WHERE (@month IS NULL 
                               OR lt.thang LIKE CONCAT(DATE_FORMAT(@month, '%Y-%m'), '%'))
                        ORDER BY lt.thang DESC, lt.nhan_vien_id ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Kiểm tra null an toàn
                        object paramValue = selectedMonth.HasValue ? selectedMonth.Value : DBNull.Value;
                        cmd.Parameters.AddWithValue("@month", paramValue);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                MonthlySalary item = new MonthlySalary();

                                item.Id = Convert.ToInt32(reader["id"]);
                                item.NhanVienId = reader["nhan_vien_id"].ToString();
                                item.HoTen = reader["ho_ten"].ToString();

                                // Xử lý cột 'thang' an toàn (Dù DB lưu là '2025-11' hay '2025-11-01' đều chạy được)
                                if (reader["thang"] != DBNull.Value)
                                {
                                    try
                                    {
                                        // Cố gắng chuyển đổi sang ngày để format đẹp
                                        item.Thang = Convert.ToDateTime(reader["thang"]).ToString("MM/yyyy");
                                    }
                                    catch
                                    {
                                        // Nếu không chuyển được (lỗi định dạng lạ) thì hiển thị nguyên gốc
                                        item.Thang = reader["thang"].ToString();
                                    }
                                }

                                if (reader["tong_luong_co_ban"] != DBNull.Value) item.TongLuongCoBan = Convert.ToDecimal(reader["tong_luong_co_ban"]);
                                if (reader["tong_phu_cap"] != DBNull.Value) item.TongPhuCap = Convert.ToDecimal(reader["tong_phu_cap"]);
                                if (reader["tong_tang_ca"] != DBNull.Value) item.TongTangCa = Convert.ToDecimal(reader["tong_tang_ca"]);
                                if (reader["tong_tru_bao_hiem"] != DBNull.Value) item.TongTruBaoHiem = Convert.ToDecimal(reader["tong_tru_bao_hiem"]);
                                if (reader["tong_tru_thue"] != DBNull.Value) item.TongTruThue = Convert.ToDecimal(reader["tong_tru_thue"]);
                                if (reader["thuc_linh_thang"] != DBNull.Value) item.ThucLinhThang = Convert.ToDecimal(reader["thuc_linh_thang"]);

                                item.NgayChotLuong = reader["ngay_chot_luong"].ToString();
                                list.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Lỗi từ Service: " + ex.Message);
                }
            }
            return list;
        }
    }
}
