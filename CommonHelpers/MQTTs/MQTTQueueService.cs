using M2Mqtt;
using M2Mqtt.Messages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CommonHelpers.MQTTs
{
    public class MQTTQueueService : IMQTTQueueService
    {
        private MqttClient _MQTTclient = null;
        private Dictionary<string, List<Action<MqttMsgPublishEventArgs>>> _subscribers = new Dictionary<string, List<Action<MqttMsgPublishEventArgs>>>();
        private readonly string _mqttBroker;
        private readonly ILogger<MQTTQueueService> _logger;

        public MQTTQueueService(string mqttBroker, ILogger<MQTTQueueService> logger)
        {
            _logger = logger;
            _mqttBroker = mqttBroker;
            _tryConnect();
        }

        private void _tryConnect()
        {
            while (true)
            {
                _logger.LogDebug("Try connect");

                if (_MQTTclient == null || !_MQTTclient.IsConnected)
                {
                    try
                    {
                        _MQTTclient = new MqttClient(_mqttBroker);
                        _MQTTclient.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                        _MQTTclient.ConnectionClosed += _MQTTclient_ConnectionClosed;
                        _MQTTclient.Connect("TelegramBot" + DateTime.Now.Ticks.ToString());
                        _logger.LogDebug("Connected");
                    }
                    catch
                    {
                        Thread.Sleep(5000);
                    }
                }
                else
                {
                    break;
                }
            }
        }

        private void _MQTTclient_ConnectionClosed(object sender, EventArgs e)
        {
            _MQTTclient = null;

            Task.Factory.StartNew(() =>
            {
                _tryConnect();
            });
        }

        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (_subscribers.ContainsKey(e.Topic))
            {
                _subscribers[e.Topic].ForEach(evt => evt(e));
            }
        }

        public void AddSubscriber(string queueName, Action<MqttMsgPublishEventArgs> messageCallBack)
        {
            if (!_subscribers.ContainsKey(queueName))
            {
                _subscribers.Add(queueName, new List<Action<MqttMsgPublishEventArgs>>());
                _MQTTclient.Subscribe(new string[] { queueName }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }

            _subscribers[queueName].Add(messageCallBack);
        }

        public void RemoveSubscriber(string queueName, Action<MqttMsgPublishEventArgs> messageCallBack)
        {
            if (_subscribers.ContainsKey(queueName))
            {
                _subscribers[queueName].Remove(messageCallBack);
            }
        }

        public void Publish(string queueName, byte[] data)
        {
            _MQTTclient.Publish(queueName, data);
        }
    }
}