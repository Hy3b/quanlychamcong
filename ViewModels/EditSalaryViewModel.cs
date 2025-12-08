using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyChamCong.Models;
using System;
using System.Configuration;
using System.Windows;
using MySql.Data.MySqlClient;
using QuanLyChamCong.Services; // Nhớ using namespace Service
namespace QuanLyChamCong.ViewModels
{
    public partial class EditSalaryViewModel : ObservableObject
    {
        private readonly SalaryDayService _salaryService;
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
            // Khởi tạo Service
            _salaryService = new SalaryDayService();

            _nhanVienId = salaryItem.NhanVienId;
            _hoTen = salaryItem.HoTen;
            _luongCoBan = salaryItem.LuongCoBanNgay;
            _phuCap = salaryItem.PhuCap;
            _luongTangCa = salaryItem.LuongTangCa;
            _truThue = salaryItem.TruThue;
        }

        [RelayCommand]
        private async Task Save() // Chuyển thành async Task
        {
            try
            {
                // Gọi Service để lưu dữ liệu
                // Lưu ý: Dùng Property viết hoa (LuongCoBan, PhuCap...) để lấy giá trị mới nhất trên giao diện
                bool isSuccess = await _salaryService.UpdateDailySalary(
                    _nhanVienId,
                    LuongCoBan,
                    PhuCap,
                    LuongTangCa,
                    TruThue
                );

                if (isSuccess)
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo");
                    CloseAction?.Invoke();
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu nào thay đổi hoặc lỗi ID.", "Cảnh báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cập nhật thất bại: " + ex.Message, "Lỗi");
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseAction?.Invoke();
        }

    }
}