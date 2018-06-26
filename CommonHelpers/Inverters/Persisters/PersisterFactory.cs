using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Intefaces;
using CommonHelpers.Inverters.Interfaces;
using CommonHelpers.Times;
using System.Collections.Generic;

namespace CommonHelpers.Inverters.Persisters
{
    public class PersisterFactory : IPersisterFactory
    {
        private static List<IPersister> _persisterList = new List<IPersister>();

        public PersisterFactory(IConfigurationService configService, ITimeService timeService)
        {
            //_persisterList.Add(new FilePersister(configService, timeService));
            //_persisterList.Add(new PVOutputPersister_addstatus());
            _persisterList.Add(new PVOutputPersister_addbatchstatus(timeService));
            //_persisterList.Add(new EmonCMSPersister(timeService));
        }

        public  void Save(IInverter inv, ConverterStatus? value)
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