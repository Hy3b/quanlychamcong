using MySql.Data.MySqlClient;
using QuanLyChamCong.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Services
{
    class DashboardService
    {
        public List<DaylyAttendanceStat> GetLast7DaysStats()
        {
            // 1. Danh sách tạm để chứa dữ liệu thô từ Database

            List<DaylyAttendanceStat> rawData = new List<DaylyAttendanceStat>();
            string connect = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
            string query = @"
                    SELECT 
                        cl.ngay_lam,
                        SUM(CASE WHEN cc.trang_thai = 'checked_in' THEN 1 ELSE 0 END) AS dung_gio,
                        SUM(CASE WHEN cc.trang_thai = 'late' THEN 1 ELSE 0 END) AS di_muon,
                        SUM(CASE WHEN cc.trang_thai = 'absent' THEN 1 ELSE 0 END) AS vang
                    FROM cham_cong cc
                    JOIN ca_lam cl ON cc.ca_id = cl.id
                    WHERE cl.ngay_lam BETWEEN DATE_SUB(CURDATE(), INTERVAL 6 DAY) AND CURDATE()
                    GROUP BY cl.ngay_lam";
            // Lưu ý: Tôi bỏ cột 'thu_trong_tuan' trong SQL để xử lý đồng bộ ở C# cho dễ

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
                                rawData.Add(new DaylyAttendanceStat
                                {
                                    Date = Convert.ToDateTime(reader["ngay_lam"]),
                                    // Các hàm SUM trong MySQL trả về Decimal/Int64, cần convert an toàn
                                    OnTimeCount = Convert.ToInt32(reader["dung_gio"]),
                                    LateCount = Convert.ToInt32(reader["di_muon"]),
                                    AbsentCount = Convert.ToInt32(reader["vang"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi SQL: " + ex.Message);
                // Tùy chọn: return null hoặc list rỗng tùy logic
            }

            // ============================================================
            // BƯỚC XỬ LÝ QUAN TRỌNG: LẤP ĐẦY DỮ LIỆU (GAP FILLING)
            // ============================================================

            List<DaylyAttendanceStat> full7Days = new List<DaylyAttendanceStat>();
            DateTime today = DateTime.Now.Date;
            CultureInfo viCulture = new CultureInfo("vi-VN"); // Để hiển thị Thứ Hai, Thứ Ba...

            // Chạy vòng lặp 7 lần (0 -> 6) tương ứng: Hôm nay -> 6 ngày trước
            for (int i = 0; i < 7; i++)
            {
                DateTime targetDate = today.AddDays(-i);

                // Tìm xem ngày này có trong dữ liệu SQL trả về không
                var stat = rawData.FirstOrDefault(x => x.Date.Date == targetDate);

                if (stat != null)
                {
                    // Nếu có trong DB -> Tính toán lại tên thứ cho chuẩn C# -> Thêm vào list
                    stat.DayName = targetDate.ToString("dddd", viCulture);
                    full7Days.Add(stat);
                }
                else
                {
                    // Nếu ngày này KHÔNG có trong DB (ví dụ Chủ nhật nghỉ làm)
                    // Tạo một đối tượng giả với toàn số 0
                    full7Days.Add(new DaylyAttendanceStat
                    {
                        Date = targetDate,
                        DayName = targetDate.ToString("dddd", viCulture), // C# tự sinh tên thứ
                        OnTimeCount = 0,
                        LateCount = 0,
                        AbsentCount = 0
                    });
                }
            }

            return full7Days; // List này luôn có đủ 7 phần tử, index 0 là hôm nay.
        }
        public List<int> GetTodayAttendanceStats()
        {
            // Khởi tạo danh sách kết quả mặc định là [0, 0, 0] để tránh lỗi nếu chưa có dữ liệu
            List<int> results = new List<int> { 0, 0, 0 };
            string connect = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            // Câu SQL của bạn
            string query = @"
                    SELECT 
                        SUM(CASE WHEN cc.trang_thai = 'checked_in' THEN 1 ELSE 0 END) AS dung_gio,
                        SUM(CASE WHEN cc.trang_thai = 'late' THEN 1 ELSE 0 END) AS di_muon,
                        SUM(CASE WHEN cc.trang_thai = 'absent' THEN 1 ELSE 0 END) AS vang
                    FROM cham_cong cc
                    JOIN ca_lam cl ON cc.ca_id = cl.id
                    WHERE cl.ngay_lam = CURDATE()
                    GROUP BY cl.ngay_lam";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connect))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Nếu có dữ liệu ngày hôm nay
                            {
                                // Xóa giá trị mặc định để nạp giá trị thật
                                results.Clear();

                                // Lưu ý: Hàm SUM trong MySQL thường trả về Decimal hoặc Int64
                                // Cần dùng Convert.ToInt32 để ép kiểu an toàn
                                int dungGio = reader["dung_gio"] != DBNull.Value ? Convert.ToInt32(reader["dung_gio"]) : 0;
                                int diMuon = reader["di_muon"] != DBNull.Value ? Convert.ToInt32(reader["di_muon"]) : 0;
                                int vang = reader["vang"] != DBNull.Value ? Convert.ToInt32(reader["vang"]) : 0;

                                results.Add(dungGio); // Index 0
                                results.Add(diMuon);  // Index 1
                                results.Add(vang);    // Index 2
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi (Log lỗi hoặc thông báo)
                Console.WriteLine("Lỗi truy vấn: " + ex.Message);
            }

            return results;
        }
        public List<WeeklyAttendanceStat> GetWeeklyStats()
        {
            List<WeeklyAttendanceStat> results = new List<WeeklyAttendanceStat>();
            string connect = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
            // Thay thông tin kết nối của bạn vào đây

            string query = @"
                    SELECT 
                        YEAR(cl.ngay_lam) AS nam,
                        WEEK(cl.ngay_lam, 1) AS tuan_so,
                        CONCAT(
                            DATE_FORMAT(DATE_ADD(MAX(cl.ngay_lam), INTERVAL(0 - WEEKDAY(MAX(cl.ngay_lam))) DAY), '%d/%m'), 
                            ' - ', 
                            DATE_FORMAT(DATE_ADD(MAX(cl.ngay_lam), INTERVAL(6 - WEEKDAY(MAX(cl.ngay_lam))) DAY), '%d/%m')
                        ) AS khoang_thoi_gian,
                        SUM(CASE WHEN cc.trang_thai = 'checked_in' THEN 1 ELSE 0 END) AS tong_dung_gio,
                        SUM(CASE WHEN cc.trang_thai = 'late' THEN 1 ELSE 0 END) AS tong_di_muon,
                        SUM(CASE WHEN cc.trang_thai = 'absent' THEN 1 ELSE 0 END) AS tong_vang
                    FROM cham_cong cc
                    JOIN ca_lam cl ON cc.ca_id = cl.id
                    WHERE YEARWEEK(cl.ngay_lam, 1) <= YEARWEEK(CURDATE(), 1)
                    GROUP BY YEAR(cl.ngay_lam), WEEK(cl.ngay_lam, 1)
                    ORDER BY nam DESC, tuan_so DESC
                    LIMIT 4; -- (Tùy chọn) Chỉ lấy 4 tuần gần nhất";

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
                                results.Add(new WeeklyAttendanceStat
                                {
                                    // Mapping dữ liệu từ SQL vào Class
                                    WeekRange = reader["khoang_thoi_gian"].ToString(),

                                    // Convert an toàn từ Decimal/Int64 sang Int
                                    OnTimeCount = Convert.ToInt32(reader["tong_dung_gio"]),
                                    LateCount = Convert.ToInt32(reader["tong_di_muon"]),
                                    AbsentCount = Convert.ToInt32(reader["tong_vang"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi hoặc log ra console
                Console.WriteLine(ex.Message);
            }
            System.Diagnostics.Debug.Write($"Debug: Lấy được {results} tuần dữ liệu.");
            return results; // Kết quả trả về đang là thứ tự: Tuần mới nhất -> Tuần cũ nhất
        }
        public int GetTotalEmployees()
        {
            int total = 0;
            string connect = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
            string query = "SELECT COUNT(*) \r\nFROM nhan_vien nv\r\nINNER JOIN tai_khoan tk \r\n    ON tk.id = nv.tai_khoan_id\r\n    AND tk.trang_thai = 'active';";
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connect))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        total = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi SQL: " + ex.Message);
            }
            return total;
        }
    }
}
