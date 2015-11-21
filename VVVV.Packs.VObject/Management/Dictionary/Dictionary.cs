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
    // class for high level object management in VVVV
    public class VObjectCollection : VPathQueryable
    {
        public Dictionary<string, VObjectCollectionWrap> Objects = new Dictionary<string, VObjectCollectionWrap>();
        public List<string> RemoveList = new List<string>();
        public VObjectCollection() { }

        public void RemoveObject(string k)
        {
            VObjectCollection tbr = this.Objects[k].Content as VObjectCollection;
            tbr.Removing = true;
        }
        public void RemoveTagged()
        {
            this.RemoveList.Clear();
            foreach (KeyValuePair<string, VObjectCollectionWrap> kvp in this.Objects)
            {
                VObjectCollection Content = kvp.Value.Content as VObjectCollection;
                if (Content.Removing) this.RemoveList.Add(kvp.Key);
            }

            foreach (string k in this.RemoveList)
            {
                this.Objects[k].Dispose();
                this.Objects.Remove(k);
            }
            this.RemoveList.Clear();
        }
        public void Clear()
        {
            foreach (KeyValuePair<string, VObjectCollectionWrap> kvp in this.Objects) kvp.Value.Dispose();
            this.Objects.Clear();
        }

        public override object VPathGetItem(string key)
        {
            if (this.Objects.ContainsKey(key))
                return this.Objects[key];
            else return null;
        }

        public override string[] VPathQueryKeys()
        {
            return this.Objects.Keys.ToArray();
        }

    }
    public class VObjectDictionaryWrap : VObject
    {
        public VObjectDictionaryWrap() : base() { }
        public VObjectDictionaryWrap(VObjectCollection o) : base(o) { }
        public VObjectDictionaryWrap(Stream s) : base(s) { }

        public override void Dispose()
        {
            VObjectCollection ThisContent = this.Content as VObjectCollection;
            ThisContent.Clear();
            base.Dispose();
        }
        public override void Serialize()
        {
            base.Serialize();
            VObjectCollection ThisContent = this.Content as VObjectCollection;
            Stream dest = this.Serialized;

            dest.WriteUint((uint)ThisContent.Objects.Count); // 0 | 4

            foreach (KeyValuePair<string, VObjectCollectionWrap> kvp in ThisContent.Objects) // 4 | CC*4
            {
                kvp.Value.Serialize();
                uint l = (uint)kvp.Value.Serialized.Length; // serialized here
                l += kvp.Key.UnicodeLength() + 4;
                dest.WriteUint(l);
            }

            foreach (KeyValuePair<string, VObjectCollectionWrap> kvp in ThisContent.Objects) // 4 + CC*4
            {
                dest.WriteUint(kvp.Key.UnicodeLength()); // 0 | 4
                dest.WriteUnicode(kvp.Key); // 4 | KL

                kvp.Value.Serialized.CopyTo(dest); // 4 + KL | CL // using the stream created above
            }
        }
        public override void DeSerialize(Stream Input)
        {
            base.DeSerialize(Input);
            VObjectCollection ThisContent = new VObjectCollection();

            uint Count = this.Serialized.ReadUint();

            List<uint> ChildrenLengths = new List<uint>();
            for (int i = 0; i < Count; i++)
            {
                ChildrenLengths.Add(this.Serialized.ReadUint());
            }

            for (int i = 0; i < Count; i++)
            {
                uint keylength = this.Serialized.ReadUint();
                string keyname = this.Serialized.ReadUnicode((int)keylength);

                uint l = ChildrenLengths[i] - keylength - 4;
                Stream vobject = new MemoryStream();
                this.Serialized.CopyTo(vobject, (int)l);
                ThisContent.Objects.Add(keyname, new VObjectCollectionWrap(vobject));
            }
            this.Content = ThisContent;
        }
        public override VObject DeepCopy()
        {
            VObjectCollection ThisContent = (VObjectCollection)this.Content;
            VObjectCollection NewObject = new VObjectCollection();
            foreach(KeyValuePair<string, VObjectCollectionWrap> kvp in ThisContent.Objects)
            {
                VObjectCollectionWrap NewCollection = (VObjectCollectionWrap)kvp.Value.DeepCopy();
                NewObject.Objects.Add(kvp.Key, NewCollection);
            }
            VObjectDictionaryWrap NewWrap = new VObjectDictionaryWrap(NewObject);
            return (VObject)NewWrap;
        }
    }
}
