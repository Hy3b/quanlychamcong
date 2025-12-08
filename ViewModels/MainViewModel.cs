using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Org.BouncyCastle.Asn1.X509;

namespace QuanLyChamCong.ViewModels
{
    // [ObservableObject] giúp nó có thể thông báo cho View khi có gì thay đổi
    public partial class MainViewModel : ObservableObject
    {
        // 1. Tạo các thể hiện (instances) của các ViewModel con
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ShortName))]
        private string _ownerName;

        // ShortName viết dạng Read-only (chỉ đọc), tự tính toán dựa trên OwnerName
        public string ShortName => !string.IsNullOrEmpty(OwnerName)
                                   ? OwnerName[0].ToString().ToUpper()
                                   : "A";
        private readonly DashboardViewModel _dashboardViewModel;
        private readonly EmployeesViewModel _employeesViewModel;
        private readonly AttendanceViewModel _attendanceViewModel;
        private readonly CaLamViewModel _caLamViewModel;
        // 2. Một property để lưu ViewModel *hiện tại* đang được hiển thị
        // [ObservableProperty] sẽ tự động tạo 1 property tên là CurrentViewModel
        [ObservableProperty]
        private object _currentViewModel;

        // 3. Các "Lệnh" (Command) mà các nút trong View có thể gọi
        public IRelayCommand ShowDashboardCommand { get; }
        public IRelayCommand ShowEmployeesCommand { get; }


        public IRelayCommand ShowAttendanceCommand { get; }
        public IRelayCommand ShowCaLamCommand { get; }
        // ... Thêm các Command khác cho Reports, Settings...

        public MainViewModel()
        {
            // Khởi tạo các ViewModel con
            _dashboardViewModel = new DashboardViewModel();
            _employeesViewModel = new EmployeesViewModel();
            _attendanceViewModel = new AttendanceViewModel();
            _caLamViewModel = new CaLamViewModel();
            // Khởi tạo các Lệnh
            ShowDashboardCommand = new RelayCommand(ExecuteShowDashboard);
            ShowEmployeesCommand = new RelayCommand(ExecuteShowEmployees);
            ShowAttendanceCommand = new RelayCommand(ExecuteShowAttendance);
            ShowCaLamCommand = new RelayCommand(ExecuteShowCaLam);

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
        private void ExecuteShowAttendance()
        {
            CurrentViewModel = _attendanceViewModel;
        }
        private void ExecuteShowCaLam()
        {
            CurrentViewModel = _caLamViewModel;
        }
    }
}