using ClosedXML.Excel; // Thư viện Excel
using Microsoft.Win32; // Để dùng SaveFileDialog
using QuanLyChamCong.Models;
using System.Windows;
using System.Windows.Controls;

namespace QuanLyChamCong.Views
{

    public partial class EmployeesView : UserControl
    {
        public EmployeesView()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void ExportToExcel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // 1. Lấy dữ liệu từ DataGrid (Giả sử DataGrid tên là dgEmployees)
            // Ép kiểu về danh sách nhân viên
            var listData = dgEmployees.ItemsSource as IEnumerable<EmployeesModel>;

            if (listData == null || !listData.Any())
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 2. Mở hộp thoại chọn nơi lưu
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"DanhSachNhanVien_{DateTime.Now:ddMMyyyy}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 3. Tạo file Excel bằng ClosedXML
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Danh Sách Nhân Viên");

                        // --- TẠO HEADER (Dòng 1) ---
                        worksheet.Cell(1, 1).Value = "ID";
                        worksheet.Cell(1, 2).Value = "Họ và Tên";
                        worksheet.Cell(1, 3).Value = "Chức vụ";
                        worksheet.Cell(1, 4).Value = "Số điện thoại";
                        worksheet.Cell(1, 5).Value = "Ngày tạo";

                        // Định dạng Header: In đậm, nền xám
                        var headerRow = worksheet.Range("A1:E1");
                        headerRow.Style.Font.Bold = true;
                        headerRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        // --- ĐỔ DỮ LIỆU VÀO DÒNG ---
                        int row = 2;
                        foreach (var item in listData)
                        {
                            worksheet.Cell(row, 1).Value = item.EmployeeID;
                            worksheet.Cell(row, 2).Value = item.Name;
                            worksheet.Cell(row, 3).Value = item.Department;
                            worksheet.Cell(row, 4).Value = item.PhoneNumbers;
                            // Ví dụ thêm ngày tháng (nếu có)
                            worksheet.Cell(row, 5).Value = DateTime.Today;

                            row++;
                        }

                        // 4. Tự động chỉnh độ rộng cột cho đẹp
                        worksheet.Columns().AdjustToContents();

                        // 5. Lưu file
                        workbook.SaveAs(saveFileDialog.FileName);
                    }

                    MessageBox.Show("Xuất Excel thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Tùy chọn: Mở file ngay sau khi lưu
                    // System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(saveFileDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Có lỗi khi lưu file: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToPDF_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement PDF export logic here
            MessageBox.Show("Export to PDF clicked.");
        }
    }
}
