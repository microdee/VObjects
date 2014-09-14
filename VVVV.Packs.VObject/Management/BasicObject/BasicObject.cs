using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Packs.VObject
{
    public class BasicObject
    {
        public Dictionary<string, ObjectTypePair> Fields = new Dictionary<string, ObjectTypePair>();

        public BasicObject() { }
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
                if (this.Fields.ContainsKey(name)) return Fields[name];
                else return null;
            }
            set { Fields[name] = value; }
        }


    }
    public class BasicObjectWrap : VObject
    {
        public BasicObjectWrap(BasicObject o) : base(o.GetType(), o) { }
        public BasicObjectWrap(Stream s) : base(typeof(BasicObject), s) { }

        protected override void Dispose(bool disposing)
        {
            if (this.disposed)
                return;
            if (disposing)
            {
                BasicObject ThisContent = this.Content as BasicObject;
                ThisContent.Dispose();
                this.Serialized.Dispose();
            }
            disposed = true;
        }

        public override Stream Serialize()
        {
            BasicObject ThisContent = this.Content as BasicObject;
            Stream dest = this.Serialized;
            dest.SetLength(0);
            dest.Position = 0;

            dest.WriteUint((uint)ThisContent.Fields.Count);

            foreach (KeyValuePair<string, ObjectTypePair> kvp in ThisContent.Fields)
            {
                kvp.Value.Serialize();
                uint l = (uint)kvp.Value.Serialized.Length;
                l += kvp.Key.UnicodeLength();
                l += kvp.Value.GetType().ToString().UnicodeLength() + 8;
                dest.WriteUint(l);
            }

            dest.WriteUnicode(ThisContent.Name);
            dest.WriteUnicode(ThisContent.Debug);

            foreach (KeyValuePair<string, VObject> kvp in ThisContent.Children) // 12 + CC*4 + NL + DL
            {
                dest.WriteUint(kvp.Key.UnicodeLength()); // 0 | 4
                dest.WriteUint(kvp.Value.GetType().ToString().UnicodeLength()); // 0 | 4
                dest.WriteUnicode(kvp.Key); // 4 | KL
                dest.WriteUnicode(kvp.Value.GetType().ToString());

                kvp.Value.Serialized.CopyTo(dest); // 4 + KL | CL // using the stream created above
            }
            return dest;
        }
    }
}
