using CommonHelpers.MQTTs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Logs.MQTTProvider
{
    public static class MQTTLoggerExtensions
    {
        public static ILoggerFactory AddMQTTLogger(this ILoggerFactory loggerFactory, MQTTLoggerConfiguration config, IMQTTQueueService mqttService)
        {
            loggerFactory.AddProvider(new MQTTLoggerProvider(config, mqttService));
            return loggerFactory;
        }      
        public static ILoggerFactory AddMQTTLogger(this ILoggerFactory loggerFactory, Action<MQTTLoggerConfiguration> configure, IMQTTQueueService mqttService)
        {
            var config = new MQTTLoggerConfiguration();
            configure(config);
            return loggerFactory.AddMQTTLogger(config, mqttService);
        }
    }
}
