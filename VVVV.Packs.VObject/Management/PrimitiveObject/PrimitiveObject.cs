using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Packs.VObjects
{
    public class PrimitiveObject
    {
        public Dictionary<string, ObjectTypePair> Fields = new Dictionary<string, ObjectTypePair>();

        public PrimitiveObject() { }
        public void Add(string name, object Field)
        {
            if (!this.Fields.ContainsKey(name))
            {
                if (Field is ObjectTypePair)
                {
                    this.Fields.Add(name, (ObjectTypePair)Field);
                    return;
                }
                if (TypeIdentity.Instance.ContainsKey(Field.GetType()))
                {
                    ObjectTypePair obj = new ObjectTypePair();
                    obj.Objects.Add(Field);
                    obj.Type = Field.GetType();
                }
            }
        }
        public void Add(string name, List<object> Field)
        {
            if (!this.Fields.ContainsKey(name))
            {
                if (TypeIdentity.Instance.ContainsKey(Field[0].GetType()))
                {
                    ObjectTypePair obj = new ObjectTypePair();
                    foreach (object o in Field)
                    {
                        obj.Objects.Add(o);
                    }
                    obj.Type = Field[0].GetType();
                    this.Fields.Add(name, obj);
                }
            }
        }
        public string GetConfig()
        {
            string conf = "";
            foreach(KeyValuePair<string, ObjectTypePair> kvp in this.Fields)
            {
                conf += kvp.Value.Type.ToString() + " ";
                conf += kvp.Key + ", ";
            }
            return conf;
        }

        public ObjectTypePair this[string name]
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
            foreach(KeyValuePair<string, ObjectTypePair> kvp in this.Fields)
            {
                kvp.Value.Dispose();
            }
            this.Fields.Clear();
        }

        public void Remove(string key, bool match)
        {
            if(match)
            {
                if (this.Fields.ContainsKey(key))
                    this.Fields.Remove(key);
            }
            else
            {
                List<string> ToBeRemoved = new List<string>();
                foreach (KeyValuePair<string, ObjectTypePair> kvp in this.Fields)
                {
                    if(kvp.Key.Contains(key)) ToBeRemoved.Add(kvp.Key);
                }
                foreach(string k in ToBeRemoved)
                {
                    this.Fields[k].Dispose();
                    this.Fields.Remove(k);
                }
                ToBeRemoved.Clear();
            }
        }
        public void VPath(string path, List<object> Results, string Separator)
        {
            string[] levels = path.Split(Separator.ToCharArray());
            string nextpath = string.Join(Separator, levels, 1, levels.Length - 1);
            if ((levels[0][0] == '"') && (levels[0][levels[0].Length - 1] == '"'))
            {
                string key = levels[0].Trim('"');
                if (this.Fields.ContainsKey(key))
                {
                    if (levels.Length == 1)
                    {
                        Results.Add(this.Fields[key]);
                        return;
                    }
                }
            }
            else
            {
                Regex Pattern = new Regex(levels[0]);
                List<VObject> matches = new List<VObject>();
                foreach (string k in this.Fields.Keys)
                {
                    if (Pattern.Match(k).Value != string.Empty)
                    {
                        if (levels.Length == 1)
                        {
                            Results.Add(this.Fields[k]);
                        }
                    }
                }
                return;
            }
        }
        public List<object> VPath(string path, string Separator)
        {
            List<object> Results = new List<object>();
            this.VPath(path, Results, Separator);
            return Results;
        }
    }

    public class PrimitiveObjectWrap : VObject
    {
        public PrimitiveObjectWrap() : base() { }
        public PrimitiveObjectWrap(PrimitiveObject o) : base(o) { }
        //public PrimitiveObjectWrap(Stream s) : base(s) { }

        public override void Dispose()
        {
            PrimitiveObject ThisContent = this.Content as PrimitiveObject;
            ThisContent.Clear();
            base.Dispose();
        }
        
        public override void Serialize()
        {
            base.Serialize();
            PrimitiveObject ThisContent = this.Content as PrimitiveObject;
            Stream dest = this.Serialized;

            dest.WriteUint((uint)ThisContent.Fields.Count);

            foreach (KeyValuePair<string, ObjectTypePair> kvp in ThisContent.Fields)
            {
                kvp.Value.Serialize();
                uint l = (uint)kvp.Value.Serialized.Length;
                l += kvp.Key.UnicodeLength() + 4;
                dest.WriteUint(l);
            }

            foreach (KeyValuePair<string, ObjectTypePair> kvp in ThisContent.Fields) // 12 + CC*4 + NL + DL
            {
                dest.WriteUint(kvp.Key.UnicodeLength()); // 0 | 4
                dest.WriteUnicode(kvp.Key); // 4 | KL
                kvp.Value.Serialized.CopyTo(dest); // 4 + KL | CL // using the stream created above
            }
        }

        public override void DeSerialize(Stream Input)
        {
            base.DeSerialize(Input);
            PrimitiveObject ThisContent = new PrimitiveObject();
            Stream dest = this.Serialized;

            uint Count = dest.ReadUint();
            List<int> lengths = new List<int>();
            
            for(int i=0; i<Count; i++)
            {
                lengths.Add((int)dest.ReadUint());
            }

            for (int i = 0; i < Count; i++)
            {
                int nameL = (int)dest.ReadUint();
                string name = dest.ReadUnicode(nameL);
                int l = lengths[i]-4-nameL;

                Stream CurrObj = new MemoryStream();
                CurrObj.SetLength(0);
                dest.CopyTo(CurrObj, l);

                ObjectTypePair otp = new ObjectTypePair(CurrObj);
                ThisContent.Add(name, otp);
            }
        }
        
        public override VObject DeepCopy()
        {
            PrimitiveObject ThisContent = (PrimitiveObject)this.Content;
            PrimitiveObject NewObject = new PrimitiveObject();
            foreach (KeyValuePair<string, ObjectTypePair> kvp in ThisContent.Fields)
            {
                ObjectTypePair NewCollection = kvp.Value.DeepCopy();
                NewObject.Add(kvp.Key, NewCollection);
            }
            PrimitiveObjectWrap NewWrap = new PrimitiveObjectWrap(NewObject);
            return (VObject)NewWrap;
        }
    }
}
