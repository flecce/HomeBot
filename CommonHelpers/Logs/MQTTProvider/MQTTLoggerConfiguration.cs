using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Logs.MQTTProvider
{
    public class MQTTLoggerConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
       
        public string BrokerServer { get; set; }
    }
}
