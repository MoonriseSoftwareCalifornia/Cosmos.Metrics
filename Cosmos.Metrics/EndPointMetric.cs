using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Metrics
{
    /// <summary>
    /// Summary statistics for the FrontDoorAccessLog.
    /// </summary>
    /// <remarks>
    /// The following KQL Log Analytics query can get data from this table:
    /// 
    /// AzureDiagnostics | where ResourceProvider == "MICROSOFT.CDN" | where Category == "FrontDoorAccessLog"
    /// 
    /// </remarks>
    public class EndPointMetric
    {
        /// <summary>
        /// Gets or sets the date (day) during which bytes are recorded.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Gets or sets the total response bytes for the date indicated.
        /// </summary>
        public long? Bytes { get; set; }

        /// <summary>
        /// Gets or sets the host header.
        /// </summary>
        public string Host { get; set; } = string.Empty;
    }
}
