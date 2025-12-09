using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Windows.Threading;
namespace QuanLyChamCong.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {

        private DispatcherTimer timer;
        public string SoLuongNhanVienChuaCham { get; set; }
        public int SoLuongNhanVienDaChamCongHomNay { get; set; }
        public int SoLuongNhanVien { get; set; }
        public SeriesCollection ChamCongHomNay { get; set; }
        public SeriesCollection DaylySeries { get; set; }
        public SeriesCollection WeeklySeries { get; set; }
        public string[] Weeks { get; set; }

        public Func<double, string> Formatter { get; set; }
        public Func<ChartPoint, string> PointLabel { get; set; }
        public string[] Labels { get; set; }
        // === DỮ LIỆU CHO BIỂU ĐỒ 1: XU HƯỚNG CHUYÊN CẦN ===
        private readonly DashboardService _dashboardService;
        public void LoadDaylyAttendanceData()
        {
            // Lấy dữ liệu từ dịch vụ
            List<DaylyAttendanceStat> stats = _dashboardService.GetLast7DaysStats();
            // Tạo các danh sách để lưu trữ số liệu
            List<int> onTimeCounts = new List<int>();
            List<int> lateCounts = new List<int>();
            List<int> absentCounts = new List<int>();
            stats.Reverse();
            // Điền dữ liệu vào các danh sách
            foreach (var stat in stats)
            {
                onTimeCounts.Add(stat.OnTimeCount);
                lateCounts.Add(stat.LateCount);
                absentCounts.Add(stat.AbsentCount);
            }
            // Cập nhật SeriesCollection cho biểu đồ
            DaylySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Đúng giờ",
                    Values = new ChartValues<int>(onTimeCounts),
                    Fill = new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)),
                    Stroke = Brushes.Transparent

                },
                new ColumnSeries
                {
                    Title = "Đến muộn",
                    Values = new ChartValues<int>(lateCounts),
                    Fill = new SolidColorBrush(Color.FromArgb(255, 231, 76, 60)),
                    Stroke = Brushes.Transparent

                },
                new ColumnSeries
                {
                    Title = "Vắng mặt",
                    Values = new ChartValues<int>(absentCounts),
                    Fill = new SolidColorBrush(Color.FromArgb(255,255, 193,  7)),
                    Stroke = Brushes.Transparent

                }
            };
        }
        public void LoadPieChartData()
        {
            // 1. Lấy dữ liệu từ hàm SQL bạn đã viết
            // Giả sử hàm trả về List<int>: [0] = Đúng giờ, [1] = Đi muộn, [2] = Vắng
            List<int> data = _dashboardService.GetTodayAttendanceStats();
            SoLuongNhanVienDaChamCongHomNay = data[0] + data[1]; // Tổng số đã chấm công (Đúng giờ + Đi muộn)
            double tiLe = SoLuongNhanVienDaChamCongHomNay / ((double)data[2] + data[0] + data[1]) * 100;
            SoLuongNhanVienChuaCham = tiLe.ToString(",#") + "% nhân viên đã có mặt"; // Tổng số nhân viên - Số đã chấm công = Số chưa chấm công
            // 2. Tạo Converter để xử lý màu HEX
            var brushConverter = new BrushConverter();

            // 3. Khởi tạo SeriesCollection
            ChamCongHomNay = new SeriesCollection
                {
                new PieSeries
                {
                    Title = "Đúng giờ",
                    Values = new ChartValues<int> { data[0] }, // Giá trị từ SQL
                    DataLabels = true,
                    Fill = (Brush)brushConverter.ConvertFrom("#2ECC71") // Màu Xanh
                },
                new PieSeries
                {
                    Title = "Đi muộn",
                    Values = new ChartValues<int> { data[1] }, // Giá trị từ SQL
                    DataLabels = true,
                    Fill = (Brush)brushConverter.ConvertFrom("#E74C3C") // Màu Đỏ
                },
                new PieSeries
                {
                    Title = "Chưa vào",
                    Values = new ChartValues<int> { data[2] }, // Giá trị từ SQL
                    DataLabels = true,
                    Fill = (Brush)brushConverter.ConvertFrom("#95A5A6") // Màu Xám
                }
                };

            // Nếu bạn dùng MVVM, nhớ gọi OnPropertyChanged("AttendanceSeries") ở đây để giao diện cập nhật
            // Nếu dùng Code-Behind (MainWindow.xaml.cs), gán DataContext = this;

        }
        public void LoadWeeklyAttendanceData()
        {
            // 1. Lấy dữ liệu từ DB
            var stats = _dashboardService.GetWeeklyStats();

            // 2. ĐẢO NGƯỢC danh sách để biểu đồ vẽ từ Quá khứ -> Hiện tại
            stats.Reverse();
            var brushConverter = new BrushConverter();

            // 3. Tách lấy Labels (Trục X) - Chính là cột WeekRange ("17/11 - 23/11")
            Weeks = stats.Select(x => x.WeekRange).ToArray();

            // 4. Khởi tạo SeriesCollection
            WeeklySeries = new SeriesCollection
    {
        // Cột Đúng giờ (Màu Xanh lá)
        new StackedAreaSeries // Hoặc dùng StackedAreaSeries tùy bạn
        {
            Title = "Đúng giờ",
            Values = new ChartValues<int>(stats.Select(x => x.OnTimeCount)),
            Fill = new SolidColorBrush(Color.FromArgb(255, 46, 204, 113)), // #2ECC71

                Stroke = Brushes.Transparent,
                LineSmoothness = 0.8,
    
                // --- Mô tả dữ liệu ---
                DataLabels = true, // Bắt buộc phải bật cái này thì LabelPoint mới hiện

        },
        
        // Cột Đi muộn (Màu Đỏ)
        new StackedAreaSeries
        {
                Title = "Đi muộn", // Tên ở Legend
                Values = new ChartValues<int>(stats.Select(x => x.LateCount)),
    
                // --- Giao diện ---
                Fill = (Brush)brushConverter.ConvertFrom("#E74C3C"),
                Stroke = Brushes.Transparent,
                LineSmoothness = 0.8,
    
                // --- Mô tả dữ liệu ---
                DataLabels = true, // Bắt buộc phải bật cái này thì LabelPoint mới hiện
    
        },

        // Cột Vắng (Màu Vàng/Cam)
        new StackedAreaSeries
        {
            Title = "Vắng mặt",
            Values = new ChartValues<int>(stats.Select(x => x.AbsentCount)),
            Fill = new SolidColorBrush(Color.FromRgb(255, 193, 7)), // #FFC107
                Stroke = Brushes.Transparent,
                LineSmoothness = 0.8,
    
                // --- Mô tả dữ liệu ---
                DataLabels = true, // Bắt buộc phải bật cái này thì LabelPoint mới hiện
               
    
        }
    };

            // Nếu dùng MVVM, nhớ gọi OnPropertyChanged cho WeeklySeries và WeeklyLabels
        }
        public string[] GetDateLabels(int numberOfDays)
        {
            string[] labels = new string[numberOfDays];
            DateTime today = DateTime.Now;

            // Thiết lập ngôn ngữ Tiếng Việt để hiển thị "Thứ Hai", "Thứ Ba"...
            // thay vì "Monday", "Tuesday" nếu máy tính đang dùng tiếng Anh.
            CultureInfo vietnameseCulture = new CultureInfo("vi-VN");

            for (int i = 0; i < numberOfDays; i++)
            {
                // Logic: 
                // i chạy từ 0 đến cuối mảng.
                // daysToSubtract là số ngày cần lùi lại.
                // Tại phần tử cuối cùng (i = numberOfDays - 1), daysToSubtract sẽ bằng 0 (là hôm nay).
                int daysToSubtract = (numberOfDays - 1) - i;

                DateTime targetDate = today.AddDays(-daysToSubtract);

                // Định dạng chuỗi. Ví dụ: "Thứ Năm (20/11)"
                // "dddd": Tên thứ đầy đủ.
                // "dd/MM": Ngày/Tháng.
                labels[i] = targetDate.ToString("dddd (dd/MM)", vietnameseCulture);
            }

            return labels;
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            LoadPieChartData();
        }
        public DashboardViewModel()
        {
            // Khởi tạo dịch vụ
            _dashboardService = new DashboardService();
            // Dữ liệu biểu đồ tròn chuyên cần hôm nay

            // Dữ liệu biểu đồ thanh ngang chuyên cần hàng tuần
            LoadPieChartData();
            LoadDaylyAttendanceData();
            Labels = GetDateLabels(7);
            SoLuongNhanVien = _dashboardService.GetTotalEmployees();

            Formatter = value => value.ToString("N");
            LoadWeeklyAttendanceData();
            // Dữ liệu biểu đồ xu hướng chuyên cần hàng tuần
            // Định dạng trục Y
            PointLabel = chartPoint => string.Format("{0} ({1:P})", chartPoint.Y, chartPoint.Participation);// Định dạng nhãn điểm dữ liệu
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(20); // Đặt thời gian là 20 giây
            timer.Tick += Timer_Tick;
            timer.Start();
        }
    }
}
