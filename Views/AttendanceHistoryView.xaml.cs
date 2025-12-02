using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using QuanLyChamCong.Models;
using QuanLyChamCong.ViewModels;
namespace QuanLyChamCong.Views
{
    /// <summary>
    /// Interaction logic for AttendanceHistoryView.xaml
    /// </summary>
    /// 


    public partial class AttendanceHistoryView : Window
    {
        public AttendanceHistoryView()
        {
            InitializeComponent();
            this.DataContext = new AttendanceHistoryViewModel();
        }
    }
}
