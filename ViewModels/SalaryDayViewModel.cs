using MySql.Data.MySqlClient;
using QuanLyChamCong.Models;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows; // Để dùng MessageBox
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel; // <--- QUAN TRỌNG: Phải có cái này
using QuanLyChamCong.Views;

namespace QuanLyChamCong.ViewModels
{
    // 1. Thêm 'public partial' và kế thừa ': ObservableObject'
    public partial class SalaryDayViewModel : ObservableObject
    {
        // TÌm kiếm theo giờ
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get { return _selectedDate; }
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));

                // Khi chọn ngày, tự động tải lại dữ liệu để lọc ngay
                LoadDataFromMySQL();
            }
        } 
        //Thanh tìm kiếm
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));

                // Khi gõ chữ, tự động gọi hàm tải lại dữ liệu để lọc ngay
                LoadDataFromMySQL();
            }
        }
        public ObservableCollection<SalaryDay> salaryForms { get; set; }

        private SalaryDay _selectedSalaryItem;
        public SalaryDay SelectedSalaryItem
        {
            get { return _selectedSalaryItem; }
            set
            {
                _selectedSalaryItem = value;
                // Hàm này chỉ chạy được khi đã kế thừa ObservableObject
                OnPropertyChanged(nameof(SelectedSalaryItem));
            }
        }

        public IRelayCommand EditSalaryCommand { get; }
        public IRelayCommand ShowHistoryCommand { get; }
        public SalaryDayViewModel()
        {
            salaryForms = new ObservableCollection<SalaryDay>();
            EditSalaryCommand = new RelayCommand(ExecuteEditSalary);
            // 2. KHỞI TẠO LỆNH (Bước này bạn bị thiếu)
            // Nếu không có dòng này, nút bấm sẽ không hoạt động
            EditSalaryCommand = new RelayCommand(ExecuteEditSalary);
            // 2. Khởi tạo lệnh mở lịch sử
            ShowHistoryCommand = new RelayCommand(ExecuteShowHistory);
            LoadDataFromMySQL();
        }
        private void ExecuteShowHistory()
        {
            // Tạo ViewModel cho lịch sử
            var historyVM = new MonthlySalaryViewModel();

            // Tạo Window và gán DataContext
            MonthlySalaryWindow historyWindow = new MonthlySalaryWindow();
            historyWindow.DataContext = historyVM;

            // Hiện cửa sổ
            historyWindow.ShowDialog();
        }
        private void LoadDataFromMySQL()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ketloicuatoi"].ConnectionString;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // 1. CÂU LỆNH SQL CÓ TÌM KIẾM
                    // Tìm theo Tên gần đúng HOẶC ID gần đúng
                    // @key để kiểm tra nếu ô tìm kiếm trống thì lấy hết
                    string query = @"
                SELECT 
                    luong_ngay.*, 
                    nhan_vien.ho_ten 
                FROM luong_ngay
                JOIN nhan_vien ON luong_ngay.nhan_vien_id = nhan_vien.id
                WHERE (@key IS NULL OR @key = '' 
                       OR nhan_vien.ho_ten LIKE @search 
                       OR luong_ngay.nhan_vien_id LIKE @search)
                       AND (@date IS NULL OR DATE(luong_ngay.ngay_tinh_luong) = DATE(@date))";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // 2. TRUYỀN THAM SỐ TÌM KIẾM
                        // SearchText lấy từ ô nhập liệu bạn đã tạo
                        cmd.Parameters.AddWithValue("@key", SearchText);
                        cmd.Parameters.AddWithValue("@search", $"%{SearchText}%");
                        // 3. THAM SỐ NGÀY
                        // Nếu SelectedDate là null, SQL sẽ bỏ qua điều kiện này nhờ (@date IS NULL)
                        cmd.Parameters.AddWithValue("@date", SelectedDate);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            // 3. QUAN TRỌNG: Xóa danh sách cũ trước khi nạp mới
                            // Nếu không có dòng này, danh sách sẽ bị nối dài vô tận khi bạn gõ phím
                            salaryForms.Clear();

                            while (reader.Read())
                            {
                                SalaryDay emp = new SalaryDay();

                                emp.NhanVienId = reader["nhan_vien_id"].ToString();
                                emp.HoTen = reader["ho_ten"].ToString();

                                if (reader["luong_co_ban_ngay"] != DBNull.Value)
                                    emp.LuongCoBanNgay = Convert.ToDecimal(reader["luong_co_ban_ngay"]);

                                if (reader["phu_cap"] != DBNull.Value)
                                    emp.PhuCap = Convert.ToDecimal(reader["phu_cap"]);

                                if (reader["luong_tang_ca"] != DBNull.Value)
                                    emp.LuongTangCa = Convert.ToDecimal(reader["luong_tang_ca"]);

                                if (reader["tru_thue"] != DBNull.Value)
                                    emp.TruThue = Convert.ToDecimal(reader["tru_thue"]);

                                if (reader["thuc_linh_ngay"] != DBNull.Value)
                                    emp.ThucLinhNgay = Convert.ToDecimal(reader["thuc_linh_ngay"]);

                                emp.TrangThai = reader["ngay_tinh_luong"].ToString();

                                salaryForms.Add(emp);
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

        private void ExecuteEditSalary()
        {
            if (SelectedSalaryItem == null)
            {
                MessageBox.Show("Vui lòng chọn nhân viên cần sửa!", "Thông báo");
                return;
            }

            // Tạo ViewModel và Window sửa
            var editVM = new EditSalaryViewModel(SelectedSalaryItem);
            EditSalaryWindow editWindow = new EditSalaryWindow();
            editWindow.DataContext = editVM;

            // Setup hành động đóng cửa sổ
            editVM.CloseAction = () => editWindow.Close();

            // Hiện cửa sổ
            editWindow.ShowDialog();

            // Tải lại dữ liệu sau khi sửa xong
            salaryForms.Clear();
            LoadDataFromMySQL();
        }
    }
}