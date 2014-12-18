using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{

    [PluginInfo(Name = "Cast", Category = "From", Version = "VObjectDictionary")]
    public class FromVObjectDictionaryCastNode : CastFromNode<VObjectDictionaryWrap> { }

    [PluginInfo(Name = "Cast", Category = "To", Version = "VObjectDictionary")]
    public class ToVObjectDictionaryCastNode : CastToNode<VObjectDictionaryWrap> { }


    [PluginInfo(Name = "Dictionary", Category = "VObjectDictionary")]
    public class VObjectDictionaryNode : ParentNode<VObjectDictionaryWrap>
    {
        VObjectDictionaryWrap EverythingWrap;
        VObjectDictionary Everything;

        [ImportingConstructor]
        VObjectDictionaryNode()
        {
            Everything = new VObjectDictionary();
            EverythingWrap = new VObjectDictionaryWrap(Everything);
        }
        public override void Cast()
        {
            FOutput[0] = EverythingWrap;
        }
        public override void Clear()
        {
            Everything.Clear();
        }
        public override void Remove()
        {
            Everything.RemoveTagged();
        }
    }
    [PluginInfo(Name = "Add", Category = "VObjectDictionary", AutoEvaluate = true)]
    public class VObjectDictionaryAddNode : AddVObjectNode<VObjectDictionaryWrap>
    {
        [Input("Reset Age", Order = 10)]
        public ISpread<bool> FResetAge;

        [Input("Manage Existing Object", DefaultEnumEntry = "Extend", Order = 11)]
        public IDiffSpread<ManageExistingObject> FExistObjMan;
        [Input("Manage Existing Child", DefaultEnumEntry = "Overwrite", Order = 12)]
        public ISpread<ISpread<ManageExistingKey>> FExistKeyMan;
        [Input("Manage Not-Existing Object", DefaultEnumEntry = "Create", Order = 13)]
        public IDiffSpread<ManageNotExisting> FNotExistObjMan;
        [Input("Manage Not-Existing Child", DefaultEnumEntry = "Create", Order = 14)]
        public ISpread<ISpread<ManageNotExisting>> FNotExistKeyMan;

        [Output("Valid")]
        public ISpread<ISpread<bool>> FValid;

        public override void InitializeFrame()
        {
            FValid.SliceCount = FSource.SliceCount;
            for(int i=0; i<FValid.SliceCount; i++)
            {
                FValid[i].SliceCount = FSource[i].SliceCount;
                for (int j = 0; j < FValid[i].SliceCount; j++)
                {
                    FValid[i][j] = FSource[i][j].Content is VObjectCollectionWrap;
                }
            }
        }
        public override void AddVObject(VObjectDictionaryWrap Parent, VObject Source)
        {
            VObjectDictionary Content = Parent.Content as VObjectDictionary;
            if (Source is VObjectCollectionWrap)
            {
                VObjectCollection Child = Source.Content as VObjectCollection;
                if (!Content.Objects.ContainsKey(Child.Name))
                {
                    if (FNotExistObjMan[this.CurrParent] == ManageNotExisting.Create)
                    {
                        Content.Objects.Add(Child.Name, (VObjectCollectionWrap)Source);
                    }
                }
                else
                {
                    if (FExistObjMan[this.CurrParent] == ManageExistingObject.Overwrite)
                    {
                        Content.Objects[Child.Name].Dispose();
                        Content.Objects[Child.Name] = (VObjectCollectionWrap)Source;
                    }
                    if (FExistObjMan[this.CurrParent] == ManageExistingObject.Extend)
                    {
                        VObjectCollection ColSource = Content.Objects[Child.Name].Content as VObjectCollection;
                        VObjectCollection CurrSource = Source.Content as VObjectCollection;

                        foreach(KeyValuePair<string, VObject> kvp in CurrSource.Children)
                        {
                            if(ColSource.Children.ContainsKey(kvp.Key))
                            {
                                if(FExistKeyMan[this.CurrParent][this.CurrSource] == ManageExistingKey.Overwrite)
                                {
                                    ColSource[kvp.Key].Dispose();
                                    ColSource[kvp.Key] = kvp.Value;
                                }
                            }
                            else
                            {
                                if (FNotExistKeyMan[this.CurrParent][this.CurrSource] == ManageNotExisting.Create)
                                {
                                    ColSource.Add(kvp.Key, kvp.Value);
                                }
                            }
                        }
                    }
                }
                if(FResetAge[this.CurrParent])
                {
                    VObjectCollection CurrSource = Source.Content as VObjectCollection;
                    CurrSource.Age.Restart();
                }
            }
        }
    }

    [PluginInfo(Name = "Remove", Category = "VObjectDictionary", AutoEvaluate = true)]
    public class VObjectDictionaryRemoveNode : RemoveVObjectNode<VObjectDictionaryWrap>
    {
        [Input("Name", Order = 10)]
        public ISpread<ISpread<string>> FName;
        [Input("Match", Order = 11)]
        public ISpread<ISpread<bool>> FMatch;

        public override void SetSliceCount(int SpreadMax)
        {
            this.SliceCount.SliceCount = FName.SliceCount;
            for (int i = 0; i < SliceCount.SliceCount; i++)
            {
                SliceCount[i] = FName[i].SliceCount;
            }
        }
        public override void RemoveVObject(VObjectDictionaryWrap Parent)
        {
            VObjectDictionary Content = Parent.Content as VObjectDictionary;
            if(FMatch[this.CurrParent][this.CurrChild])
                Content.RemoveObject(FName[this.CurrParent][this.CurrChild]);
            else
            {
                foreach(string k in Content.Objects.Keys)
                {
                    if (k.Contains(FName[this.CurrParent][this.CurrChild]))
                        Content.RemoveObject(k);
                }
            }
        }
    }

    [PluginInfo(Name = "ToSpread", Category = "VObjectDictionary")]
    public class VObjectDictionaryToSpreadNode : ToSpreadNode<VObjectDictionaryWrap>
    {
        public override Spread<VObject> ToSpread(VObjectDictionaryWrap Source)
        {
            Spread<VObject> Out = new Spread<VObject>();
            VObjectDictionary Content = Source.Content as VObjectDictionary;
            Out.SliceCount = 0;
            foreach (KeyValuePair<string, VObjectCollectionWrap> kvp in Content.Objects)
            {
                Out.Add(kvp.Value);
            }
            return Out;
        }
    }

    [PluginInfo(Name = "Sift", Category = "VObjectDictionary")]
    public class VObjectDictionarySiftNode : SiftNode<VObjectDictionaryWrap>
    {
        public override void Sift(VObjectDictionaryWrap Source, string Filter, bool Contains, bool Exclude, List<int> MatchingIndices, List<VObject> Output)
        {
            VObjectDictionary Content = Source.Content as VObjectDictionary;
            if (Contains)
            {
                foreach (string k in Content.Objects.Keys)
                {
                    if (k.Contains(Filter) ^ Exclude)
                    {
                        MatchingIndices.Add(this.CurrentAbsIndex);
                        Output.Add(Content.Objects[k]);
                    }
                }
            }
            else
            {
                foreach (string k in Content.Objects.Keys)
                {
                    if ((k == Filter) ^ Exclude)
                    {
                        MatchingIndices.Add(this.CurrentAbsIndex);
                        Output.Add(Content.Objects[k]);
                    }
                }
            }
        }
    }

    [PluginInfo(Name = "VPath", Category = "VObjectDictionary")]
    public class VObjectDictionaryVPathNode : VPathNode<VObjectDictionaryWrap>
    {
        public override void Sift(VObjectDictionaryWrap Source, string Filter, List<int> MatchingIndices, List<VObject> Output)
        {
            VObjectDictionary Content = Source.Content as VObjectDictionary;
            List<object> result = Content.VPath(Filter, FSeparator[0]);
            foreach(object o in result)
            {
                if (o is VObject)
                {
                    Output.Add(o as VObject);
                    MatchingIndices.Add(this.CurrentAbsIndex);
                }
            }
        }
    }
}
