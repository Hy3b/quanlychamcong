
using System.Windows;
using QuanLyChamCong.ViewModels;
using QuanLyChamCong.Views;
namespace QuanLyChamCong.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainViewModel mainViewModel { get; set; }
        public MainWindow(string owner_name)
        {
            InitializeComponent();
            mainViewModel = new MainViewModel(); 
            mainViewModel.OwnerName = owner_name;
            this.DataContext = mainViewModel;
        }
    }
}