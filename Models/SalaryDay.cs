using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Models
{
    public class SalaryDay
    {
        public string NhanVienId { get; set; }
        public string HoTen { get; set; }
        public decimal LuongCoBanNgay { get; set; }
        public decimal PhuCap { get; set; } // Phụ cấp
        public decimal LuongTangCa { get; set; } // Lương tăng ca
        public decimal TruThue { get; set; } // Trừ thuế
        public decimal ThucLinhNgay { get; set; } // Thực lĩnh ngày
        public string TrangThai { get; set; } // Trạng thái
    }
}
