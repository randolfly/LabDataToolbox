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
using BlazorComponent;
using Masa.Blazor.Presets;
using Microsoft.Extensions.Logging;

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
            
#if DEBUG
            serviceCollection.AddBlazorWebViewDeveloperTools();
            serviceCollection.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.None);
            });
#endif

            serviceCollection.AddMasaBlazor(options =>
            {
                options.ConfigureIcons(IconSet.FontAwesome);
                options.Defaults = new Dictionary<string, IDictionary<string, object?>?>()
                {
                    {
                        PopupComponents.SNACKBAR, new Dictionary<string, object?>()
                        {
                            { nameof(PEnqueuedSnackbars.Closeable), true },
                            { nameof(PEnqueuedSnackbars.Position), SnackPosition.TopRight }
                        }
                    }
                };
            });
            serviceCollection.AddSingleton<AdsDataLogService>();
            serviceCollection.AddSingleton<AppConfigService>();

            Resources.Add("services", serviceCollection.BuildServiceProvider());
        }
    }
}
