using CommunityToolkit.Mvvm.ComponentModel; // <--- QUAN TRỌNG: Phải có cái này
using CommunityToolkit.Mvvm.Input;
using MySql.Data.MySqlClient;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using QuanLyChamCong.Views;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows; // Để dùng MessageBox
namespace QuanLyChamCong.ViewModels
{
    // 1. Thêm 'public partial' và kế thừa ': ObservableObject'
    public partial class SalaryDayViewModel : ObservableObject
    {
        /// <summary>
       
        /// khai báo dữ liệu trong SalarySeRVice
        private readonly SalaryDayService _salaryService;
        // TÌm kiếm theo giờ
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                // --- SỬA Ở ĐÂY ---
                // Nếu value là null (người dùng xóa trắng), tự động gán về DateTime.Now
                _selectedDate = value ?? DateTime.Now;

                OnPropertyChanged(nameof(SelectedDate));

                // Tự động tải lại dữ liệu theo ngày vừa gán
                LoadDataFromMySQL();
            }
        } 
        //Thanh tìm kiếm
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));

                // Khi gõ chữ, tự động gọi hàm tải lại dữ liệu để lọc ngay
                LoadDataFromMySQL();
            }
        }
        public ObservableCollection<SalaryDay> salaryForms { get; set; }

        private SalaryDay _selectedSalaryItem;
        public SalaryDay SelectedSalaryItem
        {
            get { return _selectedSalaryItem; }
            set
            {
                _selectedSalaryItem = value;
                // Hàm này chỉ chạy được khi đã kế thừa ObservableObject
                OnPropertyChanged(nameof(SelectedSalaryItem));
            }
        }

        public IRelayCommand EditSalaryCommand { get; }
        public IRelayCommand ShowHistoryCommand { get; }
        [RelayCommand]
        public void Refresh()
        {
            LoadDataFromMySQL();
        }
        public SalaryDayViewModel()
        {
            _salaryService = new SalaryDayService(); 
            salaryForms = new ObservableCollection<SalaryDay>();
            EditSalaryCommand = new RelayCommand(ExecuteEditSalary);
            ShowHistoryCommand = new RelayCommand(ExecuteShowHistory);
            SelectedDate = DateTime.Now;
            //LoadDataFromMySQL();
        }
        private void ExecuteShowHistory()
        {
            // Tạo ViewModel cho lịch sử
            var historyVM = new MonthlySalaryViewModel();

            // Tạo Window và gán DataContext
            MonthlySalaryWindow historyWindow = new MonthlySalaryWindow();
            historyWindow.DataContext = historyVM;

            // Hiện cửa sổ
            historyWindow.ShowDialog();
        }
        private async void LoadDataFromMySQL()
        {
            try
            {
                // Gọi Service để lấy List danh sách
                var listData = await _salaryService.GetDailySalaries(SearchText, SelectedDate);

                // Xóa dữ liệu cũ trên giao diện
                salaryForms.Clear();

                // Đổ dữ liệu mới vào
                foreach (var item in listData)
                {
                    salaryForms.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi kết nối");
            }
        }
        

        private void ExecuteEditSalary()
        {
            if (SelectedSalaryItem == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!", "Thông báo");
                return;
            }

            // Tạo ViewModel và Window sửa
            var editVM = new EditSalaryViewModel(SelectedSalaryItem);
            EditSalaryWindow editWindow = new EditSalaryWindow();
            editWindow.DataContext = editVM;

            // Setup hành động đóng cửa sổ
            editVM.CloseAction = () => editWindow.Close();

            // Hiện cửa sổ
            editWindow.ShowDialog();

            // Tải lại dữ liệu sau khi sửa xong
            LoadDataFromMySQL();
        }
    }
}