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
    internal class AttendanceService 
    {
        public List<AttendanceModel> GetDailyAttendance()
        {
            List<AttendanceModel> list = new List<AttendanceModel>();

            string connect = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            // Câu SQL Y HỆT bạn yêu cầu
            string query = @"
                    SELECT 
                        nv.id,
                        nv.ho_ten,
                        nv.chuc_vu,
                        cl.gio_bat_dau,
                        cl.gio_ket_thuc,
                        cc.gio_vao,
                        cc.gio_ra,
                        cc.trang_thai
                    FROM cham_cong cc
                    JOIN nhan_vien nv ON cc.nhan_vien_id = nv.id
                    JOIN ca_lam cl ON cc.ca_id = cl.id
                    WHERE cl.ngay_lam = CURDATE() 
                    ORDER BY cc.gio_vao DESC;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connect))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new AttendanceModel();

                                // 1. Thông tin nhân viên
                                item.NhanVienId = Convert.ToInt32(reader["id"]);
                                item.HoTen = reader["ho_ten"].ToString();
                                item.ChucVu = reader["chuc_vu"] != DBNull.Value ? reader["chuc_vu"].ToString() : "";

                                // 2. Giờ Ca làm (Trong DB là TIME -> C# là TimeSpan)
                                item.GioBatDau = (TimeSpan)reader["gio_bat_dau"];
                                item.GioKetThuc = (TimeSpan)reader["gio_ket_thuc"];

                                // 3. Giờ Check-in (Trong DB là DATETIME -> C# lấy TimeOfDay)
                                if (reader["gio_vao"] != DBNull.Value)
                                {
                                    item.GioVao = Convert.ToDateTime(reader["gio_vao"]).TimeOfDay;
                                }
                                else
                                {
                                    item.GioVao = null;
                                }

                                // 4. Giờ Check-out
                                if (reader["gio_ra"] != DBNull.Value)
                                {
                                    item.GioRa = Convert.ToDateTime(reader["gio_ra"]).TimeOfDay;
                                }
                                else
                                {
                                    item.GioRa = null;
                                }

                                // 5. Trạng thái
                                item.TrangThai = reader["trang_thai"].ToString();

                                list.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (Log hoặc thông báo)
                Console.WriteLine("Lỗi SQL: " + ex.Message);
            }

            return list;
        }
    }
}
