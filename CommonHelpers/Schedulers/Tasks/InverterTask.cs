using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Inverters.Persisters;
using CommonHelpers.Inverters.Plugins.Fimer;
using CommonHelpers.MQTTs;
using CommonHelpers.Powers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;

namespace CommonHelpers.Schedulers.Tasks
{
    public class InverterTask : ITask
    {
        public void Init()
        {
        }

        public void Run()
        {
            ILogger<InverterTask> logger = ServiceFactory.CurrentServiceProvider.GetService<ILogger<InverterTask>>();
            ILogger<FimerR25Inverter> loggerFimer = ServiceFactory.CurrentServiceProvider.GetService<ILogger<FimerR25Inverter>>();

            IMQTTQueueService mqttService = ServiceFactory.CurrentServiceProvider.GetService<IMQTTQueueService>();

            using (IInverter currentInverter = new FimerR25Inverter(loggerFimer))
            {
                Hashtable ht = new Hashtable
                {
                    { InverterCommonProperties.NET_IP_ADDRESS, "192.168.0.99" },
                    { InverterCommonProperties.NET_IP_PORT, "33330" },
                    { InverterCommonProperties.SERIAL_NUMBER, "18344" }
                };

                logger.LogDebug($"Connect to: {ht[InverterCommonProperties.NET_IP_ADDRESS]} - Port: {ht[InverterCommonProperties.NET_IP_PORT]} - Serial : {ht[InverterCommonProperties.SERIAL_NUMBER]}");

                Ping p = new Ping();

                PingReply pr = p.Send(IPAddress.Parse((string)ht[InverterCommonProperties.NET_IP_ADDRESS]), 5000);
                if (pr.Status == IPStatus.Success)
                {
                    logger.LogDebug("initing...");
                    currentInverter.Init(ht);
                    logger.LogDebug("connecting...");
                    TransmissionState ts = currentInverter.Connect();
                    logger.LogDebug("connected");
                    if (ts == TransmissionState.Ok)
                    {
                        logger.LogDebug("start reading...");

                        ConverterStatus? cs = currentInverter.ReadData(31);

                        logger.LogDebug("Reading status:" + (cs != null).ToString());
                        if (cs == null)
                        {
                            logger.LogDebug("Nessun dato letto");
                            currentInverter.Dispose();
                        }
                        ServiceFactory.CurrentServiceProvider.GetService<IPersisterFactory>().Save(currentInverter, cs);

                        MessageBase mb = new MessageBase
                        {
                            Command = Constants.Power.Messages.ProductionDataAcquired,
                            Data = new ProductionDataInfo
                            {
                                DailyValue = cs.Value.CommonStatus.EnergieTag * 1000,
                                CurrentValue = (ushort)cs.Value.TypeStatus.GetProperty(CommonPropertyType.ProduzioneCorrente)
                            }
                        };
                        var jsonSerializerSettings = new JsonSerializerSettings()
                        {
                            TypeNameHandling = TypeNameHandling.All
                        };
                        mqttService.Publish(Constants.Power.Queues.Production, JsonConvert.SerializeObject(mb, jsonSerializerSettings));
                    }
                    else
                    {
                        logger.LogDebug($"reading error:{ts}");
                    }
                }
            }
        }
    }
}