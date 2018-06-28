using CommonHelpers.Gardens.Water;
using CommonHelpers.MQTTs;
using M2Mqtt.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Gardens
{
    public class GardenService : IGardenService
    {
        private readonly ILogger<GardenService> _logger;
        private readonly IMQTTQueueService _mqttService;
        private readonly IGardenWaterControllerService _waterController;
        private List<Action> _onStartHandlerList = new List<Action>();
        private List<Action> _onStopHandlerList = new List<Action>();

        public GardenService(IMQTTQueueService mqttService, IGardenWaterControllerService waterController, ILogger<GardenService> logger)
        {
            _logger = logger;
            _mqttService = mqttService;
            _waterController = waterController;
            _mqttService.AddSubscriber(Constants.Garden.Queues.Water, _gardenWaterMessageManager);
        }

        public void SubscribeOnStart(Action onStartAction)
        {
            _onStartHandlerList.Add(onStartAction);
        }

        public void SubscribeOnStop(Action onStopAction)
        {
            _onStopHandlerList.Add(onStopAction);
        }

        private void _gardenWaterMessageManager(MqttMsgPublishEventArgs evt)
        {
            string textCommand = ASCIIEncoding.ASCII.GetString(evt.Message);

            if (textCommand == Constants.Garden.Messages.ON)
            {
                _onStartHandlerList.ForEach(action => action());
            }

            if (textCommand == Constants.Garden.Messages.OFF)
            {
                _onStopHandlerList.ForEach(action => action());
            }

            if (textCommand == Constants.Garden.Messages.OFFRequired)
            {
                // Chiamata a arduino per chiusura
                _waterController.Close();
                _mqttService.Publish(Constants.Garden.Queues.Water, Constants.Garden.Messages.OFF);
            }

            if (textCommand == Constants.Garden.Messages.ONRequired)
            {
                // Chiamata a arduino per apertura
                _waterController.Open();
                _mqttService.Publish(Constants.Garden.Queues.Water, Constants.Garden.Messages.ON);
            }
        
            _logger.LogDebug($"Recv:{ASCIIEncoding.ASCII.GetString(evt.Message)}");
        }

        public void ForceStop()
        {
            _mqttService.Publish(Constants.Garden.Queues.Water, Constants.Garden.Messages.OFFRequired);
        }

        public void ForceStart()
        {
            _mqttService.Publish(Constants.Garden.Queues.Water, Constants.Garden.Messages.ONRequired);
        }
    }
}