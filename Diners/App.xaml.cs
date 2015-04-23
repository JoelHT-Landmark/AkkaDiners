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
            Metric.Config.WithHttpEndpoint("http://localhost:1234/")
                .WithReporting(c => c.WithSerilogReports(TimeSpan.FromSeconds(30)));

            ////var targetSplunkServerIp = Dns.GetHostEntry("input-prd-p-5z6kswdhj3l2.cloud.splunk.com");
            var targetSplunkServerIp = Dns.GetHostEntry("localhost");
            var targetIpAddress = targetSplunkServerIp.AddressList[0].ToString();

            var splunkContext = new Splunk.Client.Context(Splunk.Client.Scheme.Https, targetIpAddress, 8089);
            var txArgs = new Splunk.Client.TransmitterArgs { Source = "Stephano", SourceType = "AkkaDiners.exe" };

            var serilogContext = new Serilog.Sinks.Splunk.SplunkContext(splunkContext, "AkkaDiners", "admin", "Qz1j4Mkeb", null, txArgs);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(this.logLevelSwitch)
                .WriteTo.ColoredConsole()
                .WriteTo.RollingFile(@"c:\logs\AkkaDiners\Diners-{Date}.txt")
                ////.WriteTo.SplunkViaTcp(targetIpAddress, 10001)
                .WriteTo.SplunkViaUdp(targetIpAddress, 10000)
                ////.WriteTo.SplunkViaHttp(serilogContext, 15, new TimeSpan(0, 0, 0, 5))
                .CreateLogger();
        }

        public void SetLoggingLevel(LogEventLevel level)
        {
            this.logLevelSwitch.MinimumLevel = level;
        }
    }
}
