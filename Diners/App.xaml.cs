namespace Diners
{
    using Metrics;
    using Serilog;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Metric.Config.WithHttpEndpoint("http://localhost:1234/");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile("Diners-{Date}.txt")
                .CreateLogger();
        }
    }
}
