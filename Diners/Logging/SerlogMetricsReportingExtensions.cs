using Metrics.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diners.Logging
{
    public static class SerlogMetricsReportingExtensions
    {
        public static MetricsReports WithSerilogReports(this MetricsReports reports, TimeSpan interval)
        {
            reports.WithReport(new SerilogReport(), interval);

            return reports;
        }
    }
}
