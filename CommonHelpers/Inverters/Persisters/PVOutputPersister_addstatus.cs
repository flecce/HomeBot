using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Times;
using System;
using System.Collections.Generic;

namespace CommonHelpers.Inverters.Persisters
{
    internal class PVOutputPersister_addstatus : WebPersisterBase
    {
        private DateTime _lastSend = DateTime.MinValue;
        private double _sendIntervalSeconds = 90;
        private readonly ITimeService _timeService;

        //private bool _alreadyReadySummary = false;
        private Dictionary<string, CurrentInformation> _currentInformations = new Dictionary<string, CurrentInformation>();

        internal PVOutputPersister_addstatus(ITimeService timeService)
        {
            _timeService = timeService;
        }

        public override bool Save(IInverter inv, ConverterStatus value)
        {
            if (_currentInformations.ContainsKey(inv.SerialeNumber) == false)
            {
                _currentInformations.Add(inv.SerialeNumber, new CurrentInformation()
                {
                    CurrentDay = _timeService.GetCurrentDate().Day
                });
            }

            CurrentInformation ci = _currentInformations[inv.SerialeNumber];

            ushort uiValue = (ushort)value.TypeStatus.GetProperty(CommonPropertyType.ProduzioneCorrente);
            float temp1 = (float)value.TypeStatus.GetProperty(CommonPropertyType.Temperatura1);

            ci.CurrentProduction = (int)(value.CommonStatus.EnergieTag * 1000);
            ci.CurrentPowers.Add((int)uiValue);
            ci.Temps.Add(temp1);

            if (_timeService.GetCurrentDateTime().Subtract(_lastSend).TotalSeconds > _sendIntervalSeconds)
            {
                // LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "Comunicate to pvoutput for addstatus");

                int prodGiornaliera = ci.CurrentProduction;

                string key = "key=024fe8dd52991266562af15510ab1a999c927805&sid=9627";
                string statusParameters = String.Format("d={0:yyyyMMdd}&t={2:HH:mm}&v1={1}&v2={3}&v5={4}&v6={5}",
                                                            _timeService.GetCurrentDate(),
                                                            prodGiornaliera,
                                                            _timeService.GetCurrentDateTime(),
                                                            ci.MaxPower,
                                                            ci.AverangeTemps.ToString("0.0", new System.Globalization.CultureInfo("en-US")),
                                                            value.TypeStatus.GetProperty(Enums.CommonPropertyType.UAC)
                                                            );
                string statusURI = "http://pvoutput.org/service/r2/addstatus.jsp?";
                string statusUrl = statusURI + key + "&" + statusParameters;
                // LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "status url:" + statusUrl);
                string responseValue = "";
                bool response = _doGETRequest(statusUrl, ref responseValue);

                _lastSend = _timeService.GetCurrentDateTime();
                ci.CurrentPowers.Clear();

                return response;
            }
            return false;
        }

        protected override bool _checkResponse(string resp)
        {
            return resp.StartsWith("OK 200:");
        }
    }
}