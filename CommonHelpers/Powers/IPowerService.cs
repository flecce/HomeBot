using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Powers
{
    public interface IPowerService
    {
        void SubscribeOnValueAcquired(Action<PowerDataBase> onValueAcquiredAction);
    }
}
