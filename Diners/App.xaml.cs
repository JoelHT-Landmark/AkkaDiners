namespace Diners
{
    using Metrics;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly LoggingLevelSwitch logLevelSwitch = new LoggingLevelSwitch();

        public App()
        {
            Metric.Config.WithHttpEndpoint("http://localhost:1234/");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(this.logLevelSwitch)
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile("Diners-{Date}.txt")
                .CreateLogger();
        }

        public void SetLoggingLevel(LogEventLevel level)
        {
            this.logLevelSwitch.MinimumLevel = level;
        }
    }
}
