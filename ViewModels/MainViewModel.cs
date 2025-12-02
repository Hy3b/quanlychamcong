using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace QuanLyChamCong.ViewModels
{
    // [ObservableObject] giúp nó có thể thông báo cho View khi có gì thay đổi
    public partial class MainViewModel : ObservableObject
    {
        // 1. Tạo các thể hiện (instances) của các ViewModel con
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly EmployeesViewModel _employeesViewModel;
        private readonly AttendanceViewModel _attendanceViewModel;
        // 2. Một property để lưu ViewModel *hiện tại* đang được hiển thị
        // [ObservableProperty] sẽ tự động tạo 1 property tên là CurrentViewModel
        [ObservableProperty]
        public object _currentViewModel;

        // 3. Các "Lệnh" (Command) mà các nút trong View có thể gọi
        public IRelayCommand ShowDashboardCommand { get; }
        public IRelayCommand ShowEmployeesCommand { get; }
        public IRelayCommand ShowAttendaceCommand { get; }
        // ... Thêm các Command khác cho Reports, Settings...

        public MainViewModel()
        {
            // Khởi tạo các ViewModel con
            _dashboardViewModel = new DashboardViewModel();
            _employeesViewModel = new EmployeesViewModel();
            _attendanceViewModel = new AttendanceViewModel();

            // Khởi tạo các Lệnh
            ShowDashboardCommand = new RelayCommand(ExecuteShowDashboard);
            ShowEmployeesCommand = new RelayCommand(ExecuteShowEmployees);
            ShowAttendaceCommand = new RelayCommand(ExecuteShowAttendace);
            // ...

            // 4. Thiết lập trang mặc định khi mở ứng dụng
            CurrentViewModel = _dashboardViewModel;
        }

        // 5. Các phương thức được gọi bởi Command
        private void ExecuteShowDashboard()
        {
            CurrentViewModel = _dashboardViewModel;
        }

        private void ExecuteShowEmployees()
        {
            CurrentViewModel = _employeesViewModel;
        }
        private void ExecuteShowAttendace()
        {
            CurrentViewModel = _attendanceViewModel;
        }

    }
}