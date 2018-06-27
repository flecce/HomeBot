using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Inverters.Persisters;
using CommonHelpers.Inverters.Plugins.Fimer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
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

            IInverter currentInverter = new FimerR25Inverter();
            Hashtable ht = new Hashtable
                {
                    { InverterCommonProperties.NET_IP_ADDRESS, "192.168.0.99" },
                    { InverterCommonProperties.NET_IP_PORT, "33330" },
                    { InverterCommonProperties.SERIAL_NUMBER, "18344" }
                };

            logger.LogDebug(String.Format("Connect to: {0}", ht[InverterCommonProperties.NET_IP_ADDRESS]));
            logger.LogDebug(String.Format("Port: {0}", ht[InverterCommonProperties.NET_IP_PORT]));
            logger.LogDebug(String.Format("Serial : {0}", ht[InverterCommonProperties.SERIAL_NUMBER]));

            Ping p = new Ping();

            PingReply pr = p.Send(IPAddress.Parse((string)ht[InverterCommonProperties.NET_IP_ADDRESS]), 5000);
            if (pr.Status == IPStatus.Success)
            {
                currentInverter.Init(ht);
                TransmissionState ts = currentInverter.Connect();

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
                }
            }
        }
    }
}