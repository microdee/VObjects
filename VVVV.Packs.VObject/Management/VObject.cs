using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VVVV.Packs.VObjects
{
    public class VObjectField : IVPathQueryable
    {
        public string Name;
        public List<object> Objects = new List<object>();
        public Type CommonType;

        public object this[int i]
        {
            get { return Objects[i]; }
            set
            {
                Objects[i] = value;
                UpdateCommonType();
            }
        }

        public void UpdateCommonType()
        {
            if(Objects.Count == 0) return;
            Type type = Objects[0].GetType();
            if (Objects.Count > 1)
            {
                object po = Objects[0];
                for (int i = 1; i < Objects.Count; i++)
                {
                    if (!type.IsInstanceOfType(Objects[i]))
                    {
                        foreach (var T in Objects[i].GetType().GetTypes())
                        {
                            if (T.IsInstanceOfType(po))
                            {
                                type = T;
                                break;
                            }
                        }
                    }
                    po = Objects[i];
                }
            }
            CommonType = type;
        }

        public string[] VPathQueryKeys()
        {
            var res = new List<string>();
            for(int i=0; i<Objects.Count; i++)
                res.Add(i.ToString());
            return res.ToArray();
        }

        public object VPathGetItem(string key)
        {
            return Objects[int.Parse(key)];
        }
    }
    public class VObject : IVPathQueryable
    {
        public Dictionary<string, VObjectField> Fields = new Dictionary<string, VObjectField>();
        
        public void Add(string name, object Field)
        {
            if (!this.Fields.ContainsKey(name))
            {
                if (Field is VObjectField)
                {
                    Fields.Add(name, (VObjectField)Field);
                }
                var vf = new VObjectField();
                vf.Name = name;
                if (Field is List<object>)
                {
                    vf.Objects = Field as List<object>;
                    vf.UpdateCommonType();
                }
                else
                {
                    vf.Objects.Add(Field);
                }
                Fields.Add(name, vf);
            }
        }
        public void Add(VObjectField Field)
        {
            if (!this.Fields.ContainsKey(Field.Name))
            {
                Fields.Add(Field.Name, Field);
            }
        }
        public string GetConfig()
        {
            string conf = "";
            foreach(var k in Fields.Keys)
            {
                conf += k + ", ";
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

        public VObjectField Get(string name)
        {
            if (Fields.ContainsKey(name)) return Fields[name];
            return null;
        }
        public void Set(string name, object value)
        {
            if (Fields.ContainsKey(name))
            {
                if (value is VObjectField)
                {
                    Fields[name] = (VObjectField)value;
                }
                if (value is IList)
                {
                    var objl = (IList)value;
                    Fields[name].Objects.Clear();
                    foreach (var o in objl)
                    {
                        Fields[name].Objects.Add(o);
                    }
                    Fields[name].UpdateCommonType();
                }
            }
        }

        public void Clear()
        {
            foreach(var f in Fields.Values)
            {
                DisposeDisposable(f.Objects);
            }
            Fields.Clear();
        }

        public void Remove(string key, bool match)
        {
            if(match)
            {
                if (Fields.ContainsKey(key))
                {
                    DisposeDisposable(Fields[key].Objects);
                    Fields.Remove(key);
                }
            }
            else
            {
                var ToBeRemoved = (from kvp in Fields where kvp.Key.Contains(key) select kvp.Key).ToList();
                foreach(var k in ToBeRemoved)
                {
                    DisposeDisposable(Fields[k].Objects);
                    this.Fields.Remove(k);
                }
                ToBeRemoved.Clear();
            }
        }

        public object VPathGetItem(string key)
        {
            return Fields.ContainsKey(key) ? Fields[key] : null;
        }

        public string[] VPathQueryKeys()
        {
            return Fields.Keys.ToArray();
        }
    }
}
