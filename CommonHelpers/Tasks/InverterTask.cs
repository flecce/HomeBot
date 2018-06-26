using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Inverters.Persisters;
using CommonHelpers.Inverters.Plugins.Fimer;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;

namespace CommonHelpers.Tasks
{
    public class InverterTask : ITask
    {
        public void Init()
        {
        }

        public void Run()
        {
            IInverter currentInverter = new FimerR25Inverter();
            Hashtable ht = new Hashtable
                {
                    { InverterCommonProperties.NET_IP_ADDRESS, "192.168.0.99" },
                    { InverterCommonProperties.NET_IP_PORT, "33330" },
                    { InverterCommonProperties.SERIAL_NUMBER, "18344" }
                };

            //LogFactory.GetLog().ForceWriteToLog(TraceEventType.Verbose, String.Format("Connect to: {0}", ht[InverterCommonProperties.NET_IP_ADDRESS]));
            //LogFactory.GetLog().ForceWriteToLog(TraceEventType.Verbose, String.Format("Port: {0}", ht[InverterCommonProperties.NET_IP_PORT]));
            //LogFactory.GetLog().ForceWriteToLog(TraceEventType.Verbose, String.Format("Serial : {0}", ht[InverterCommonProperties.SERIAL_NUMBER]));

            Ping p = new Ping();

            PingReply pr = p.Send(IPAddress.Parse((string)ht[InverterCommonProperties.NET_IP_ADDRESS]), 5000);
            if (pr.Status == IPStatus.Success)
            {
                //_alreadyAlive.SendOnlyOneTimesByDay("Notifica inverter", "Ciao sono di nuovo attivo.", _adminMail);
                //_offline.Reset();

                currentInverter.Init(ht);
                TransmissionState ts = currentInverter.Connect();

                if (ts == TransmissionState.Ok)
                {
                    // LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "start reading...");

                    //LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "Reading...");
                    ConverterStatus? cs = currentInverter.ReadData(31);

                    //LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "Reading status:" + (cs != null).ToString());
                    if (cs == null)
                    {
                        //LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "Nessun dato letto");
                        currentInverter.Dispose();
                    }
                    ServiceFactory.CurrentServiceProvider.GetService<IPersisterFactory>().Save(currentInverter, cs);
                }
            }
        }
    }
}