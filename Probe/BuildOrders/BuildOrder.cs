using System;
using System.Collections.Generic;

namespace Probe.BuildOrders
{
    public class BuildOrder : List<BuildOrderStep>
    {
        public string Title;

        public enum BuildOrderExecutionType
        {
            Time,
            Step
        }
        public BuildOrderExecutionType ExecutionExecutionType { get; set; }
    }
}
