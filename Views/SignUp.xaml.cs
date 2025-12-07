using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;
namespace QuanLyChamCong.Views
{
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
        }
        public bool KiemTraSoDienThoai(string sdt)
        {
            // Quy tắc: 
            // ^0     : Bắt đầu bằng số 0
            // \d{9}  : Theo sau là 9 chữ số bất kỳ (tổng cộng là 10 số)
            // $      : Kết thúc chuỗi
            string pattern = @"^0\d{9}$";

            // Nếu bạn muốn chặt chẽ hơn (chỉ nhận đầu số nhà mạng VN thực tế: 03, 05, 07, 08, 09)
            // string pattern = @"^(03|05|07|08|09)\d{8}$";

            return Regex.IsMatch(sdt, pattern);
        }
        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            // ======================================================
            // PHẦN 1: LẤY DỮ LIỆU VÀ KIỂM TRA (VALIDATION)
            // ======================================================

            string soDienThoai = txtSoDienThoai.Text.Trim();
            string matKhau = txtPass.Password;
            string confirm = txtConfirm.Password;
            string tenDN = txtName.Text.Trim();
            if (!KiemTraSoDienThoai(soDienThoai))
            {
                MessageBox.Show("Số điện thoại không hợp lệ! Phải là 10 số và bắt đầu bằng số 0.");
                return;
            }
            // Lấy text hiển thị trong ComboBox (ví dụ: "Chủ doanh nghiệp")
            // Sử dụng toán tử ?. để tránh lỗi nếu người dùng chưa chọn gì
            string selectedText = (cbVaiTro.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Kiểm tra rỗng
            if (string.IsNullOrEmpty(soDienThoai) || string.IsNullOrEmpty(matKhau) ||
                string.IsNullOrEmpty(confirm) || string.IsNullOrEmpty(tenDN) ||
                string.IsNullOrEmpty(selectedText))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // Kiểm tra mật khẩu khớp nhau
            if (matKhau != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            // Xử lý Logic Vai Trò (Mapping từ Tiếng Việt sang Database)
            string vaiTroDb = "";
            if (selectedText == "Chủ doanh nghiệp")
            {
                vaiTroDb = "owner";
            }
            else if (selectedText == "Quản lý")
            {
                vaiTroDb = "admin";
            }
            else
            {
                vaiTroDb = "staff";
            }

            // ======================================================
            // PHẦN 2: XỬ LÝ DATABASE (TRANSACTION)
            // ======================================================

            string constr = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(constr))
            {
                try
                {
                    conn.Open();

                    // BẮT ĐẦU GIAO DỊCH (Transaction)
                    // Mọi lệnh trong khối này là một thể thống nhất.
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            long doanhNghiepId = 0;
                            long taiKhoanId = 0;

                            // ---------------------------------------------------------
                            // BƯỚC 1: INSERT DOANH NGHIỆP (Tạo công ty trước)
                            // Lúc này 'tai_khoan_chu_so_huu' sẽ là NULL
                            // ---------------------------------------------------------
                            string insertDN = "INSERT INTO doanh_nghiep (ten_doanh_nghiep) VALUES (@tenDN)";
                            using (MySqlCommand cmd = new MySqlCommand(insertDN, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@tenDN", tenDN);
                                cmd.ExecuteNonQuery();

                                // Lấy ID doanh nghiệp vừa tự sinh ra (Ví dụ: 101)
                                doanhNghiepId = cmd.LastInsertedId;
                            }

                            // ---------------------------------------------------------
                            // BƯỚC 2: INSERT TÀI KHOẢN (Gắn vào công ty vừa tạo)
                            // ---------------------------------------------------------
                            string insertTK = @"INSERT INTO tai_khoan 
                                               (doanh_nghiep_id, so_dien_thoai, mat_khau_hash, vai_tro, trang_thai)
                                               VALUES (@dnId, @soDT, @mk, @vaiTro, 'active')";

                            using (MySqlCommand cmd = new MySqlCommand(insertTK, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@dnId", doanhNghiepId); // Truyền ID 101 vào
                                cmd.Parameters.AddWithValue("@soDT", soDienThoai);

                                // Lưu ý: Thực tế nên mã hóa mật khẩu (SHA256) trước khi truyền vào đây
                                cmd.Parameters.AddWithValue("@mk", matKhau);
                                cmd.Parameters.AddWithValue("@vaiTro", vaiTroDb); // owner hoặc admin

                                cmd.ExecuteNonQuery();

                                // Lấy ID tài khoản vừa tạo (Ví dụ: 50)
                                taiKhoanId = cmd.LastInsertedId;
                            }

                            // ---------------------------------------------------------
                            // BƯỚC 3: UPDATE NGƯỢC LẠI DOANH NGHIỆP (Xác nhận chủ sở hữu)
                            // Chỉ thực hiện nếu vai trò là 'owner'
                            // ---------------------------------------------------------
                            if (vaiTroDb == "owner")
                            {
                                string updateDN = "UPDATE doanh_nghiep SET tai_khoan_chu_so_huu = @tkId WHERE id = @dnId";
                                using (MySqlCommand cmd = new MySqlCommand(updateDN, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@tkId", taiKhoanId); // ID 50
                                    cmd.Parameters.AddWithValue("@dnId", doanhNghiepId); // ID 101
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // ---------------------------------------------------------
                            // HOÀN TẤT: Lưu mọi thay đổi vào Database
                            // ---------------------------------------------------------
                            transaction.Commit();

                            MessageBox.Show("Đăng ký thành công!");

                            // Chuyển sang màn hình Login
                            Login main = new Login();
                            main.Show();
                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            // Nếu có lỗi ở bất kỳ bước nào (1, 2 hoặc 3) -> Hủy bỏ tất cả
                            transaction.Rollback();
                            MessageBox.Show("Lỗi trong quá trình xử lý đăng ký (đã hoàn tác): " + ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi kết nối CSDL: " + ex.Message);
                }
            }
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Mở lại Login Window
            Login loginWindow = new Login(); // Login.xaml phải tồn tại
            loginWindow.Show();

            // Đóng ForgotPassword hiện tại
            this.Close();
        }
    }
}