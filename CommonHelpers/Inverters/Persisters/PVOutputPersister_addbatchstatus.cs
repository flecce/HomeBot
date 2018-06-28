using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Times;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CommonHelpers.Inverters.Persisters
{
    internal class PVOutputPersister_addbatchstatus : WebPersisterBase
    {
        private DateTime _lastAdd = DateTime.MinValue;
        private readonly ITimeService _timeService;
        //private double _sendIntervalSeconds = 90;
        //private bool _alreadyReadySummary = false;
        private CurrentInformation _localData = new CurrentInformation();

        private readonly ILogger<PVOutputPersister_addbatchstatus> _logger;
        internal PVOutputPersister_addbatchstatus(ITimeService timeService, ILogger<PVOutputPersister_addbatchstatus> logger)
        {
            _timeService = timeService;
            _logger = logger;
            _logger.LogDebug($"PVOutputPersister_addbatchstatus ctor");
        }

        public override bool Save(IInverter inv, ConverterStatus value)
        {
            if (_timeService.GetCurrentDateTime().Subtract(_lastAdd).TotalSeconds < 10) return false;

            ushort uiValue = (ushort)value.TypeStatus.GetProperty(CommonPropertyType.ProduzioneCorrente);
            float temp1 = (float)value.TypeStatus.GetProperty(CommonPropertyType.Temperatura1);

            _localData.CurrentProductions.Add((int)(value.CommonStatus.EnergieTag * 1000));
            _localData.CurrentPowers.Add((int)uiValue);
            _localData.Taketimes.Add(_timeService.GetCurrentDateTime());
            _localData.Temps.Add(temp1);
            _lastAdd = _timeService.GetCurrentDateTime();
            _logger.LogDebug($"Data added: Count={_localData.CurrentPowers.Count}");
            if (_localData.CurrentPowers.Count >= 2)
            {
                lock (this)
                {
                    _logger.LogDebug("Comunicate to pvoutput for addbatchstatus");

                    string key = "key=024fe8dd52991266562af15510ab1a999c927805&sid=9627";
                    List<string> sb = new List<string>();

                    for (int index = 0; index < _localData.CurrentPowers.Count; index++)
                    {
                        sb.Add(String.Format("{0:yyyyMMdd},{1:HH:mm},{2},{3},{4},{5},{6},{7}",
                                                                _localData.Taketimes[index],
                                                                _localData.Taketimes[index],
                                                                _localData.CurrentProductions[index],
                                                                (int)_localData.CurrentPowers[index],
                                                                -1,
                                                                -1,
                                                                _localData.Temps[index].ToString("0.0", new System.Globalization.CultureInfo("en-US")),
                                                                value.TypeStatus.GetProperty(Enums.CommonPropertyType.UAC)
                                                                ));
                    }

                    string statusParameters = "data=" + String.Join(";", sb.ToArray());
                    string statusURI = "http://pvoutput.org/service/r2/addbatchstatus.jsp?";
                    string statusUrl = statusURI + key + "&" + statusParameters;
                    _logger.LogDebug("status url:" + statusUrl);
                    string responseValue = "";
                    bool response = _doGETRequest(statusUrl, ref responseValue);

                    _localData.Clear();
                    return response;
                }
            }
            return false;
        }

        protected override bool _checkResponse(string resp)
        {
            _logger.LogDebug("  risposta:" + resp);
            return true;
        }
    }
}