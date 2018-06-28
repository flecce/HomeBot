using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers
{
    public static class Constants
    {
        public static class Garden
        {
            public static class Messages
            {
                public const string ON = "ON";
                public const string OFF = "OFF";
                public const string ONRequired = "ON-REQUIRED";
                public const string OFFRequired = "OFF-REQUIRED";
            }

            public static class Queues
            {
                public const string Water = "/home/garden/water";
                public const string Logs = "/home/garden/logs";
            }
        }

        public static class Power
        {
            public static class Messages
            {
                public const string ProductionDataAcquired = "POWER_DATA_ACQUIRED";
                public const string ProductionDataRequest = "POWER_DATA_REQUEST";
            }

            public static class Queues
            {
                public const string Production = "/home/power/production";
                public const string Consumation = "/home/power/consumation";
                public const string Logs = "/home/garden/logs";
            }
        }
    }
}
