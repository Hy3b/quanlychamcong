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
            var newEmp = new nhan_vien
            {
                ho_ten = Hoten.Trim(),
                so_dien_thoai = Sodt.Trim(),
                chuc_vu = Chucvu.Trim(),
            };

            int currentDoanhNghiepId = DoanhNghiep.CurrentID;
            // 2. Kiểm tra dữ liệu (Validate)
            if (string.IsNullOrEmpty(newEmp.ho_ten) || string.IsNullOrEmpty(newEmp.so_dien_thoai) || string.IsNullOrEmpty(newEmp.chuc_vu))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // 3. Gọi Service để thêm (tương tự như code Sửa bạn đã làm)
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
                    MessageBox.Show("Thêm nhân viên thất bại. Vui lòng thử lại.");
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
