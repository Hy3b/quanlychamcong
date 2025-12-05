
using System.Windows;
namespace QuanLyChamCong
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // The method LiveCharts.Configure does not exist.
            // Instead, you should configure LiveCharts using static properties or initialization code as per LiveCharts documentation.
            // For example, you can set up mappers and themes directly.

            // Example: No need to call LiveCharts.Configure, just set up your configuration here if needed.
            // If you need to configure global settings, do it in the appropriate place (e.g., before using charts).

            // Remove or replace the following line:
            // object value = LiveCharts.Configure(config => config
            //     .AddWpf(options => { })
            //     .AddSkiaSharp()
            //     .AddDefaultMappers()
            //     .AddLightTheme()
            // );

            // If you need to configure LiveCharts, refer to the documentation for the correct approach.
            // For now, simply remove the erroneous line to fix CS0234.
        }
    }
}
