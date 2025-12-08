using CommunityToolkit.Mvvm.ComponentModel;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using QuanLyChamCong.Views;
using System.Windows;
namespace QuanLyChamCong.ViewModels
{
    internal partial class CaLamViewModel : ObservableObject
    {
        private readonly CaLamService _caLamService;
        private List<CaLamModel> _caLamListGoc = new List<CaLamModel>();
        [ObservableProperty]
        private ObservableCollection<CaLamModel> _danhSachCa = new ObservableCollection<CaLamModel>();
        [ObservableProperty]
        private string _searchKeyword;

        partial void OnSearchKeywordChanged(string value)
        {
            LocDuLieu();
        }
        [ObservableProperty]
        private DateTime? _selectedDay = DateTime.Now;
        partial void OnSelectedDayChanged(DateTime? value)
        {
            LocDuLieu();
        }

        public CaLamViewModel()
        {
            _caLamService = new CaLamService();
            _ = LoadDataAsync();
        }
        [RelayCommand]
        public async Task LoadDataAsync()
        {
            var data = await _caLamService.LayDanhSachCaAsync();

            _caLamListGoc = data;

            // Reset bộ lọc và hiển thị tất cả
            LocDuLieu();
        }
        [RelayCommand]
        public void ThemCa() { 
            var window = new ThemCaLam();
            bool? result = window.ShowDialog();
        }
        [RelayCommand]
        public void SuaCa(CaLamModel ca)
        {
            if (ca == null)
            {
                MessageBox.Show("Vui lòng chọn ca làm để sửa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            var window = new SuaCaLamView(ca);
            bool? result = window.ShowDialog();
        }
        [RelayCommand]
        public async Task XoaCa(CaLamModel ca)
        {
            if (ca == null)
            {
                MessageBox.Show("Vui lòng chọn ca làm để xóa.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var confirmResult = MessageBox.Show($"Bạn có chắc chắn muốn xóa ca '{ca.LoaiCa}' không?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmResult == MessageBoxResult.Yes)
            {
                bool isDeleted = await _caLamService.XoaCaAsync(ca.CaId);
                if (isDeleted)
                {
                    MessageBox.Show("Xóa ca làm thành công.", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    _ = LoadDataAsync();
                }
                else
                {
                    MessageBox.Show("Xóa ca làm thất bại. Vui lòng thử lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void LocDuLieu()
        {
            // Nếu chưa có dữ liệu gốc thì thoát
            if (_caLamListGoc == null || !_caLamListGoc.Any()) return;

            var query = _caLamListGoc.AsEnumerable();

            // 1. Lọc theo từ khóa (Mã hoặc Tên)
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                string kw = SearchKeyword.ToLower();
                query = query.Where(x => x.LoaiCa.ToLower().Contains(kw));
            }

            // 2. Lọc theo ngày (nếu có chọn)
            if (SelectedDay.HasValue)
            {
                query = query.Where(x => x.NgayLam.Date == SelectedDay.Value.Date);
            }

            // 3. Cập nhật lên UI
            // Lưu ý: Tạo ObservableCollection mới gán vào Property sẽ nhanh hơn loop Add từng cái
            DanhSachCa = new ObservableCollection<CaLamModel>(query.ToList());
        }

    }
}
