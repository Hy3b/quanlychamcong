using CommunityToolkit.Mvvm.ComponentModel;
using MySql.Data.MySqlClient;
using QuanLyChamCong.Models;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;

namespace QuanLyChamCong.ViewModels
{
    public partial class MonthlySalaryViewModel : ObservableObject
    {
        public ObservableCollection<MonthlySalary> MonthlyList { get; set; }

        public MonthlySalaryViewModel()
        {
            MonthlyList = new ObservableCollection<MonthlySalary>();
            LoadHistoryFromMySQL();
        }

        private void LoadHistoryFromMySQL()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // JOIN bảng luong_thang với nhan_vien để lấy Họ Tên
                    string query = @"
                        SELECT 
                            lt.*, 
                            nv.ho_ten 
                        FROM luong_thang lt
                        JOIN nhan_vien nv ON lt.nhan_vien_id = nv.id
                        ORDER BY lt.thang DESC, lt.nhan_vien_id ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MonthlySalary item = new MonthlySalary();
                                item.Id = Convert.ToInt32(reader["id"]);
                                item.NhanVienId = reader["nhan_vien_id"].ToString();
                                item.HoTen = reader["ho_ten"].ToString();
                                item.Thang = reader["thang"].ToString();

                                item.TongLuongCoBan = Convert.ToDecimal(reader["tong_luong_co_ban"]);
                                item.TongPhuCap = Convert.ToDecimal(reader["tong_phu_cap"]);
                                item.TongTangCa = Convert.ToDecimal(reader["tong_tang_ca"]);
                                item.TongTruBaoHiem = Convert.ToDecimal(reader["tong_tru_bao_hiem"]);
                                item.TongTruThue = Convert.ToDecimal(reader["tong_tru_thue"]);
                                item.ThucLinhThang = Convert.ToDecimal(reader["thuc_linh_thang"]);

                                item.NgayChotLuong = reader["ngay_chot_luong"].ToString();

                                MonthlyList.Add(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi tải lịch sử lương: " + ex.Message);
                }
            }
        }
    }
}