using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Intefaces;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Times;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace CommonHelpers.Inverters.Persisters
{
    public class PersisterFactory : IPersisterFactory
    {
        private static List<IPersister> _persisterList = new List<IPersister>();

        public PersisterFactory(IConfigurationService configService, ITimeService timeService, ILogger<PersisterFactory> logger)
        {
            //_persisterList.Add(new FilePersister(configService, timeService));
            //_persisterList.Add(new PVOutputPersister_addstatus());
            _persisterList.Add(new PVOutputPersister_addbatchstatus(timeService, ServiceFactory.CurrentServiceProvider.GetService<ILogger<PVOutputPersister_addbatchstatus>>()));
            //_persisterList.Add(new EmonCMSPersister(timeService));
        }

        public void Save(IInverter inv, ConverterStatus? value)
        {
            if (value.HasValue)
            {
                _persisterList.ForEach(delegate (IPersister p) { p.Save(inv, value.Value); });
            }
        }
    }

    public interface IPersisterFactory
    {
        void Save(IInverter inv, ConverterStatus? value);
    }
}