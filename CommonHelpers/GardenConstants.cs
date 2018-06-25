using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers
{
    public static class Constants
    {
        public static class Garden
        {
            public static class Queues
            {
                public const string Water = "/home/garden/water";
                public const string Logs = "/home/garden/logs";
            }
        }

        public static class Power
        {
            public static class Queues
            {
                public const string Production = "/home/power/production";
                public const string Consumation = "/home/power/consumation";
                public const string Logs = "/home/garden/logs";
            }
        }
    }
}
