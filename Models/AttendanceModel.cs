using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace QuanLyChamCong.Models
{
    public class AttendanceModel
    {
        // Các Properties cần có trong ViewModel
        public int NhanVienId { get; set; }      // Map với nv.id
        public string HoTen { get; set; }        // Map với nv.ho_ten
        public string ChucVu { get; set; }       // Map với nv.chuc_vu
        public TimeSpan GioBatDau { get; set; }  // Map với cl.gio_bat_dau
        public TimeSpan GioKetThuc { get; set; } // Map với cl.gio_ket_thuc
        public TimeSpan? GioVao { get; set; }    // Map với cc.gio_vao (Lấy phần giờ)
        public TimeSpan? GioRa { get; set; }     // Map với cc.gio_ra (Lấy phần giờ)
        public string TrangThai { get; set; }       // Để disable/enable nút bấm
        //public ObservableCollection<AttendanceRecord> HistoryList { get; set; } Danh sách lịch sử
    }


}
