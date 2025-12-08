using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
namespace QuanLyChamCong.Models
{
    public partial class NhanVienChonModel : ObservableObject
    {
        public int Id { get; set; }
        public string HoTen { get; set; }

        [ObservableProperty]
        private bool _isSelected;
    }
}
