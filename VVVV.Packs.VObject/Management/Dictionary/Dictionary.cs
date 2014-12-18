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
    public class VObjectDictionary
    {
        public Dictionary<string, VObjectCollectionWrap> Objects = new Dictionary<string, VObjectCollectionWrap>();
        public List<string> RemoveList = new List<string>();
        public VObjectDictionary() { }

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

        public void VPath(string path, List<object> Results, string Separator)
        {
            string[] levels = path.Split(Separator.ToCharArray());
            string nextpath = string.Join(Separator, levels, 1, levels.Length - 1);
            if ((levels[0][0] == '"') && (levels[0][levels[0].Length - 1] == '"'))
            {
                string key = levels[0].Trim('"');
                if (this.Objects.ContainsKey(key))
                {
                    if (levels.Length == 1)
                    {
                        Results.Add(this.Objects[key] as VObject);
                        return;
                    }
                    VPathNextStep(key, nextpath, Results, Separator);
                }
            }
            else
            {
                Regex Pattern = new Regex(levels[0]);
                List<VObject> matches = new List<VObject>();
                foreach (string k in this.Objects.Keys)
                {
                    if (Pattern.Match(k).Value != string.Empty)
                    {
                        if (levels.Length == 1)
                        {
                            Results.Add(this.Objects[k] as VObject);
                        }
                        else VPathNextStep(k, nextpath, Results, Separator);
                    }
                }
                return;
            }
        }
        private void VPathNextStep(string CurrentLevel, string NextPath, List<object> Results, string Separator)
        {

            if (this.Objects[CurrentLevel] is VObjectCollectionWrap)
            {
                VObjectCollection vc = this.Objects[CurrentLevel].Content as VObjectCollection;
                vc.VPath(NextPath, Results, Separator);
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
    public class VObjectDictionaryWrap : VObject
    {
        public VObjectDictionaryWrap() : base() { }
        public VObjectDictionaryWrap(VObjectDictionary o) : base(o) { }
        public VObjectDictionaryWrap(Stream s) : base(s) { }

        public override void Dispose()
        {
            VObjectDictionary ThisContent = this.Content as VObjectDictionary;
            ThisContent.Clear();
            base.Dispose();
        }
        public override void Serialize()
        {
            base.Serialize();
            VObjectDictionary ThisContent = this.Content as VObjectDictionary;
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
            VObjectDictionary ThisContent = new VObjectDictionary();

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
            VObjectDictionary ThisContent = (VObjectDictionary)this.Content;
            VObjectDictionary NewObject = new VObjectDictionary();
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
