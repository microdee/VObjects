using System;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.VObjects
{
    public class PrimitiveObject : VPathQueryable
    {
        public Dictionary<string, List<object>> Fields = new Dictionary<string, List<object>>();

        public PrimitiveObject() { }
        public void Add(string name, object Field)
        {
            if (!this.Fields.ContainsKey(name))
            {
                if (Field is List<object>)
                {
                    List<object> objl = Field as List<object>;
                    if (TypeIdentity.Instance.ContainsKey(objl[0].GetType()))
                    {
                        this.Fields.Add(name, (List<object>)Field);
                        return;
                    }
                }
                if (TypeIdentity.Instance.ContainsKey(Field.GetType()))
                {
                    List<object> obj = new List<object>();
                    obj.Add(Field);
                }
            }
        }
        public string GetConfig()
        {
            string conf = "";
            foreach(KeyValuePair<string, List<object>> kvp in this.Fields)
            {
                conf += kvp.Value[0].GetType().ToString() + " ";
                conf += kvp.Key + ", ";
            }
            return conf;
        }

        protected void DisposeDisposable(List<object> objl)
        {
            if (objl[0] is IDisposable)
            {
                foreach (object o in objl)
                {
                    var t = o as IDisposable;
                    t.Dispose();
                }
            }
        }

        public List<object> this[string name]
        {
            get
            {
                if (this.Fields.ContainsKey(name)) return this.Fields[name];
                else return null;
            }
            set { if(this.Fields.ContainsKey(name)) this.Fields[name] = value; }
        }
        public void Clear()
        {
            foreach(KeyValuePair<string, List<object>> kvp in this.Fields)
            {
                DisposeDisposable(kvp.Value);
            }
            this.Fields.Clear();
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
                List<string> ToBeRemoved = new List<string>();
                foreach (KeyValuePair<string, List<object>> kvp in this.Fields)
                {
                    if(kvp.Key.Contains(key)) ToBeRemoved.Add(kvp.Key);
                }
                foreach(string k in ToBeRemoved)
                {
                    DisposeDisposable(Fields[k]);
                    this.Fields.Remove(k);
                }
                ToBeRemoved.Clear();
            }
        }

        public override object VPathGetItem(string key)
        {
            if (this.Fields.ContainsKey(key))
                return this.Fields[key];
            else return null;
        }

        public override string[] VPathQueryKeys()
        {
            return this.Fields.Keys.ToArray();
        }
    }
}
