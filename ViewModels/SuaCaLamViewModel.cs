using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyChamCong.Helpers;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
namespace QuanLyChamCong.ViewModels
{
    public partial class SuaCaLamViewModel : ObservableObject
    {
        private readonly CaLamService _caLamService;
        private readonly ThemCaLamService _themCaLamService;
        [ObservableProperty]
        private ObservableCollection<NhanVienChonModel> _danhSachNhanVien;
        public CaLamModel CaCanSua { get; set; }

        public SuaCaLamViewModel(CaLamModel ca)
        {
            _caLamService = new CaLamService();
            _themCaLamService = new ThemCaLamService();
            this.CaCanSua = ca; // Clone object này để tránh sửa trực tiếp khi chưa bấm Lưu

            // 1. Lấy tất cả nhân viên
            var tatCaNhanVien = _themCaLamService.LayDanhSachNhanVien(DoanhNghiep.CurrentID);

            // 2. Lấy ID những nhân viên đang làm ca này
            var idNhanVienTrongCa = _themCaLamService.LayDanhSachNhanVien(DoanhNghiep.CurrentID, CaCanSua.CaId);
            var tapHopIdDaChon = new HashSet<int>(idNhanVienTrongCa.Select(x => x.Id));
            // 3. Map dữ liệu để hiển thị lên View
            foreach (var nv in tatCaNhanVien)
            {
                // Nếu ID nhân viên có trong danh sách phân công -> Đánh dấu chọn
                if (tapHopIdDaChon.Contains(nv.Id))
                {
                    nv.IsSelected = true;
                }
            }

            DanhSachNhanVien = new ObservableCollection<NhanVienChonModel>(tatCaNhanVien);
        }
        [RelayCommand]
        public async Task LuuThayDoi(Window window)
        {
            // 1. Validation cơ bản
            if (string.IsNullOrWhiteSpace(CaCanSua.LoaiCa))
            {
                MessageBox.Show("Vui lòng nhập tên ca!", "Thông báo");
                return;
            }
            // 2. Lấy danh sách nhân viên đã được tick chọn
            var nhanVienDaChon = DanhSachNhanVien.Where(nv => nv.IsSelected).ToList();
            // 3. Gọi Service lưu (Giả lập)
            bool res = await _caLamService.SuaCaAsync(CaCanSua, nhanVienDaChon);
            if (res)
            {
                MessageBox.Show($"Đã sửa ca '{CaCanSua.LoaiCa}' với {nhanVienDaChon.Count} nhân viên.", "Thành công");
                window.Close();
            }
            else
            {
                MessageBox.Show("Có lỗi xảy ra khi sửa ca.", "Lỗi");
            }
        }

    }
}
