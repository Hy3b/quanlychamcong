using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using QuanLyChamCong.Views;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows; // Để dùng MessageBox báo lỗi

namespace QuanLyChamCong.ViewModels
{
    public partial class EmployeesViewModel : ObservableObject
    {
        private readonly EmployeesService _service;

        // Danh sách gốc (Toàn bộ nhân viên)
        [ObservableProperty]
        private ObservableCollection<EmployeesModel> employees;

        // Danh sách hiển thị (Đã lọc) -> Bind cái này ra DataGrid
        [ObservableProperty]
        private ObservableCollection<EmployeesModel> filteredEmployees;

        [ObservableProperty]
        string searchText;

        // Hàm này tự chạy khi SearchText thay đổi (Tính năng của CommunityToolkit)
        partial void OnSearchTextChanged(string value)
        {
            Search();
        }

        [RelayCommand]
        void Search()
        {
            // Nếu chưa có dữ liệu gốc thì thoát luôn
            if (Employees == null) return;

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Reset về danh sách gốc
                FilteredEmployees = new ObservableCollection<EmployeesModel>(Employees);
                return;
            }

            string text = SearchText.ToLower();

            // SỬA LỖI: Thêm kiểm tra null (e.Name?) để tránh crash
            var result = Employees.Where(e =>
                (e.Name?.ToLower().Contains(text) ?? false) ||
                (e.PhoneNumbers?.Contains(text) ?? false)
            );

            FilteredEmployees = new ObservableCollection<EmployeesModel>(result);
        }


        
        private async void LoadDataAsync()
        {
            try
            {
                // SỬA LỖI: Nên dùng ngày hiện tại hoặc truyền tham số
                DateTime selectedDate = DateTime.Today;

                var result = await _service.GetAll(selectedDate);

                // 1. Lưu vào danh sách gốc
                Employees = new ObservableCollection<EmployeesModel>(result);

                // 2. QUAN TRỌNG: Gán luôn vào danh sách hiển thị để UI hiện ngay lập tức
                FilteredEmployees = new ObservableCollection<EmployeesModel>(Employees);

                // 3. Nếu đang có chữ trong ô tìm kiếm thì lọc lại ngay
                if (!string.IsNullOrEmpty(SearchText))
                {
                    Search();
                }
            }
            catch (Exception ex)
            {
                // Bắt lỗi nếu kết nối DB hỏng
                MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message);
            }
        }

        [RelayCommand]
        private async Task DeleteEmployee(EmployeesModel employee)
        {
            if (employee == null) return;

            // Hỏi xác nhận
            var result = MessageBox.Show($"Bạn có chắc muốn xóa nhân viên {employee.EmployeeID} có tên là {employee.Name} ?",
                                         "Xác nhận xóa",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Gọi Service xóa trong Database (Bạn cần viết hàm này trong Service)
                    bool isDeleted = await _service.XoaNhanVien(employee.EmployeeID);
                    if (isDeleted)
                    {// Xóa trên giao diện
                        Employees.Remove(employee);
                        FilteredEmployees.Remove(employee);
                        MessageBox.Show("Đã xóa thành công!", "Thông báo");
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy nhân viên để xóa (Có thể đã bị xóa trước đó).", "Lỗi");
                    }

                }
                catch (Exception ex) {
                    MessageBox.Show("Lỗi: " + ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        [RelayCommand]
        private async Task EditEmployee(EmployeesModel employee)
        {
            if (employee == null) return;

            // Tạo một cửa sổ mới (Form Sửa) và truyền nhân viên vào
            // (Giả sử bạn đã tạo file EditEmployeeWindow.xaml)
            EmployeesModel employeeClone = new EmployeesModel
            {
                EmployeeID = employee.EmployeeID,
                Name = employee.Name,
                Department = employee.Department,
                PhoneNumbers = employee.PhoneNumbers,
                ShiftType = employee.ShiftType,
                TotalShiftsToday = employee.TotalShiftsToday,
                Status = employee.Status
            };
            var editWindow = new EditEmployeeWindow(employeeClone);

            bool? result = editWindow.ShowDialog(); // Hiện dạng Dialog (chặn cửa sổ chính)

            if (result == true)
            {
                // Nếu người dùng bấm Lưu bên kia, ta load lại dữ liệu hoặc cập nhật dòng đó
             
                    LoadDataAsync();
                
            }
        }
        [RelayCommand]
        private void AddEmployee()
        {
            // Tạo một cửa sổ mới (Form Thêm) 
            var addWindow = new AddEmployeeView();
            bool? result = addWindow.ShowDialog(); // Hiện dạng Dialog (chặn cửa sổ chính)
            if (result == true)
            {
                // Nếu người dùng bấm Lưu bên kia, ta load lại dữ liệu
                LoadDataAsync();
            }
        }
        [RelayCommand]
        private void Refresh()
        {
            LoadDataAsync();
        }
        /// <summary>
        // Hàm khởi tạo
        /// </summary>
        public EmployeesViewModel()
        {
            _service = new EmployeesService();
            // Khởi tạo list rỗng để tránh lỗi binding ban đầu
            Employees = new ObservableCollection<EmployeesModel>();
            FilteredEmployees = new ObservableCollection<EmployeesModel>();

            LoadDataAsync();
        }
    }
}
