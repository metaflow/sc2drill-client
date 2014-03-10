using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Probe.Utility
{
    public class KeyList : List<Keys>
    {
        private int _mask;

        public KeyList()
        {
            UpdateMask();
        }
        
        public override string ToString()
        {
            var l = new List<string>();
            var c = new List<Keys>();
            foreach (Keys k in this)
            {
                if (KeysHelper.KeysSynonyms.ContainsKey(k))
                {
                    c.Add(KeysHelper.KeysSynonyms[k]);
                }
                else
                {
                    c.Add(k);
                }
            }
            c.Sort((a, b) =>
            {
                if (a == b) return 0;
                var wa = (KeysHelper.IsAlpha(a) ? 2 : 0) + (KeysHelper.IsNumeric(a) ? 1 : 0) + (KeysHelper.IsModifier(a) ? -1 : 0);
                var wb = (KeysHelper.IsAlpha(b) ? 2 : 0) + (KeysHelper.IsNumeric(b) ? 1 : 0) + (KeysHelper.IsModifier(b) ? -1 : 0);
                if (wa != wb) return wa.CompareTo(wb);
                return a.CompareTo(b);
            });
            foreach (Keys k in c)
            {
                l.Add(KeysHelper.KeyToString(k));
            }
            return string.Join(" + ", l.ToArray());
        }

        private void UpdateMask()
        {
            _mask = 0;
            foreach (Keys k in this)
            {
                _mask = _mask | (1 << ((int)k % 32));
            }
        }

        public bool Contains(KeyList keyList)
        {
            return (this.Intersect(keyList).Count() == keyList.Count);
        }

        public bool FastContains(KeyList keyList)
        {
            return ((_mask & keyList._mask) == keyList._mask) && Contains(keyList);
        }

        public new void Add(Keys key)
        {
            base.Add(key);
            UpdateMask();
        }

        public new void Clear()
        {
            base.Clear();
            UpdateMask();
        }

        public new bool Remove(Keys item)
        {
            var r = base.Remove(item);
            UpdateMask();
            return r;
        }

        public new void Insert(int index, Keys item)
        {
            base.Insert(index, item);
            UpdateMask();
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
            UpdateMask();
        }

        new Keys this[int index]
        {
            get { return base[index]; }
            set { base[index] = value; UpdateMask(); }
        }
    };
}