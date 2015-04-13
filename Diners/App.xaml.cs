namespace Diners
{
    using Metrics;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Metric.Config.WithHttpEndpoint("http://localhost:1234/");
        }
    }
}
