using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using QuanLyChamCong.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
namespace QuanLyChamCong.ViewModels
{
    internal partial class AttendanceViewModel : ObservableObject
    {
        private readonly AttendanceService _service;
        [ObservableProperty]
        private string _searchKeyword;
        public List<AttendanceModel> _originalData = new List<AttendanceModel>();
        [ObservableProperty]
        private ObservableCollection<AttendanceModel> _dailyAttendance;
        [ObservableProperty]
        private DateTime? _selectedDate = DateTime.Now;
        //
        public AttendanceViewModel()
        {
            _service = new AttendanceService();
            DailyAttendance = new ObservableCollection<AttendanceModel>();
            LoadData();
        }
        // Xem lịch sử chấm công của nhân viên
        [RelayCommand]
        void ViewAttendanceHistory(AttendanceModel attendance)
        {
            if (attendance == null) return;
            var attendanceHistoryView = new AttendanceHistoryView(attendance);
            attendanceHistoryView.ShowDialog();
        }
        partial void OnSearchKeywordChanged(string value)
        {
            ApplyFilter();
        }
        partial void OnSelectedDateChanged(DateTime? value)
        {
            // 2. Nếu value là null (do người dùng xóa text), gán lại ngày hiện tại ngay lập tức
            if (value == null)
            {
                SelectedDate = DateTime.Now;
                // Khi gán lại, hàm này sẽ chạy lại lần nữa với value = DateTime.Now
                // nên code sẽ nhảy xuống phần 'else' bên dưới để LoadData.
            }
            else
            {
                // 3. Nếu có ngày hợp lệ, tiến hành tải dữ liệu
                // Vì value là nullable nên cần .Value để lấy DateTime thực
                LoadData();
            }
        }
        public void LoadData()
        {
            // CÁCH SỬA:
            // Dùng (SelectedDate ?? DateTime.Now)
            // Nghĩa là: Nếu SelectedDate có dữ liệu thì lấy, nếu là null thì lấy DateTime.Now

            DateTime dateToSearch = SelectedDate ?? DateTime.Now;

            // Bước 1: Gọi Service với biến dateToSearch (đã đảm bảo không null)
            _originalData = _service.GetDailyAttendance(dateToSearch);

            // Bước 2: Áp dụng bộ lọc
            ApplyFilter();
        }
        private void ApplyFilter()
        {
            // Nếu danh sách gốc chưa có dữ liệu thì thôi
            if (_originalData == null) return;

            IEnumerable<AttendanceModel> query = _originalData;

            // Lọc theo từ khóa (Nếu có nhập)
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
            {
                string keyword = SearchKeyword.ToLower();
                // Giả sử lọc theo Tên hoặc Mã nhân viên
                query = query.Where(x => x.HoTen.ToLower().Contains(keyword) ||
                                         x.NhanVienId.ToString().Contains(keyword));
            }

            // Cập nhật lại danh sách hiển thị
            DailyAttendance.Clear();
            foreach (var item in query)
            {
                DailyAttendance.Add(item);
            }
        }
        public void RefreshCommand()
        {
            LoadData();
        }

    }
}
