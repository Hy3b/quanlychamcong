using MySql.Data.MySqlClient;
using System;
using System.Configuration;
using System.Windows;
using System.Text.RegularExpressions;
using System.Security.Cryptography; // ✅ Thêm thư viện bảo mật
using System.Text;                  // ✅ Thêm thư viện xử lý chuỗi

namespace QuanLyChamCong.Views
{
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
        }

        // ✅ Hàm hỗ trợ mã hóa SHA-256 (Phải giống hệt bên Login)
        public static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public bool KiemTraSoDienThoai(string sdt)
        {
            // Quy tắc: Bắt đầu bằng 0, theo sau là 9 chữ số
            string pattern = @"^0\d{9}$";
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

            // 1. Kiểm tra rỗng trước
            if (string.IsNullOrEmpty(soDienThoai) || string.IsNullOrEmpty(matKhau) ||
                string.IsNullOrEmpty(confirm) || string.IsNullOrEmpty(tenDN))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            // 2. Kiểm tra định dạng số điện thoại
            if (!KiemTraSoDienThoai(soDienThoai))
            {
                MessageBox.Show("Số điện thoại không hợp lệ! Phải là 10 số và bắt đầu bằng số 0.");
                return;
            }

            // 3. Kiểm tra mật khẩu khớp nhau
            if (matKhau != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            // ✅ MÃ HÓA MẬT KHẨU NGAY SAU KHI KIỂM TRA HỢP LỆ
            string matKhauDaHash = ComputeSha256Hash(matKhau);


            // ======================================================
            // PHẦN 2: XỬ LÝ DATABASE (TRANSACTION)
            // ======================================================

            string constr = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(constr))
            {
                try
                {
                    conn.Open();

                    // BẮT ĐẦU GIAO DỊCH
                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            long doanhNghiepId = 0;
                            long taiKhoanId = 0;

                            // ---------------------------------------------------------
                            // BƯỚC 1: INSERT DOANH NGHIỆP
                            // ---------------------------------------------------------
                            string insertDN = "INSERT INTO doanh_nghiep (ten_doanh_nghiep) VALUES (@tenDN)";
                            using (MySqlCommand cmd = new MySqlCommand(insertDN, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@tenDN", tenDN);
                                cmd.ExecuteNonQuery();
                                doanhNghiepId = cmd.LastInsertedId;
                            }

                            // ---------------------------------------------------------
                            // BƯỚC 2: INSERT TÀI KHOẢN
                            // ---------------------------------------------------------
                            string insertTK = @"INSERT INTO tai_khoan 
                                                (doanh_nghiep_id, so_dien_thoai, mat_khau_hash, vai_tro, trang_thai)
                                                VALUES (@dnId, @soDT, @mk, 'owner', 'active')";

                            using (MySqlCommand cmd = new MySqlCommand(insertTK, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@dnId", doanhNghiepId);
                                cmd.Parameters.AddWithValue("@soDT", soDienThoai);

                                // ✅ Dùng mật khẩu đã Hash để lưu vào DB
                                cmd.Parameters.AddWithValue("@mk", matKhauDaHash);

                                cmd.ExecuteNonQuery();
                                taiKhoanId = cmd.LastInsertedId;
                            }

                            // ---------------------------------------------------------
                            // BƯỚC 3: UPDATE NGƯỢC LẠI DOANH NGHIỆP
                            // ---------------------------------------------------------
                            string updateDN = "UPDATE doanh_nghiep SET tai_khoan_chu_so_huu = @tkId WHERE id = @dnId";
                            using (MySqlCommand cmd = new MySqlCommand(updateDN, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@tkId", taiKhoanId);
                                cmd.Parameters.AddWithValue("@dnId", doanhNghiepId);
                                cmd.ExecuteNonQuery();
                            }

                            // ---------------------------------------------------------
                            // HOÀN TẤT
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
                            // Nếu có lỗi -> Hủy bỏ tất cả thao tác DB
                            transaction.Rollback();
                            MessageBox.Show("Lỗi trong quá trình xử lý đăng ký: " + ex.Message);
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
            Login loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }
    }
}