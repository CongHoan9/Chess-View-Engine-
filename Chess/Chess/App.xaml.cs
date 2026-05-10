using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Markup;

namespace Chess
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("vi-VN");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("vi-VN");
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage("vi-VN")));
            if (e.Args.Any(arg => arg.Equals("--gui", StringComparison.OrdinalIgnoreCase)))
            {
                MainWindow window = new();
                MainWindow = window;
                window.Show();
                return;
            }
            ConsoleGame.Run();
        }
    }
}
