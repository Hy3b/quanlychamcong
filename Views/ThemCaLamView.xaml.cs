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
using System.Windows.Shapes;
using QuanLyChamCong.ViewModels;
namespace QuanLyChamCong.Views
{
    /// <summary>
    /// Interaction logic for ThemCaLam.xaml
    /// </summary>
    public partial class ThemCaLam : Window
    {
        public ThemCaLam()
        {
            InitializeComponent();
            this.DataContext = new ThemCaLamViewModel();
        }
    }
}
