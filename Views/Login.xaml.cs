using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Security.Cryptography; // ✅ Thêm thư viện bảo mật
using System.Text;                  // ✅ Thêm thư viện xử lý chuỗi
using System.Windows;
using System.Windows.Input;
using QuanLyChamCong.Helpers;

namespace QuanLyChamCong.Views
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        // ✅ Hàm hỗ trợ mã hóa SHA-256
        public static string ComputeSha256Hash(string rawData)
        {
            // Tạo đối tượng SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // Chuyển chuỗi đầu vào thành mảng byte và tính toán hash
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Chuyển mảng byte thành chuỗi Hexadecimal (dạng chuỗi thường thấy trong DB)
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string user = SoDienThoai_text.Text.Trim();
            string passRaw = PassWord_text.Password.Trim(); // Lấy mật khẩu thô

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(passRaw))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tài khoản và mật khẩu.", "Thông báo");
                return;
            }

            // ✅ MÃ HÓA MẬT KHẨU TRƯỚC KHI GỬI ĐI
            string passHash = ComputeSha256Hash(passRaw);

            string constr = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            using (MySqlConnection con = new MySqlConnection(constr))
            {
                try
                {
                    con.Open();

                    // Câu truy vấn vẫn giữ nguyên so sánh 'mat_khau_hash'
                    string query = @"
                                    SELECT dn.ten_doanh_nghiep, dn.id
                                    FROM tai_khoan tk
                                    JOIN doanh_nghiep dn ON tk.id = dn.tai_khoan_chu_so_huu
                                    WHERE tk.so_dien_thoai = @user AND tk.mat_khau_hash = @pass AND tk.vai_tro = 'owner';";

                    string doanhNghiepTen = "";
                    using (MySqlCommand command = new MySqlCommand(query, con))
                    {
                        command.Parameters.AddWithValue("@user", user);

                        // ✅ Truyền mật khẩu ĐÃ MÃ HÓA vào tham số @pass
                        command.Parameters.AddWithValue("@pass", passHash);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read() == true)
                            {
                                MessageBox.Show("Đăng nhập thành công!", "Welcome");
                                DoanhNghiep.CurrentID = reader.GetInt32("id");

                                doanhNghiepTen = reader["ten_doanh_nghiep"].ToString();
                                MainWindow mainHome = new MainWindow(doanhNghiepTen);
                                mainHome.Show();
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
            this.Close();
        }

        private void SignUp_Click(object sender, MouseButtonEventArgs e)
        {
            SignUp signUp = new SignUp();
            signUp.Show();
            this.Close();
        }
    }
}