using System;
using System.Collections.Generic;
using System.Text;

namespace CommonHelpers.MQTTs
{
    public class MessageBase
    {
        public string Command { get; set; }
        public object Data { get; set; }
    }
}
