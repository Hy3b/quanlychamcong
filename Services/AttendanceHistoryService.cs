using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MySql.Data.MySqlClient;
using QuanLyChamCong.Models;
using QuanLyChamCong.Helpers;
namespace QuanLyChamCong.Services
{
    class AttendanceHistoryService
    {

        private readonly string _connectionString;
        public AttendanceHistoryService() {
            _connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
        }
        public List<AttendanceRecord> GetAttendanceHistory(int nhanVienId)
        {
            var list = new List<AttendanceRecord>();

            // Câu lệnh SQL: Lấy dữ liệu theo nhân viên, sắp xếp ngày mới nhất lên đầu
            string query = @"SELECT 
                            cc.gio_vao, 
                            cc.gio_ra, 
                            cc.trang_thai
                        FROM cham_cong cc
                        JOIN nhan_vien nv 
                              ON nv.id = cc.nhan_vien_id        -- để lấy doanh nghiệp
                        WHERE cc.nhan_vien_id = @NhanVienId
                          AND nv.doanh_nghiep_id = @DoanhNghiepID
                        ORDER BY cc.gio_vao DESC;";

            using (MySqlConnection conn = new MySqlConnection(_connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@NhanVienId", nhanVienId);
                        cmd.Parameters.AddWithValue("@DoanhNghiepID", DoanhNghiep.CurrentID);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var record = new AttendanceRecord
                                {

                                    gio_vao= reader.GetDateTime("gio_vao"),

                                    // Xử lý Giờ ra (có thể null nếu nhân viên chưa về)
                                    gio_ra = reader.IsDBNull(reader.GetOrdinal("gio_ra"))
                                            ? (DateTime?)null
                                            : reader.GetDateTime("gio_ra"),

                                    // Xử lý chuỗi (kiểm tra null cho chắc chắn)
                                    trang_thai = reader.IsDBNull(reader.GetOrdinal("trang_thai"))
                                            ? string.Empty
                                            : reader.GetString("trang_thai")
                                };

                                list.Add(record);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Xử lý lỗi (ví dụ: mất mạng, sai pass DB...)
                    throw new Exception($"Lỗi truy xuất lịch sử chấm công: {ex.Message}");
                }
            }

            return list;
        }
    }
}
