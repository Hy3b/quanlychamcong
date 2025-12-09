using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyChamCong.Helpers;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Text.RegularExpressions; // ✅ 1. Thêm thư viện này để dùng Regex

namespace QuanLyChamCong.ViewModels
{
    public partial class AddEmployeeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _hoten;
        [ObservableProperty]
        private string _sodt;
        [ObservableProperty]
        private string _chucvu;

        private readonly EmployeesService _service;

        public AddEmployeeViewModel()
        {
            _service = new EmployeesService();
        }

        [RelayCommand]
        public async void Luu(Window window)
        {
            // 1. Tạo đối tượng nhân viên mới từ dữ liệu nhập trên Form
            // Lưu ý: Nên kiểm tra null cho các biến bind (Hoten, Sodt...) trước khi Trim() để tránh lỗi nếu người dùng chưa nhập gì
            var newEmp = new nhan_vien
            {
                ho_ten = Hoten?.Trim() ?? "",
                so_dien_thoai = Sodt?.Trim() ?? "",
                chuc_vu = Chucvu?.Trim() ?? "",
            };

            int currentDoanhNghiepId = DoanhNghiep.CurrentID;

            // 2. Kiểm tra dữ liệu rỗng (Validate Empty)
            if (string.IsNullOrEmpty(newEmp.ho_ten) ||
                string.IsNullOrEmpty(newEmp.so_dien_thoai) ||
                string.IsNullOrEmpty(newEmp.chuc_vu))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // ✅ 3. Kiểm tra định dạng số điện thoại (Validate Format)
            // Pattern: Bắt đầu bằng số 0, theo sau là 9 chữ số bất kỳ (tổng 10 số)
            string pattern = @"^0\d{9}$";
            if (!Regex.IsMatch(newEmp.so_dien_thoai, pattern))
            {
                MessageBox.Show("Số điện thoại không hợp lệ! Vui lòng nhập 10 chữ số và bắt đầu bằng số 0.", "Cảnh báo");
                return;
            }

            // 4. Gọi Service để thêm
            try
            {
                bool isSuccess = await _service.ThemNhanVienVaTaiKhoan(newEmp);
                if (isSuccess)
                {
                    MessageBox.Show("Thêm nhân viên thành công!");
                    window.DialogResult = true;
                    window.Close(); // Đóng cửa sổ sau khi thêm thành công
                }
                else
                {
                    MessageBox.Show("Thêm nhân viên thất bại (Có thể SĐT đã tồn tại). Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        [RelayCommand]
        public void Huy(Window window)
        {
            window.Close();
        }
    }
}