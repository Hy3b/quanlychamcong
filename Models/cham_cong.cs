using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuanLyChamCong.Models
{
    class cham_cong
    {
        public int id { get; set; }
        public int nhan_vien_id { get; set; }
        public int ca_id { get; set; }
        public TimeSpan? gio_vao { get; set; }
        public TimeSpan? gio_ra { get; set; }
        public string trang_thai { get; set; }
        public string phut_tang_ca { get; set; }
    }
}
