using System.Windows;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;

namespace QuanLyChamCong.Views
{
    public partial class EditEmployeeWindow : Window
    {
        public EmployeesModel Employee { get; set; }
        private readonly EmployeesService _service;
        // Constructor nhận vào nhân viên cần sửa
        public EditEmployeeWindow(EmployeesModel employeeToEdit)
        {
            InitializeComponent();
            _service = new EmployeesService();
            // Gán DataContext là chính nhân viên đó để các TextBox tự hiện dữ liệu
            Employee = employeeToEdit;
            this.DataContext = Employee;
        }
        private bool kiemtradauvao(EmployeesModel emp)
        {
            if (string.IsNullOrWhiteSpace(Employee.Name) || string.IsNullOrWhiteSpace(Employee.Department) || string.IsNullOrEmpty(emp.PhoneNumbers) || emp.PhoneNumbers.Length != 10 || !emp.PhoneNumbers.All(char.IsDigit))
            {
                MessageBox.Show("Trường thông tin có vẻ không chính xác! Vui lòng nhập lại đầy đủ");
                return false;
            }
            return true;
        }
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Tại đây bạn có thể gọi Service để UPDATE xuống Database
            if (!kiemtradauvao(Employee))//xác nhận dữ liệu đầu vào
            {
                return;
            }
            try
            {
                bool isSuccess = await _service.SuaNhanVien(Employee);

                if (isSuccess)
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo");
                    this.DialogResult = true; // Báo về cửa sổ cha là OK
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại. Có thể nhân viên không còn tồn tại.", "Lỗi");
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi: " + ex.Message, "Lỗi");
            }

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Đóng cửa sổ, không làm gì cả
            this.Close();
        }
    }
}