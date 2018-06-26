using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Times;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommonHelpers.Inverters.Persisters
{
    internal class EmonCMSPersister : WebPersisterBase
    {
        private DateTime _lastSend = DateTime.MinValue;
        private double _sendIntervalSeconds = 90;
        private readonly ITimeService _timeService;

        //private bool _alreadyReadySummary = false;
        private Dictionary<string, CurrentInformation> _currentInformations = new Dictionary<string, CurrentInformation>();

        internal EmonCMSPersister(ITimeService timeService)
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
               // LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "Comunicating... to emoncms for post");

                int prodGiornaliera = ci.CurrentProduction;

                string statusURI = "http://emoncms.org/input/post?apikey=cfe3a5e1be7b49c6d6a1aa372f32ec3a&json={invtotgen:" + ci.CurrentProduction.ToString() + ",invcurpwd:" + ci.MaxPower.ToString() + "}";
                //string statusUrl = String.Format(statusURI, ci.CurrentProduction, ci.MaxPower);
               // LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "status url:" + statusURI);
                string responseValue = "";
                bool response = _doGETRequest(statusURI, ref responseValue);
               // LogFactory.GetLog().WriteToLog(TraceEventType.Verbose, "  Comunication ok");

                _lastSend = _timeService.GetCurrentDateTime();
                ci.CurrentPowers.Clear();

                return response;
            }
            return false;
        }

        protected override bool _checkResponse(string resp)
        {
            return resp.StartsWith("ok");
        }
    }
}