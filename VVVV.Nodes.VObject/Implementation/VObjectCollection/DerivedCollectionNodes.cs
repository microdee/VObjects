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

    [PluginInfo(Name = "Cast", Category = "From", Version = "VObjectCollection")]
    public class FromVObjectCollectionCastNode : CastFromNode<VObjectCollectionWrap> { }

    [PluginInfo(Name = "Cast", Category = "To", Version = "VObjectCollection")]
    public class ToVObjectCollectionCastNode : CastToNode<VObjectCollectionWrap> { }

    [PluginInfo(Name = "Construct", Category = "VObjectCollection")]
    public class VObjectCollectionConstructNode : ConstructVObjectNode<VObjectCollectionWrap>
    {
        [Input("Name", Order = 10)]
        public ISpread<string> FName;

        public override VObjectCollectionWrap ConstructVObject()
        {
            VObjectCollection NewObj = new VObjectCollection();
            NewObj.Name = FName[this.CurrObj];
            VObjectCollectionWrap NewWrap = new VObjectCollectionWrap(NewObj);
            return NewWrap;
        }
    }

    [PluginInfo(Name = "ConstructToDictionary", Category = "VObjectCollection", AutoEvaluate = true)]
    public class VObjectCollectionConstructToDictionaryNode : ConstructToParentVObjectNode<VObjectDictionaryWrap>
    {
        [Input("Name", Order = 10)]
        public ISpread<ISpread<string>> FName;

        [Input("Manage Existing Object", DefaultEnumEntry = "Overwrite", Order = 11)]
        public IDiffSpread<ManageExistingObject> FExistObjMan;
        [Input("Manage Not-Existing Object", DefaultEnumEntry = "Create", Order = 12)]
        public IDiffSpread<ManageNotExisting> FNotExistObjMan;

        public override void SetSliceCount(int SpreadMax)
        {
            this.SliceCount.SliceCount = FName.SliceCount;
            for (int i = 0; i < SliceCount.SliceCount; i++)
            {
                SliceCount[i] = FName[i].SliceCount;
            }
        }
        public override void ConstructVObject(VObjectDictionaryWrap Parent)
        {
            VObjectDictionary Content = Parent.Content as VObjectDictionary;
            if (!Content.Objects.ContainsKey(FName[this.CurrParent][this.CurrChild]))
            {
                if (FNotExistObjMan[this.CurrParent] == ManageNotExisting.Create)
                {
                    VObjectCollection NewObj = new VObjectCollection();
                    NewObj.Name = FName[this.CurrParent][this.CurrChild];
                    VObjectCollectionWrap NewWrap = new VObjectCollectionWrap(NewObj);
                    Content.Objects.Add(FName[this.CurrParent][this.CurrChild], NewWrap);

                    FOutput.Add(NewWrap);
                }
            }
            else
            {
                if (FExistObjMan[this.CurrParent] == ManageExistingObject.Overwrite)
                {
                    VObjectCollection CurrObj = Content.Objects[FName[this.CurrParent][this.CurrChild]].Content as VObjectCollection;
                    CurrObj.Clear();

                    VObjectCollection NewObj = new VObjectCollection();
                    NewObj.Name = FName[this.CurrParent][this.CurrChild];
                    VObjectCollectionWrap NewWrap = new VObjectCollectionWrap(NewObj);

                    Content.Objects[FName[this.CurrParent][this.CurrChild]] = NewWrap;
                    FOutput.Add(NewWrap);
                }
            }
        }
    }

    [PluginInfo(Name = "Add", Category = "VObjectCollection", AutoEvaluate = true)]
    public class VObjectCollectionAddNode : AddVObjectNode<VObjectCollectionWrap>
    {
        [Input("Name", Order = 10)]
        public ISpread<ISpread<string>> FName;

        [Input("Manage Existing Object", DefaultEnumEntry = "Overwrite", Order = 11)]
        public IDiffSpread<ManageExistingObject> FExistObjMan;
        [Input("Manage Not-Existing Object", DefaultEnumEntry = "Create", Order = 12)]
        public IDiffSpread<ManageNotExisting> FNotExistObjMan;

        public override void AddVObject(VObjectCollectionWrap Parent, VObject Source)
        {
            VObjectCollection Content = Parent.Content as VObjectCollection;
            if (!Content.Children.ContainsKey(FName[this.CurrParent][this.CurrSource]))
            {
                if (FNotExistObjMan[this.CurrParent] == ManageNotExisting.Create)
                {
                    Content.Add(FName[this.CurrParent][this.CurrSource], Source);
                }
            }
            else
            {
                if (FExistObjMan[this.CurrParent] == ManageExistingObject.Overwrite)
                {
                    Content[FName[this.CurrParent][this.CurrSource]].Dispose();
                    Content[FName[this.CurrParent][this.CurrSource]] = Source;
                }
            }
        }
    }

    [PluginInfo(Name = "Remove", Category = "VObjectCollection", AutoEvaluate = true)]
    public class VObjectCollectionRemoveNode : RemoveVObjectNode<VObjectCollectionWrap>
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
        public override void RemoveVObject(VObjectCollectionWrap Parent)
        {
            VObjectCollection Content = Parent.Content as VObjectCollection;
            Content.Remove(FName[this.CurrParent][this.CurrChild], FMatch[this.CurrParent][this.CurrChild]);
        }
    }

    [PluginInfo(Name = "Destroy", Category = "VObjectCollection")]
    public class VObjectCollectionDestroyNode : DestroyVObjectNode<VObjectCollectionWrap>
    {
        public override void DestroyVObject(VObjectCollectionWrap Source)
        {
            VObjectCollection Content = Source.Content as VObjectCollection;
            Content.Removing = true;
        }
    }

    [PluginInfo(Name = "ToSpread", Category = "VObjectCollection")]
    public class VObjectCollectionToSpreadNode : ToSpreadNode<VObjectCollectionWrap>
    {
        public override Spread<VObject> ToSpread(VObjectCollectionWrap Source)
        {
            Spread<VObject> Out = new Spread<VObject>();
            VObjectCollection Content = Source.Content as VObjectCollection;
            Out.SliceCount = 0;
            foreach(KeyValuePair<string, VObject> kvp in Content.Children)
            {
                Out.Add(kvp.Value);
            }
            return Out;
        }
    }

    [PluginInfo(Name = "Sift", Category = "VObjectCollection")]
    public class VObjectCollectionSiftNode : SiftNode<VObjectCollectionWrap>
    {
        public override void Sift(VObjectCollectionWrap Source, string Filter, bool Contains, bool Exclude, List<int> MatchingIndices, List<VObject> Output)
        {
            VObjectCollection Content = Source.Content as VObjectCollection;
            if(Contains)
            {
                foreach(string k in Content.Children.Keys)
                {
                    if(k.Contains(Filter) ^ Exclude)
                    {
                        MatchingIndices.Add(this.CurrentAbsIndex);
                        Output.Add(Content[k]);
                    }
                }
            }
            else
            {
                foreach (string k in Content.Children.Keys)
                {
                    if ((k == Filter) ^ Exclude)
                    {
                        MatchingIndices.Add(this.CurrentAbsIndex);
                        Output.Add(Content[k]);
                    }
                }
            }
        }
    }
}
