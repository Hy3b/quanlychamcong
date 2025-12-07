using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Models
{
    public class AttendanceRecord
    {
        public DateTime? gio_vao { get; set; }
        public DateTime? gio_ra { get; set; }
        public string trang_thai { get; set; }
        public String Ngay => gio_vao?.ToString("dd/MM/yyyy");
    }
}
