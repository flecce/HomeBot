using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Times
{
    public interface ITimeService
    {
        DateTime GetCurrentDateTime();
        DateTime GetCurrentDate();
    }
}
