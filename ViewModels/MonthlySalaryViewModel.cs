using CommunityToolkit.Mvvm.ComponentModel;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq; // <--- 1. BẮT BUỘC PHẢI CÓ DÒNG NÀY ĐỂ TẠO DANH SÁCH NĂM/THÁNG
using System.Windows;

namespace QuanLyChamCong.ViewModels
{
    public partial class MonthlySalaryViewModel : ObservableObject
    {
        private readonly SalaryMonthService _service;
        public ObservableCollection<MonthlySalary> MonthlyList { get; set; }

        // --- 2. THÊM MỚI: Danh sách để đổ vào ComboBox ---
        public ObservableCollection<int> ListMonths { get; set; } // Danh sách 1 -> 12
        public ObservableCollection<int> ListYears { get; set; }  // Danh sách năm (VD: 2020 -> 2035)

        // --- 3. THÊM MỚI: Biến lưu số Tháng và số Năm người dùng chọn ---
        private int _selectedMonthInt;
        public int SelectedMonthInt
        {
            get => _selectedMonthInt;
            set
            {
                if (_selectedMonthInt != value)
                {
                    _selectedMonthInt = value;
                    OnPropertyChanged(nameof(SelectedMonthInt));
                    UpdateSelectedDate(); // Chọn tháng xong -> Tự cập nhật ngày lọc
                }
            }
        }

        private int _selectedYearInt;
        public int SelectedYearInt
        {
            get => _selectedYearInt;
            set
            {
                if (_selectedYearInt != value)
                {
                    _selectedYearInt = value;
                    OnPropertyChanged(nameof(SelectedYearInt));
                    UpdateSelectedDate(); // Chọn năm xong -> Tự cập nhật ngày lọc
                }
            }
        }

        // --- 4. Biến ngày cũ (Giữ nguyên logic để gọi Service) ---
        // Biến này sẽ được hàm UpdateSelectedDate tự động gán giá trị
        private DateTime? _selectedMonth;
        public DateTime? SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                if (_selectedMonth != value)
                {
                    _selectedMonth = value;
                    OnPropertyChanged(nameof(SelectedMonth));
                    LoadHistoryFromMySQL(); // Khi biến này đổi -> Gọi SQL tải dữ liệu
                }
            }
        }

        // Hàm gộp Tháng + Năm thành Ngày để lọc
        private void UpdateSelectedDate()
        {
            try
            {
                // Tạo ngày mùng 1 của tháng/năm đã chọn
                // Ví dụ: Chọn tháng 12, năm 2025 -> Tạo ra ngày 01/12/2025
                SelectedMonth = new DateTime(SelectedYearInt, SelectedMonthInt, 1);
            }
            catch
            {
                // Bỏ qua lỗi nếu ngày không hợp lệ
            }
        }

        [ObservableProperty]
        private decimal _tongThucLinhToanBo;

        public MonthlySalaryViewModel()
        {
            _service = new SalaryMonthService();
            MonthlyList = new ObservableCollection<MonthlySalary>();

            // --- 5. KHỞI TẠO DỮ LIỆU CHO COMBOBOX ---
            // Tạo danh sách tháng 1 đến 12
            ListMonths = new ObservableCollection<int>(Enumerable.Range(1, 12));

            // Tạo danh sách năm từ 2020, lấy 20 năm tiếp theo
            ListYears = new ObservableCollection<int>(Enumerable.Range(2020, 20));

            // Mặc định chọn tháng/năm đã xuất trên giao diện
            DateTime lastMonth = DateTime.Now.AddMonths(-1);
            SelectedMonthInt = lastMonth.Month;
            SelectedYearInt = lastMonth.Year;

            // Dòng này kích hoạt hàm UpdateSelectedDate -> Gán SelectedMonth -> Gọi LoadHistoryFromMySQL
            UpdateSelectedDate();
        }

        private async void LoadHistoryFromMySQL()
        {
            try
            {
                var resultList = await _service.GetMonthlyHistory(SelectedMonth);

                MonthlyList.Clear();
                foreach (var item in resultList)
                {
                    MonthlyList.Add(item);
                }

                // Tính tổng tiền
                // (Lưu ý: Phải có 'using System.Linq' mới dùng được hàm .Sum() này)
                TongThucLinhToanBo = MonthlyList.Sum(x => x.ThucLinhThang);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải lịch sử lương: " + ex.Message, "Lỗi kết nối");
            }
        }
    }
}