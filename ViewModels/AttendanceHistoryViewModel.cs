using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
namespace QuanLyChamCong.ViewModels
{
    internal partial class AttendanceHistoryViewModel
    {
        private readonly AttendanceService _attendanceService;
        public List<AttendanceRecord> AttendanceRecords { get; set; }
        public AttendanceModel EmployeeInfo { get; set; }
        public AttendanceHistoryViewModel()
        {
            _attendanceService = new AttendanceService();
            EmployeeInfo = new AttendanceModel();
        }
    }
}
