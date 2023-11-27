using LabDataToolbox.Service;
using Masa.Blazor;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LabDataToolbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWpfBlazorWebView();
            serviceCollection.AddMasaBlazor(options =>
            {
                options.ConfigureIcons(IconSet.MaterialDesign);
            });
            serviceCollection.AddSingleton<AdsDataLogService>();
            serviceCollection.AddSingleton<AppConfigService>();

            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
    }
}
