using CommonHelpers.MQTTs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Logs.MQTTProvider
{
    public class MQTTLoggerProvider : ILoggerProvider
    {
        private readonly MQTTLoggerConfiguration _config;
        private readonly ConcurrentDictionary<string, MQTTLogger> _loggers = new ConcurrentDictionary<string, MQTTLogger>();
        private readonly IMQTTQueueService _mqttService;

        public MQTTLoggerProvider(MQTTLoggerConfiguration config, IMQTTQueueService mqttService)
        {
            _config = config;
            _mqttService = mqttService;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new MQTTLogger(name, _mqttService, _config));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
