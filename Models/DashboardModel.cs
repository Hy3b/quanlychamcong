using LiveCharts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Models
{
    public class DashboardModel
    {

    }
    public class WeeklyAttendanceStat
    {
        public string WeekRange { get; set; }
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
    }
    public class DaylyAttendanceStat
    {
        public DateTime Date { get; set; }
        public string DayName { get; set; }
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
    }
}
