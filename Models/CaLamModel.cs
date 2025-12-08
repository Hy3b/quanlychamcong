using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Models
{
    public class CaLamModel
    {
        public int CaId { get; set; }
        public string LoaiCa { get; set; }
        public DateTime NgayLam { get; set; }
        public TimeSpan GioBatDau { get; set; }
        public TimeSpan GioKetThuc { get; set; }
        public int SoNhanVien { get; set; }
    }
}
