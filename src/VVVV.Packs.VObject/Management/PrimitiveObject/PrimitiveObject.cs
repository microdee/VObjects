using System;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.VObjects
{
    public class PrimitiveObject : VPathQueryable
    {
        public Dictionary<string, List<object>> Fields = new Dictionary<string, List<object>>();
        
        public void Add(string name, object Field)
        {
            if (!this.Fields.ContainsKey(name))
            {
                if (Field is List<object>)
                {
                    Fields.Add(name, (List<object>)Field);
                    return;
                }
                List<object> obj = new List<object>();
                obj.Add(Field);
                Fields.Add(name, obj);
            }
        }
        public string GetConfig()
        {
            string conf = "";
            foreach(KeyValuePair<string, List<object>> kvp in this.Fields)
            {
                conf += kvp.Key + ", ";
            }
            return conf;
        }

        protected void DisposeDisposable(List<object> objl)
        {
            foreach (object o in objl)
            {
                var t = o as IDisposable;
                t?.Dispose();
            }
        }

        public List<object> this[string name]
        {
            get
            {
                if (Fields.ContainsKey(name)) return Fields[name];
                return null;
            }
            set { if(Fields.ContainsKey(name)) Fields[name] = value; }
        }
        public void Clear()
        {
            foreach(KeyValuePair<string, List<object>> kvp in Fields)
            {
                DisposeDisposable(kvp.Value);
            }
            Fields.Clear();
        }

        public void Remove(string key, bool match)
        {
            if(match)
            {
                if (Fields.ContainsKey(key))
                {
                    DisposeDisposable(Fields[key]);
                    Fields.Remove(key);
                }
            }
            else
            {
                var ToBeRemoved = (from kvp in this.Fields where kvp.Key.Contains(key) select kvp.Key).ToList();
                foreach(var k in ToBeRemoved)
                {
                    DisposeDisposable(Fields[k]);
                    this.Fields.Remove(k);
                }
                ToBeRemoved.Clear();
            }
        }

        public override object VPathGetItem(string key)
        {
            return Fields.ContainsKey(key) ? Fields[key] : null;
        }

        public override string[] VPathQueryKeys()
        {
            return Fields.Keys.ToArray();
        }
    }
}
