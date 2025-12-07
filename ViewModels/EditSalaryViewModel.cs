using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyChamCong.Models;
using System;
using System.Configuration;
using System.Windows;
using MySql.Data.MySqlClient;

namespace QuanLyChamCong.ViewModels
{
    public partial class EditSalaryViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _hoTen;

        [ObservableProperty]
        private decimal _luongCoBan;

        [ObservableProperty]
        private decimal _phuCap;

        [ObservableProperty]
        private decimal _luongTangCa;

        // 1. THÊM TRỪ THUẾ VÀO ĐÂY
        [ObservableProperty]
        private decimal _truThue;

        private string _nhanVienId;
        public Action CloseAction { get; set; }

        public EditSalaryViewModel(SalaryDay salaryItem)
        {
            _nhanVienId = salaryItem.NhanVienId;
            _hoTen = salaryItem.HoTen;
            _luongCoBan = salaryItem.LuongCoBanNgay;
            _phuCap = salaryItem.PhuCap;
            _luongTangCa = salaryItem.LuongTangCa;

            // Lấy dữ liệu thuế cũ lên
            _truThue = salaryItem.TruThue;
        }

        [RelayCommand]
        private void Save()
        {
            if (UpdateSalaryToMySQL())
            {
                MessageBox.Show("Cập nhật thành công!", "Thông báo");
                CloseAction?.Invoke();
            }
            else
            {
                MessageBox.Show("Cập nhật thất bại!", "Lỗi");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseAction?.Invoke();
        }

        private bool UpdateSalaryToMySQL()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // CÂU LỆNH SQL ĐÃ CHUẨN HÓA TÊN CỘT:
                    // 1. luong_co_ban_ngay (Đã sửa theo bạn báo)
                    // 2. Tự động tính thuc_linh_ngay = (Lương + Phụ cấp + Tăng ca) - Thuế

                    string query = @"UPDATE luong_ngay 
                             SET luong_co_ban_ngay = @luong, 
                                 phu_cap = @phuCap, 
                                 luong_tang_ca = @tangCa,
                                 tru_thue = @thue,
                                 thuc_linh_ngay = (@luong + @phuCap*500 + @tangCa) - @thue
                             WHERE nhan_vien_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Truyền tham số (Dùng Property viết hoa)
                        cmd.Parameters.AddWithValue("@luong", LuongCoBan);
                        cmd.Parameters.AddWithValue("@phuCap", PhuCap);
                        cmd.Parameters.AddWithValue("@tangCa", LuongTangCa);
                        cmd.Parameters.AddWithValue("@thue", TruThue);

                        // ID dùng để tìm dòng cần sửa (WHERE)
                        cmd.Parameters.AddWithValue("@id", _nhanVienId);

                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi SQL: " + ex.Message);
                    return false;
                }
            }
        }
    }
}