using CommonHelpers.MQTTs;
using M2Mqtt.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Powers
{
    public class PowerService : IPowerService
    {
        private readonly ILogger<PowerService> _logger;
        private readonly IMQTTQueueService _mqttService;
       
        private List<Action<PowerDataBase>> _onValueAcquiredHandlerList = new List<Action<PowerDataBase>>();

        public PowerService(IMQTTQueueService mqttService, ILogger<PowerService> logger)
        {
            _logger = logger;
            _mqttService = mqttService;           
            _mqttService.AddSubscriber(Constants.Power.Queues.Production, _powerProductionAcquired);
        }

        private void _powerProductionAcquired(MqttMsgPublishEventArgs data)
        {
            var jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
            var messageText = ASCIIEncoding.ASCII.GetString(data.Message);
            MessageBase mb = JsonConvert.DeserializeObject<MessageBase>(messageText, jsonSerializerSettings);

            if(mb.Data.GetType() == typeof(ProductionDataInfo))
            {
                _onValueAcquiredHandlerList.ForEach(item => item((PowerDataBase)mb.Data));
            }
        }

        public void SubscribeOnValueAcquired(Action<PowerDataBase> onValueAcquiredAction)
        {
            _onValueAcquiredHandlerList.Add(onValueAcquiredAction);
        }
    }
}
