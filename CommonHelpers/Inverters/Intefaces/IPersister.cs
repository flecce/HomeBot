using CommonHelpers.Inverters.Enums;
using CommonHelpers.Inverters.Interfaces;

namespace CommonHelpers.Inverters.Intefaces
{
    public interface IPersister
    {
        bool Save(IInverter inv, ConverterStatus value);
    }
}