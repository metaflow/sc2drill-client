using System;

namespace Probe.BuildOrders
{
    public class BuildOrderStep
    {
        public TimeSpan Time { get; set; }
        public string Message { get; set; }
        public int Index { get; set; }
    }
}
