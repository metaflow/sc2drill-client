using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Probe.Utility;

namespace Probe.Tools
{
    public static class ToolManager
    {
        private static readonly Dictionary<string, BackgroundWorker> _starters = new Dictionary<string, BackgroundWorker>();

        static ToolManager()
        {
            Controllers = new List<IToolController>();

            //add controllers
            Controllers.Add(new SCFusionController());
            Controllers.Add(new SC2GearsController()); 
            Controllers.Add(new WebToolController("Online BO Calculator", "http://sc2calc.org/build_order/"));
        }

        public static List<IToolController> Controllers { get; private set; }

        public static void Run(string name)
        {
            if (!_starters.ContainsKey(name)) _starters.Add(name, null);

            //check that tool going to start.
            if(_starters[name] != null && _starters[name].IsBusy)  return;

            _starters[name] = new BackgroundWorker();
            _starters[name].DoWork += worker_DoWork;
            _starters[name].RunWorkerAsync(name);
        }

        static void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var name = (string)e.Argument;

            try
            {
                var controller = Controllers.Where(c => c.Name == name).FirstOrDefault();

                if (controller == null) return;

                if (!controller.IsInstalled || controller.HasUpdates) controller.Install();

                controller.Run();
            }
            catch (Exception ex)
            {
                CustomEvents.Instance.AddException(EventsType.GeneralError, string.Format("[{0}] tool starting error.", name),ex);
            }
        }
    }
}
