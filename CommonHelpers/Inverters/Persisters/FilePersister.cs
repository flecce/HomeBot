using CommonHelpers.Inverters.Intefaces;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Times;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;

namespace CommonHelpers.Inverters.Persisters
{
    internal class FilePersister : IPersister
    {
        #region IPersister Members

        private static object _syncObject = new object();
        private DateTime _lastSend = DateTime.MinValue;
        private double _sendIntervalSeconds = 10;
        private readonly IConfigurationService _configService;
        private readonly ITimeService _timeService;
        private readonly ILogger<FilePersister> _logger;
        public FilePersister(IConfigurationService configService, ITimeService timeService, ILogger<FilePersister> logger)
        {
            _configService = configService;
            _timeService = timeService;
            _logger = logger;
        }
        public bool Save(IInverter inv, Enums.ConverterStatus value)
        {
            if (_timeService.GetCurrentDateTime().Subtract(_lastSend).TotalSeconds > _sendIntervalSeconds)
            {
                _logger.LogDebug("Save data to file");

                lock (_syncObject)
                {
                    string baseDir = _configService.GetConfigValue("Persisters.File.DataPath", null);
                    string dayFn = _timeService.GetCurrentDate().ToString("yyyy_MM_dd");
                    string dataFile = Path.Combine(baseDir, dayFn + ".txt");
                    if (File.Exists(dataFile) == false)
                    {
                        _prepareFile(dataFile);
                    }
                    string data = String.Format("{0};{1};{2};{3};{4};{5};{6};{7}",
                                                _timeService.GetCurrentDateTime().ToString("yyyy-MM-dd-HH:mm:ss"),
                                                (int)(value.CommonStatus.EnergieTag * 1000),
                                                value.TypeStatus.GetProperty(Enums.CommonPropertyType.ProduzioneCorrente),
                                                value.TypeStatus.GetProperty(Enums.CommonPropertyType.Temperatura1),
                                                value.TypeStatus.GetProperty(Enums.CommonPropertyType.IAC),
                                                value.TypeStatus.GetProperty(Enums.CommonPropertyType.IDC),
                                                value.TypeStatus.GetProperty(Enums.CommonPropertyType.UAC),
                                                value.TypeStatus.GetProperty(Enums.CommonPropertyType.UDC)
                                                );
                    using (StreamWriter sw = File.AppendText(dataFile))
                    {
                        sw.WriteLine(data);
                    }

                   _logger.LogDebug("Istantanea:{0} kWh, Prod giornaliera:{1} kWh", value.TypeStatus.GetProperty(Enums.CommonPropertyType.ProduzioneCorrente), value.CommonStatus.EnergieTag);
                }
                _lastSend = _timeService.GetCurrentDateTime();
            }
            return true;
        }

        private void _prepareFile(string dataFile)
        {
            using (File.Create(dataFile))
            { }
        }

        #endregion IPersister Members
    }
}