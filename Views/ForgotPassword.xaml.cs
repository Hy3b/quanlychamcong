using MySql.Data.MySqlClient;
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

namespace QuanLyChamCong.Views

{
    /// <summary>
    /// Interaction logic for ForgetPassword.xaml
    /// </summary>
    public partial class ForgotPassword : Window
    {
        public ForgotPassword()
        {
            InitializeComponent();
        }

        private void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string phone = txtPhone.Text.Trim();

            if (string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Vui lòng nhập số điện thoại!", "Thông báo");
                return;
            }

            try
            {
                string connectionString = "Server=localhost;Database=chamcong;User ID=root;Password=tien0399007905";
                MySqlConnection conn = new MySqlConnection(connectionString);
                conn.Open();

                string query = "SELECT * FROM tai_khoan WHERE so_dien_thoai = @phone";
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@phone", phone);

                MySqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    MessageBox.Show("Vui lòng kiểm tra điện thoại của bạn!", "Thành công");

                    // ✔ Sau này bạn có thể thêm chức năng gửi mã OTP tại đây
                    MainWindow main = new MainWindow();
                    main.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Số điện thoại chưa được đăng kí!", "Lỗi");
                }

                conn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message);
            }

        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Mở lại Login Window
            MainWindow loginWindow = new MainWindow(); // Login.xaml phải tồn tại
            loginWindow.Show();

            // Đóng ForgotPassword hiện tại
            this.Close();
        }
    }
}

