using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input; // <-- Thêm dòng này

namespace QuanLyChamCong.Views
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string user = SoDienThoai_text.Text.Trim();
            string pass = PassWord_text.Password.Trim();

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu.", "Thông báo");
                return;
            }

            // ✅ Chuỗi kết nối MySQL — sử dụng tài khoản cố định (root)
            string constr = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            using (MySqlConnection con = new MySqlConnection(constr))
            {
                try
                {
                    con.Open();

                    // ✅ Câu truy vấn kiểm tra tài khoản
                    string query = @"
                                    SELECT dn.ten_doanh_nghiep
                                    FROM tai_khoan tk
                                    JOIN doanh_nghiep dn ON tk.id = dn.tai_khoan_chu_so_huu
                                    WHERE tk.so_dien_thoai = @user AND tk.mat_khau_hash = @pass AND (tk.vai_tro = 'owner' OR tk.vai_tro = 'admin')";
                    string doanhNghiepTen = "";
                    using (MySqlCommand command = new MySqlCommand(query, con))
                    {
                        
                        command.Parameters.AddWithValue("@user", user);
                        command.Parameters.AddWithValue("@pass", pass);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read() == true) // nếu có kết quả
                            {
                                
                                MessageBox.Show("Đăng nhập thành công!", "Welcome");
                                // đóng login
                                // 👉 Chuyển sang cửa sổ chính (ví dụ HomeWindow)
                                // HomeWindow home = new HomeWindow();
                                // home.Show();
                                // this.Close();
                                doanhNghiepTen = reader["ten_doanh_nghiep"].ToString();
                                MainWindow mainHome = new MainWindow(doanhNghiepTen);

                                // 2. Hiển thị trang chủ
                                mainHome.Show();

                                // 3. Đóng cửa sổ Login hiện tại lại
                                this.Close();
                            }
                            else
                            {
                                MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu.", "Lỗi");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message);
                }
            }
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        { 
            ForgotPassword f = new ForgotPassword();
            f.Show();
            this.Close(); // nếu muốn đóng màn login

        }
        private void SignUp_Click(object sender, MouseButtonEventArgs  e)
        {
            SignUp signUp = new SignUp();
            signUp.Show();
            this.Close(); // đóng cửa sổ login
        }


    }
}
