using CommunityToolkit.Mvvm.ComponentModel;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace QuanLyChamCong.ViewModels
{
    internal class AttendanceViewModel : ObservableObject
    {
        private readonly AttendanceService _service;
        public void RefreshCommand()
        {
            LoadData();
        }
        public int SelectedDate { get; set; }
        public List<AttendanceModel> DailyAttendance { get; set; }
        public AttendanceViewModel()
        {
            _service = new AttendanceService();
            DailyAttendance = new List<AttendanceModel>();
            LoadData();
        }
        public void LoadData()
        {
            DailyAttendance = _service.GetDailyAttendance();
        }
    }
}
