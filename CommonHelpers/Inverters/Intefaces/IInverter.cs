using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Events;
using System;
using System.Collections;

namespace CommonHelpers.Inverters.Interfaces
{
    public interface IInverter : IDisposable
    {
        TransmissionState Connect();

        event EventHandlers.DataReceivedEventHandler DataReceived;

        void Init(Hashtable ps);

        ConverterStatus? ReadData(uint readTipology);

        string SerialeNumber { get; }
    }
}