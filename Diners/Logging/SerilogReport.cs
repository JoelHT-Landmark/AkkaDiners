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
        protected override void WriteLine(string line, params string[] args)
        {
            Log.Information(line, args);
        }
    }
}
