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
            HealthChecks.RegisterHealthCheck("IsAlive", () => { return "true"; });

            Log.Logger = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile(@"c:\logs\AkkaDiners\Diners-{Date}.txt")
              .CreateLogger();

        }
    }
}
