using M2Mqtt.Messages;
using System;

namespace CommonHelpers.MQTTs
{
    public interface IMQTTQueueService
    {
        void AddSubscriber(string queueName, Action<MqttMsgPublishEventArgs> messageCallBack);

        void RemoveSubscriber(string queueName, Action<MqttMsgPublishEventArgs> messageCallBack);

        void Publish(string queueName, byte[] data);
    }
}