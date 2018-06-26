using CommonHelpers.Inverters.Enums;

namespace CommonHelpers.Inverters.Interfaces
{
    public interface ITypeStatus
    {
        object GetProperty(CommonPropertyType t);

        bool PropertyIsSupported(CommonPropertyType t);
    }
}