using MySql.Data.MySqlClient;
using QuanLyChamCong.Helpers;
using QuanLyChamCong.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyChamCong.Models;
using System.Security.Cryptography; // ✅ Thêm thư viện bảo mật
using System.Text;
namespace QuanLyChamCong.Services
{
    public class EmployeesService
    {
        public async Task<bool> SuaNhanVien(EmployeesModel employee)
        {
            string query = @"UPDATE nhan_vien 
                         SET ho_ten = @Name, 
                             chuc_vu = @Department, 
                             so_dien_thoai = @Phone 
                         WHERE id = @Id";
            try
            {
                using (var conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", employee.Name);
                        cmd.Parameters.AddWithValue("@Department", employee.Department);
                        cmd.Parameters.AddWithValue("@Phone", employee.PhoneNumbers);
                        cmd.Parameters.AddWithValue("@Id", employee .EmployeeID);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }

                }
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần
                throw new Exception("Lỗi cập nhật: " + ex.Message);
            }

        }
        public async Task<bool> XoaNhanVien(int ID)
        {
            string query = @"
                            UPDATE tai_khoan tk
                            JOIN nhan_vien nv ON tk.id = nv.tai_khoan_id
                            SET tk.trang_thai = 'inactive'
                            WHERE nv.id = @id";
            try
            {
                using (var conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString))
                {
                    await conn.OpenAsync();
                    using (var cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", ID);
                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                // 4. Xử lý lỗi Khóa ngoại (Foreign Key Constraint)
                // Mã lỗi 1451: Cannot delete or update a parent row: a foreign key constraint fails
                if (ex.Number == 1451)
                {
                    // Ném lỗi này ra để ViewModel bắt được và hiện thông báo tiếng Việt dễ hiểu
                    throw new Exception("Không thể xóa nhân viên này vì họ đã có dữ liệu chấm công hoặc bảng lương. Hãy xóa dữ liệu liên quan trước!");
                }

                // Các lỗi khác (mất mạng, sai query...)
                throw;
            }
        }
        public async Task<List<EmployeesModel>> GetAll(DateTime date)
        {
            string connect = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;
            var attendanceList = new List<EmployeesModel>();
            string sql = @"
                        SELECT 
                        nv.id AS ma_nhan_vien,
                        nv.ho_ten,
                        nv.chuc_vu,
                        nv.so_dien_thoai,

                        IFNULL(MAX(cl.loai_ca), 'Hiện không trong ca') AS loai_ca,

                        (
                            SELECT COUNT(*) 
                            FROM phan_cong_ca pcc2 
                            JOIN ca_lam cl2 ON pcc2.ca_id = cl2.id 
                            WHERE pcc2.nhan_vien_id = nv.id 
                                      AND cl2.ngay_lam = @ngay_lam
                        ) AS tong_so_ca_hom_nay,

                        MAX(IFNULL(cc.trang_thai, '')) AS trang_thai

                    FROM nhan_vien AS nv

                            -- 1. Các lệnh JOIN phải được thực hiện trước
                    INNER JOIN tai_khoan tk ON tk.id = nv.tai_khoan_id
                                AND tk.trang_thai = 'active' -- 🔥 Chỉ lấy nhân viên có tài khoản ACTIVE

                    LEFT JOIN phan_cong_ca AS pcc 
                           ON nv.id = pcc.nhan_vien_id

                    LEFT JOIN ca_lam AS cl 
                           ON pcc.ca_id = cl.id
                                  AND cl.ngay_lam = @ngay_lam

                    LEFT JOIN cham_cong AS cc 
                           ON cc.nhan_vien_id = nv.id 
                          AND cc.ca_id = cl.id

                            -- 2. Mệnh đề WHERE chuyển xuống đây
                            WHERE nv.doanh_nghiep_id = @DoanhNghiepID

                            -- 3. Cuối cùng là GROUP BY
                    GROUP BY 
                        nv.id, nv.ho_ten, nv.chuc_vu;
            ";
            using (var conn = new MySqlConnection(connect))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ngay_lam", date.Date);
                    cmd.Parameters.AddWithValue("@DoanhNghiepID", DoanhNghiep.CurrentID);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var model = new EmployeesModel();

                            // ========== Map dữ liệu ==========
                            model.EmployeeID = reader.GetInt32("ma_nhan_vien");

                            model.Name = reader.GetString(reader.GetOrdinal("ho_ten"));

                            model.Department = reader.GetString(reader.GetOrdinal("chuc_vu"));

                            // PhoneNumbers có thể null → dùng IsDBNull
                            int phoneOrdinal = reader.GetOrdinal("so_dien_thoai");
                            model.PhoneNumbers = reader.IsDBNull(phoneOrdinal)
                                ? null
                                : reader.GetString(phoneOrdinal);

                            // ShiftType (loai_ca) trả về string
                            int shiftOrdinal = reader.GetOrdinal("loai_ca");
                            model.ShiftType = reader.IsDBNull(shiftOrdinal)
                                ? null
                                : reader.GetString(shiftOrdinal);

                            // Total shifts today → số nguyên
                            int totalOrdinal = reader.GetOrdinal("tong_so_ca_hom_nay");
                            model.TotalShiftsToday = reader.IsDBNull(totalOrdinal)
                                ? 0
                                : reader.GetInt32(totalOrdinal);

                            // Status (trang_thai), có thể rỗng hoặc null
                            int statusOrdinal = reader.GetOrdinal("trang_thai");
                            model.Status = reader.IsDBNull(statusOrdinal)
                                ? null
                                : reader.GetString(statusOrdinal);

                            attendanceList.Add(model);
                        }
                    }
                }
            }
            return attendanceList;
        }
        public static string RemoveVietnameseAccents(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            string[] arr1 = new string[] { 
            "á", "à", "ả", "ã", "ạ", "â", "ấ", "ầ", "ẩ", "ẫ", "ậ", "ă", "ắ", "ằ", "ẳ", "ẵ", "ặ",
            "đ",
            "é", "è", "ẻ", "ẽ", "ẹ", "ê", "ế", "ề", "ể", "ễ", "ệ",
            "í", "ì", "ỉ", "ĩ", "ị",
            "ó", "ò", "ỏ", "õ", "ọ", "ô", "ố", "ồ", "ổ", "ỗ", "ộ", "ơ", "ớ", "ờ", "ở", "ỡ", "ợ",
            "ú", "ù", "ủ", "ũ", "ụ", "ư", "ứ", "ừ", "ử", "ữ", "ự",
            "ý", "ỳ", "ỷ", "ỹ", "ỵ"};
                    string[] arr2 = new string[] { 
            "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a", "a",
            "d",
            "e", "e", "e", "e", "e", "e", "e", "e", "e", "e", "e",
            "i", "i", "i", "i", "i",
            "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o", "o",
            "u", "u", "u", "u", "u", "u", "u", "u", "u", "u", "u",
            "y", "y", "y", "y", "y"};

            text = text.ToLower(); // Chuyển hết sang chữ thường trước
            for (int i = 0; i < arr1.Length; i++)
            {
                text = text.Replace(arr1[i], arr2[i]);
                text = text.Replace(arr1[i].ToUpper(), arr2[i].ToUpper());
            }
            return text;
        }
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
        public async Task<bool> ThemNhanVienVaTaiKhoan(nhan_vien employee)
        {
            if (employee == null) return false;

            // LƯU Ý QUAN TRỌNG:
            // Câu lệnh 1: Thêm tài khoản VÀ gọi SELECT LAST_INSERT_ID() ngay lập tức để lấy ID vừa sinh ra.

            string queryTaiKhoan = @"INSERT INTO tai_khoan (doanh_nghiep_id, so_dien_thoai, mat_khau_hash, vai_tro, trang_thai) 
                             VALUES (@DoanhNghiepId, @SoDienThoai, @MatKhau, @VaiTro, @TrangThai);
                             SELECT LAST_INSERT_ID();";

            // Câu lệnh 2: Thêm nhân viên, lúc này đã có @TaiKhoanId
            string queryNhanVien = @"INSERT INTO nhan_vien (doanh_nghiep_id, tai_khoan_id ,ho_ten, so_dien_thoai, chuc_vu) 
                             VALUES (@DoanhNghiepId, @TaiKhoanId, @HoTen, @SoDienThoai, @ChucVu);";

            using (var conn = new MySqlConnection(ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString))
            {
                await conn.OpenAsync();

                using (var transaction = await conn.BeginTransactionAsync())
                {
                    try
                    {
                        long newTaiKhoanId = 0; // Biến để hứng ID mới sinh ra

                        // --- BƯỚC 1: INSERT TÀI KHOẢN TRƯỚC ---
                        string  TenKhongDau = RemoveVietnameseAccents(employee.ho_ten).Replace(" ", "");
                        string generatedPassword = TenKhongDau + employee.so_dien_thoai;
                        string passwordHash = ComputeSha256Hash(generatedPassword);
                        using (var cmdTK = new MySqlCommand(queryTaiKhoan, conn, transaction))
                        {
                            cmdTK.Parameters.AddWithValue("@DoanhNghiepId", DoanhNghiep.CurrentID);
                            cmdTK.Parameters.AddWithValue("@TrangThai", "active");
                            cmdTK.Parameters.AddWithValue("@SoDienThoai", employee.so_dien_thoai);
                            cmdTK.Parameters.AddWithValue("@MatKhau", passwordHash);
                            cmdTK.Parameters.AddWithValue("@VaiTro", "employee");

                            // ExecuteScalarAsync dùng để lấy giá trị cột đầu tiên của dòng đầu tiên (chính là ID vừa tạo)
                            // Kết quả trả về thường là ulong hoặc decimal, cần convert sang long/int
                            var result = await cmdTK.ExecuteScalarAsync();
                            newTaiKhoanId = Convert.ToInt64(result);
                        }

                        // --- BƯỚC 2: INSERT NHÂN VIÊN (KÈM TAI_KHOAN_ID) ---
                        using (var cmdNV = new MySqlCommand(queryNhanVien, conn, transaction))
                        {
                            cmdNV.Parameters.AddWithValue("@DoanhNghiepId", DoanhNghiep.CurrentID);
                            cmdNV.Parameters.AddWithValue("@HoTen", employee.ho_ten);
                            cmdNV.Parameters.AddWithValue("@SoDienThoai", employee.so_dien_thoai);
                            cmdNV.Parameters.AddWithValue("@ChucVu", employee.chuc_vu);

                            // Điền ID vừa lấy được ở bước trên vào đây
                            cmdNV.Parameters.AddWithValue("@TaiKhoanId", newTaiKhoanId);

                            await cmdNV.ExecuteNonQueryAsync();
                        }

                        // --- BƯỚC 3: COMMIT ---
                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception("Lỗi thêm mới: " + ex.Message);
                    }
                }
            }
        }
    }
}
