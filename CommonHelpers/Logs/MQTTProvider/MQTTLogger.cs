using CommonHelpers.MQTTs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Logs.MQTTProvider
{
    public class MQTTLogger : ILogger
    {
        private readonly string _name;
        private readonly MQTTLoggerConfiguration _config;
        private readonly IMQTTQueueService _mqttService;

        public MQTTLogger(string name, IMQTTQueueService mqttService, MQTTLoggerConfiguration config)
        {
            _name = name;
            _config = config;
            _mqttService = mqttService;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == _config.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }
            _mqttService.Publish("/home/logs/"+_name, $"{logLevel}-{_name}-{formatter(state, exception)}");
        }
    }
}
