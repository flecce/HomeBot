using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Times
{
    public class RealTimeService : ITimeService
    {
        public DateTime GetCurrentDate()
        {
            return DateTime.Now.Date;
        }

        public DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
        }
    }
}
