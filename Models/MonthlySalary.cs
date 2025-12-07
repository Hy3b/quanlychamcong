using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Models
{
    public class MonthlySalary
    {
        public int Id { get; set; }
        public string NhanVienId { get; set; }
        public string HoTen { get; set; } // Lấy từ bảng nhan_vien
        public string Thang { get; set; } // Ví dụ: "2025-11"

        public decimal TongLuongCoBan { get; set; }
        public decimal TongPhuCap { get; set; }
        public decimal TongTangCa { get; set; }
        public decimal TongTruBaoHiem { get; set; }
        public decimal TongTruThue { get; set; }
        public decimal ThucLinhThang { get; set; }

        public string NgayChotLuong { get; set; }
    }
}
