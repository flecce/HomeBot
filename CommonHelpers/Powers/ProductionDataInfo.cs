using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.Powers
{
    public class PowerDataBase
    { }

    public class ProductionDataInfo: PowerDataBase
    {
        public double DailyValue { get; set; }

        public double CurrentValue { get; set; }
    }
}
