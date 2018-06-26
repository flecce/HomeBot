using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers
{
    public static class ServiceFactory
    {
        public static IServiceProvider CurrentServiceProvider { get; set; }
    }
}
