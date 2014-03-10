using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Probe.Game;

namespace Probe.Utility
{
    public interface IKeyCombinationListener
    {
        void ProcessKeysMatchEvent(List<KeyboardEventContext> matchedCodes);
        void ProcessKeysMismatchEvent(List<KeyboardEventContext> matched);
    }

    public struct KeyboardEventContext
    {
        public int EventCode;
        public object Data;
    }

    internal struct KeyEventListenEntry
    {
        public KeyList KeyCombination;
        public IKeyCombinationListener Listener;
        public KeyboardEventContext context;
    }

    class KeyboardEventsHandler
    {
        #region singleton
        private static readonly KeyboardEventsHandler _instance = new KeyboardEventsHandler();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static KeyboardEventsHandler()
        {
        }

        public static KeyboardEventsHandler Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        KeyboardEventsHandler()
        {
            Hook = new UserActivityHook(false, false);
            Hook.KeyUp += HookKeyUp;
            Hook.KeyDown += HookKeyDown;
            Hook.OnMouseActivity += hook_OnMouseActivity;
        }

        public KeyList CurrentKeyCombination = new KeyList();
        public UserActivityHook Hook;
        private List<KeyEventListenEntry> _listenFor = new List<KeyEventListenEntry>();
        private List<int> _alreadyMatched = new List<int>();

        public void Start()
        {
            Hook.Start(false, true);
        }

        public void Stop()
        {
            Hook.Stop();
            Clear();
        }

        private void hook_OnMouseActivity(object sender, MouseEventArgs e)
        {
            if (!GameClock.Instance.Running) return;
            UIController.ProcessMouseMove(e);
            //rightClickWatcher.ProcessMouseEvent(e, _currentKeysDown);
        }

        private void HookKeyUp(object sender, KeyEventArgs e)
        {
            var key = e.KeyCode;
            CurrentKeyCombination.Remove(key);
            if (KeysHelper.KeysSynonyms.ContainsKey(key)) CurrentKeyCombination.Remove(KeysHelper.KeysSynonyms[key]);

            var removeMatched = new List<int>();
            var d = new Dictionary<IKeyCombinationListener, List<KeyboardEventContext>>();

            foreach (int i in _alreadyMatched)
            {
                if (CurrentKeyCombination.FastContains(_listenFor[i].KeyCombination)) continue;
                if (!d.ContainsKey(_listenFor[i].Listener))
                    d[_listenFor[i].Listener] = new List<KeyboardEventContext>();
                d[_listenFor[i].Listener].Add(_listenFor[i].context);
                removeMatched.Add(i);
            }
            
            foreach (KeyValuePair<IKeyCombinationListener, List<KeyboardEventContext>> pair in d)
                pair.Key.ProcessKeysMismatchEvent(pair.Value);

            _alreadyMatched = _alreadyMatched.Except(removeMatched).ToList();
        }

        private void HookKeyDown(object sender, KeyEventArgs e)
        {
            var key = e.KeyCode;
            if (CurrentKeyCombination.Contains(key)) return;
            CurrentKeyCombination.Add(key);
            if (KeysHelper.KeysSynonyms.ContainsKey(key)) CurrentKeyCombination.Add(KeysHelper.KeysSynonyms[key]);

            var d = new Dictionary<IKeyCombinationListener, List<KeyboardEventContext>>();
            for (int i = 0; i < _listenFor.Count; i++)
            {
                if (_alreadyMatched.Contains(i)) continue;
                if (!CurrentKeyCombination.FastContains(_listenFor[i].KeyCombination)) continue;
                if (!d.ContainsKey(_listenFor[i].Listener))
                    d[_listenFor[i].Listener] = new List<KeyboardEventContext>();
                d[_listenFor[i].Listener].Add(_listenFor[i].context);
                _alreadyMatched.Add(i);
            }

            foreach (KeyValuePair<IKeyCombinationListener, List<KeyboardEventContext>> pair in d)
                pair.Key.ProcessKeysMatchEvent(pair.Value);
        }

        public void AddListener(KeyList combination, int code, IKeyCombinationListener listener)
        {
            var e = new KeyEventListenEntry() { Listener = listener, KeyCombination = combination, context = new KeyboardEventContext() { EventCode = code } };
            _listenFor.Add(e);
        }

        public void AddListener(KeyList combination, int code, object context, IKeyCombinationListener listener)
        {
            var e = new KeyEventListenEntry() { Listener = listener, KeyCombination = combination, context = new KeyboardEventContext() { EventCode = code, Data = context } };
            _listenFor.Add(e);
        }

        public void Clear()
        {
            CurrentKeyCombination.Clear();
            _listenFor.Clear();
            _alreadyMatched.Clear();
        }
    }
}
