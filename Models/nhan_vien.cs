using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Models
{
    class nhan_vien
    {
        public int id { get; set; }
        public int doanh_nghiep_id { get; set; }
        public int tai_khoan_id { get; set; }
        public string ho_ten { get; set; }
        public string so_dien_thoai { get; set; }
        public string chuc_vu { get; set; }
        public double luong_co_ban_thang { get; set; }
        public double phu_cap { get; set; }
        public float ti_le_bao_hiem { get; set; }
        public float ti_le_thue { get; set; }
    }
}
