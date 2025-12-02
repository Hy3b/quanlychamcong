using QuanLyChamCong.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QuanLyChamCong.Services;

namespace QuanLyChamCong.Views
{
    /// <summary>
    /// Interaction logic for AddEmployeeView.xaml
    /// </summary>
    public partial class AddEmployeeView : Window
    {
        private readonly EmployeesService _service = new EmployeesService();
        private async void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. Tạo đối tượng nhân viên mới từ dữ liệu nhập trên Form
            var newEmp = new nhan_vien
            {
                ho_ten = txtFullName.Text.Trim(),
                so_dien_thoai = txtPhone.Text.Trim(),
                chuc_vu = txtPosition.Text.Trim(),
            };

            int currentDoanhNghiepId = 1;
            // 2. Kiểm tra dữ liệu (Validate)
            if (string.IsNullOrEmpty(newEmp.ho_ten) || string.IsNullOrEmpty(newEmp.so_dien_thoai) || string.IsNullOrEmpty(newEmp.chuc_vu))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // 3. Gọi Service để thêm (tương tự như code Sửa bạn đã làm)
            try
            {
                bool isSuccess = await _service.ThemNhanVienVaTaiKhoan(newEmp, currentDoanhNghiepId);
                if (isSuccess)
                {
                    MessageBox.Show("Thêm nhân viên thành công!");
                    this.DialogResult = true;
                    this.Close(); // Đóng cửa sổ sau khi thêm thành công
                }
                else
                {
                    MessageBox.Show("Thêm nhân viên thất bại. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}");
            }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        public AddEmployeeView()
        {
            InitializeComponent();
        }
    }
}
