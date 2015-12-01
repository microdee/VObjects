using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(Name = "Construct", Category = "VObjectCollection")]
    public class VObjectCollectionConstructNode : ConstructObjectNode
    {
        [Input("Name", Order = 10)]
        public ISpread<string> FName;

        public override object ConstructObject()
        {
            VObjectCollection NewObj = new VObjectCollection();
            NewObj.Name = FName[this.CurrObj];
            return NewObj;
        }
    }

    [PluginInfo(Name = "ConstructToCollection", Category = "VObjectCollection", AutoEvaluate = true)]
    public class VObjectCollectionConstructToCollectionNode : ConstructToParentObjectNode
    {
        [Input("Name", Order = 10, BinOrder = 11)]
        public ISpread<ISpread<string>> FName;

        [Input("Manage Existing Object", DefaultEnumEntry = "Overwrite", Order = 12)]
        public IDiffSpread<ManageExistingObject> FExistObjMan;
        [Input("Manage Not-Existing Object", DefaultEnumEntry = "Create", Order = 13)]
        public IDiffSpread<ManageNotExisting> FNotExistObjMan;

        public override void SetSliceCount(int SpreadMax)
        {
            this.SliceCount.SliceCount = FName.SliceCount;
            for (int i = 0; i < SliceCount.SliceCount; i++)
            {
                SliceCount[i] = FName[i].SliceCount;
            }
        }
        public override void ConstructObject(object Parent)
        {
            if (Parent is VObjectCollection)
            {
                VObjectCollection Content = Parent as VObjectCollection;
                if (!Content.Children.ContainsKey(FName[this.CurrParent][this.CurrChild]))
                {
                    if (FNotExistObjMan[this.CurrParent] == ManageNotExisting.Create)
                    {
                        VObjectCollection NewObj = new VObjectCollection();
                        NewObj.Name = FName[this.CurrParent][this.CurrChild];
                        Content.Children.Add(FName[this.CurrParent][this.CurrChild], NewObj);

                        FOutput.Add(NewObj);
                    }
                }
                else
                {
                    if (FExistObjMan[this.CurrParent] == ManageExistingObject.Overwrite)
                    {
                        VObjectCollection CurrObj = Content.Children[FName[this.CurrParent][this.CurrChild]] as VObjectCollection;
                        CurrObj.Clear();

                        VObjectCollection NewObj = new VObjectCollection();
                        NewObj.Name = FName[this.CurrParent][this.CurrChild];

                        Content.Children[FName[this.CurrParent][this.CurrChild]] = NewObj;
                        FOutput.Add(NewObj);
                    }
                }
            }
        }
    }

    [PluginInfo(Name = "Add", Category = "VObjectCollection", AutoEvaluate = true)]
    public class VObjectCollectionAddNode : AddObjectNode
    {
        [Input("Name", Order = 10, BinOrder = 11)]
        public ISpread<ISpread<string>> FName;

        [Input("Manage Existing Object", DefaultEnumEntry = "Overwrite", Order = 12)]
        public IDiffSpread<ManageExistingObject> FExistObjMan;
        [Input("Manage Not-Existing Object", DefaultEnumEntry = "Create", Order = 13)]
        public IDiffSpread<ManageNotExisting> FNotExistObjMan;

        public override void AddObject(object Parent, object Source)
        {
            if (Parent is VObjectCollection)
            {
                VObjectCollection Content = Parent as VObjectCollection;
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
                        Content[FName[this.CurrParent][this.CurrSource]] = Source;
                    }
                }
            }
        }
    }

    [PluginInfo(Name = "Remove", Category = "VObjectCollection", AutoEvaluate = true)]
    public class VObjectCollectionRemoveNode : RemoveObjectNode
    {
        [Input("Name", Order = 10, BinOrder = 11)]
        public ISpread<ISpread<string>> FName;
        [Input("Match", Order = 12, BinOrder = 13)]
        public ISpread<ISpread<bool>> FMatch;

        public override void SetSliceCount(int SpreadMax)
        {
            this.SliceCount.SliceCount = FName.SliceCount;
            for (int i = 0; i < SliceCount.SliceCount; i++)
            {
                SliceCount[i] = FName[i].SliceCount;
            }
        }
        public override void RemoveObject(object Parent)
        {
            if (Parent is VObjectCollection)
            {
                VObjectCollection Content = Parent as VObjectCollection;
                Content.Remove(FName[this.CurrParent][this.CurrChild], FMatch[this.CurrParent][this.CurrChild]);
            }
        }
    }

    [PluginInfo(Name = "ToSpread", Category = "VObjectCollection")]
    public class VObjectCollectionToSpreadNode : ToSpreadNode
    {
        public override Spread<object> ToSpread(object Source)
        {
            if (Source is VObjectCollection)
            {
                Spread<object> Out = new Spread<object>();
                VObjectCollection Content = Source as VObjectCollection;
                Out.SliceCount = 0;
                foreach (KeyValuePair<string, object> kvp in Content.Children)
                {
                    Out.Add(kvp.Value);
                }
                return Out;
            }
            else
            {
                Spread<object> Out = new Spread<object>();
                Out.SliceCount = 0;
                return Out;
            }
        }
    }

    [PluginInfo(Name = "GetObject", Category = "VObjectCollection")]
    public class VObjectCollectionSiftNode : GetObjectNode
    {
        public override void Sift(object Source, string Filter, bool Contains, bool Exclude, List<int> MatchingIndices, List<object> Output)
        {
            if (Source is VObjectCollection)
            {
                VObjectCollection Content = Source as VObjectCollection;
                if (Contains)
                {
                    foreach (string k in Content.Children.Keys)
                    {
                        if (k.Contains(Filter) ^ Exclude)
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
}
