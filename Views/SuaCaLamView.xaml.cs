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
using QuanLyChamCong.Models;
namespace QuanLyChamCong.Views
{
    /// <summary>
    /// Interaction logic for SuaCaLamView.xaml
    /// </summary>
    public partial class SuaCaLamView : Window
    {
        public SuaCaLamView(CaLamModel nv)
        {
            InitializeComponent();
            DataContext = new SuaCaLamViewModel(nv);
        }
        private void BtnHuy_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
