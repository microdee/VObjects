using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace VVVV.Packs.VObjects
{
    // class for high level and simple object emulation in VVVV
    public class VObjectCollection : VPathQueryable
    {
        public Dictionary<string, object> Children = new Dictionary<string, object>();
        public string Name = "";
        public Stopwatch Age = new Stopwatch();
        public string Debug = "";
        public bool Removing = false;
        public VObjectCollection()
        {
            this.Age.Reset();
            this.Age.Start();
        }

        protected void DisposeDisposable()
        {
            foreach (KeyValuePair<string, object> kvp in this.Children)
            {
                if (kvp.Value is IDisposable)
                {
                    ObjectHelper.DisposeDisposable(kvp.Value);
                }
            }
        }

        public void Add(string name, object obj)
        {
            if (!this.Children.ContainsKey(name))
            {
                this.Children.Add(name, obj);
            }
        }
        public void Clear()
        {
            DisposeDisposable();
            this.Children.Clear();
        }
        public void Remove(string key, bool match)
        {
            if (match)
            {
                if (this.Children.ContainsKey(key))
                {
                    ObjectHelper.DisposeDisposable(Children[key]);
                    this.Children.Remove(key);
                }
            }
            else
            {
                List<string> ToBeRemoved = new List<string>();
                foreach (KeyValuePair<string, object> kvp in this.Children)
                {
                    if (kvp.Key.Contains(key)) ToBeRemoved.Add(kvp.Key);
                }
                foreach (string k in ToBeRemoved)
                {
                    ObjectHelper.DisposeDisposable(Children[k]);
                    this.Children.Remove(k);
                }
                ToBeRemoved.Clear();
            }
        }
        public object this[string name]
        {
            get
            {
                if (this.Children.ContainsKey(name)) return this.Children[name];
                else return null;
            }
            set
            {
                if (this.Children.ContainsKey(name)) this.Children[name] = value;
            }
        }

        public override object VPathGetItem(string key)
        {
            if (this.Children.ContainsKey(key))
                return this.Children[key];
            else return null;
        }

        public override string[] VPathQueryKeys()
        {
            return this.Children.Keys.ToArray();
        }
    }
}
