namespace Diners
{
    using Diners.Logging;
    using Metrics;
    using Serilog;
    using Serilog.Core;
    using Serilog.Events;
    using System;
    using System.Net;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly LoggingLevelSwitch logLevelSwitch = new LoggingLevelSwitch();

        public App()
        {
            var targetSplunkServerIp = Dns.GetHostEntry("jrht-ubuntusplunk.cloudapp.net");
            var targetIpAddress = targetSplunkServerIp.AddressList[0].ToString();

            Metric.Config.WithHttpEndpoint("http://localhost:1234/")
                    .WithReporting(c => c.WithSerilogReports(TimeSpan.FromSeconds(30)))
                    .WithReporting(c => c.WithGraphite(new Uri("net.udp://jrht-ubuntusplunk.cloudapp.net:2003"), TimeSpan.FromSeconds(30)));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(this.logLevelSwitch)
                .WriteTo.Console()
                .WriteTo.RollingFile(@"c:\logs\AkkaDiners\Diners-{Date}.txt")
                .WriteTo.EventCollector("http://lmk-jrht-winsplunk.cloudapp.net:8088/services/collector/event", "3F5D11AC-C4AB-4EFC-A525-7B145F3DF1C8")
                .CreateLogger();
        }

        public void SetLoggingLevel(LogEventLevel level)
        {
            this.logLevelSwitch.MinimumLevel = level;
        }
    }
}
