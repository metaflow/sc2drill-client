using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Probe.Game;
using Probe.Utility;

namespace Probe.Estimators
{
    class RightClickWatcher
    {
        private bool _rightClickSequenced;
        private int _latestAllClicks;
        private int _latestRepeatedClicks;

        public void ProcessMouseEvent(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None) return;
            if (e.Button != MouseButtons.Right)
            {
                _rightClickSequenced = false;
                return;
            }
            _latestAllClicks++;

            if (_rightClickSequenced && !KeyboardEventsHandler.Instance.CurrentKeyCombination.Contains(Keys.Shift))
                _latestRepeatedClicks++;

            if (_latestAllClicks >= 20)
                SendCurrentPart();
            _rightClickSequenced = true;
        }

        private void SendCurrentPart()
        {
            GameLog.Instance.AddEntry("right clicks", "part", new JArray() { _latestAllClicks, _latestRepeatedClicks });
            _latestAllClicks = 0;
            _latestRepeatedClicks = 0;
        }

        public void ProcessKeyDown(Keys key)
        {
            _rightClickSequenced = false;
        }

        public void Start()
        {
            _rightClickSequenced = false;
            _latestAllClicks = 0;
            _latestRepeatedClicks = 0;
        }

        public void Stop()
        {
            SendCurrentPart();
        }
    }
}
