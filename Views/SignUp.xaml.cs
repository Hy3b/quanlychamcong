using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyChamCong.Views
{
    public partial class SignUp : Window
    {
        public SignUp()
        {
            InitializeComponent();
        }

        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            string soDienThoai = txtSoDienThoai.Text.Trim();
            string matKhau = txtPass.Password;
            string confirm = txtConfirm.Password;
            string tenDN = txtName.Text.Trim();
            string taiKhoanChuNhap = txtTaiKhoan.Password;
            string vaiTro = (cbVaiTro.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Validate
            if (string.IsNullOrEmpty(soDienThoai) || string.IsNullOrEmpty(matKhau) || string.IsNullOrEmpty(confirm)
                || string.IsNullOrEmpty(tenDN) || string.IsNullOrEmpty(taiKhoanChuNhap) || string.IsNullOrEmpty(vaiTro))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            if (matKhau != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!");
                return;
            }

            // KHAI BÁO BIẾN Ở ĐÂY — FIX LỖI
            long doanhNghiepId = 0;
            long taiKhoanId = 0;

            string constr = "Server=localhost;Database=chamcong;User ID=root;Password=tien0399007905;";

            using (MySqlConnection conn = new MySqlConnection(constr))
            {
                conn.Open();

                // 1. Insert doanh nghiệp trước
                string insertDN = "INSERT INTO doanh_nghiep (ten_doanh_nghiep) VALUES (@tenDN)";
                using (MySqlCommand cmd = new MySqlCommand(insertDN, conn))
                {
                    cmd.Parameters.AddWithValue("@tenDN", tenDN);
                    cmd.ExecuteNonQuery();
                    doanhNghiepId = cmd.LastInsertedId;   // LẤY ID
                }

                // 2. Insert tài khoản
                string insertTK = @"INSERT INTO tai_khoan 
                            (doanh_nghiep_id, so_dien_thoai, mat_khau_hash, vai_tro, trang_thai)
                            VALUES (@dnId, @soDT, @mk, @vaiTro, 'active')";
                using (MySqlCommand cmd = new MySqlCommand(insertTK, conn))
                {
                    cmd.Parameters.AddWithValue("@dnId", doanhNghiepId);
                    cmd.Parameters.AddWithValue("@soDT", soDienThoai);
                    cmd.Parameters.AddWithValue("@mk", matKhau);
                    cmd.Parameters.AddWithValue("@vaiTro", vaiTro);

                    cmd.ExecuteNonQuery();
                    taiKhoanId = cmd.LastInsertedId;      // LẤY ID
                }

                // 3. Update doanh nghiệp → gán chủ sở hữu
                string updateDN = "UPDATE doanh_nghiep SET tai_khoan_chu_so_huu = @tkId WHERE id = @dnId";
                using (MySqlCommand cmd = new MySqlCommand(updateDN, conn))
                {
                    cmd.Parameters.AddWithValue("@tkId", taiKhoanId);
                    cmd.Parameters.AddWithValue("@dnId", doanhNghiepId);
                    cmd.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Đăng ký thành công!");
            Login main = new Login();
            main.Show();
            this.Close();
        }

        private void cbVaiTro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
