using Metrics.Reporters;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diners.Logging
{
    public class SerilogReport : HumanReadableReport
    {
        private readonly StringBuilder builder = new StringBuilder();

        public SerilogReport()
        {

        }

        protected override void WriteLine(string line, params string[] args)
        {
            builder.AppendLine(string.Format(line, args));
        }

        protected override void StartReport(string contextName)
        {
            builder.Clear();
            base.StartReport(contextName);
        }
        protected override void EndReport(string contextName)
        {
            base.EndReport(contextName);
            Log.Information(builder.ToString());
        }
    }
}
