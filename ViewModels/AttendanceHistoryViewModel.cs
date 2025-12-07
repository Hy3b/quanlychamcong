using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLyChamCong.Models;
using QuanLyChamCong.Services;
using CommunityToolkit.Mvvm.ComponentModel;
namespace QuanLyChamCong.ViewModels
{
    internal partial class AttendanceHistoryViewModel : ObservableObject
    {
        private readonly AttendanceHistoryService attendanceHistoryService;
        [ObservableProperty]
        private List<AttendanceRecord> _attendanceRecords;
        [ObservableProperty]
        private int _nhanVienId;
        [ObservableProperty]
        private string _hoTen;
        [ObservableProperty]
        private string _chucVu;
        public AttendanceHistoryViewModel(AttendanceModel employeeInfo)
        {
            NhanVienId = employeeInfo.NhanVienId;
            HoTen = employeeInfo.HoTen;
            ChucVu = employeeInfo.ChucVu;
            attendanceHistoryService = new AttendanceHistoryService();
            AttendanceRecords = attendanceHistoryService.GetAttendanceHistory(NhanVienId);

        }
    }
}
