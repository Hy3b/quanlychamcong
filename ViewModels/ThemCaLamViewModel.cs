using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using QuanLyChamCong.Helpers;
namespace QuanLyChamCong.ViewModels
{
    public partial class ThemCaLamViewModel : ObservableObject
    {
        private readonly ThemCaLamService _service;
        [ObservableProperty]
        private string _tenCa;

        [ObservableProperty]
        private DateTime _ngayLam = DateTime.Today;

        [ObservableProperty]
        private string _gioBatDau = "08:00"; // Mặc định

        [ObservableProperty]
        private string _gioKetThuc = "17:00"; // Mặc định

        // Danh sách nhân viên để chọn (Checkbox)
        [ObservableProperty]
        private ObservableCollection<NhanVienChonModel> _danhSachNhanVien;

        public ThemCaLamViewModel()
        {
            _service = new ThemCaLamService();
            LoadNhanVien();
        }
        private void LoadNhanVien()
        {
            // Giả lập lấy danh sách nhân viên từ DB
            // Thực tế bạn sẽ gọi: await _nhanVienService.LayTatCaNhanVien();
            var ListNhanVien = _service.LayDanhSachNhanVien(DoanhNghiep.CurrentID);
            DanhSachNhanVien = new ObservableCollection<NhanVienChonModel>(ListNhanVien);
        }
        [RelayCommand]
        public async Task Luu(Window window)
        {
            // 1. Validation cơ bản
            if (string.IsNullOrWhiteSpace(TenCa))
            {
                MessageBox.Show("Vui lòng nhập tên ca!", "Thông báo");
                return;
            }

            // 2. Lấy danh sách nhân viên đã được tick chọn
            var nhanVienDaChon = DanhSachNhanVien.Where(nv => nv.IsSelected).ToList();

            // 3. Tạo Model để gửi xuống Service
            var caMoi = new CaLamModel
            {
                LoaiCa = TenCa,
                NgayLam = NgayLam,
                GioBatDau = TimeSpan.Parse(GioBatDau),
                GioKetThuc = TimeSpan.Parse(GioKetThuc)
            };

            // 4. Gọi Service lưu (Giả lập)
            bool res = await _service.ThemCaMoiAsync(caMoi, nhanVienDaChon);

            if (res)
            {
                MessageBox.Show($"Đã thêm ca '{TenCa}' với {nhanVienDaChon.Count} nhân viên.", "Thành công");
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi thêm ca.", "Lỗi");
            }

            // 5. Đóng cửa sổ sau khi lưu xong
            window?.Close();
        }

        // Logic Hủy
        [RelayCommand]
        public void Huy(Window window)
        {
            window?.Close();
        }
    }
}
