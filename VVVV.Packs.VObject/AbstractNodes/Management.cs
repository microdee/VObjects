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
    public enum ManageExistingObject
    {
        Ignore,
        Overwrite,
        Extend
    }
    public enum ManageExistingKey
    {
        Ignore,
        Overwrite
    }
    public enum ManageNotExisting
    {
        Ignore,
        Create
    }
    public abstract class ParentNode<ParentObject> : IPluginEvaluate where ParentObject : VObject
    {
        [Input("Clear", IsBang = true, IsSingle = true, Order = 0)]
        public IDiffSpread<bool> FClear;

        [Output("Output", Order = 0)]
        public ISpread<ParentObject> FOutput;

        // You have to construct your parent object when the node is being constructed
        public virtual void Cast() { }
        public virtual void Clear() { }
        public virtual void Remove() { }

        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = 1;
            if (FClear[0]) this.Clear();
            this.Remove();
            this.Cast();
        }
    }
    public abstract class ConstructVObjectNode<ResultObject> : IPluginEvaluate where ResultObject : VObject
    {
        [Input("Construct", IsBang = true, Order = 0)]
        public ISpread<bool> FConstruct;
        [Input("Auto Clear", DefaultBoolean = true, Order = 1)]
        public ISpread<bool> FAutoClear;
        [Output("Output", Order = 0)]
        public ISpread<ResultObject> FOutput;

        public int CurrObj;
        public int SliceCount = 0;
        public int fc = 0;
        public virtual void SetSliceCount(int SpreadMax)
        {
            this.SliceCount = SpreadMax;
        }
        public virtual void InitializeFrame()
        {
            return;
        }

        public virtual ResultObject ConstructVObject()
        {
            return null;
        }

        public void Evaluate(int SpreadMax)
        {
            this.SetSliceCount(SpreadMax);
            this.InitializeFrame();

            if (FAutoClear[0])
            {
                bool clear = false;
                for (int i = 0; i < this.SliceCount; i++)
                {
                    if (FConstruct[i]) clear = true;
                }
                if (clear) fc = 0;
            }
            if (fc == 0) FOutput.SliceCount = 0;
            fc++;

            for (int i = 0; i < this.SliceCount; i++)
            {
                this.CurrObj = i;
                if (FConstruct[i])
                {
                    ResultObject ro = ConstructVObject();
                    if (ro != null) FOutput.Add(ro);
                }
            }
        }
    }
    public abstract class ConstructToParentVObjectNode<ParentObject> : IPluginEvaluate where ParentObject : VObject
    {
        [Input("Parent", Order = 0)]
        public Pin<ParentObject> FParent;
        [Input("Construct", Order = 1, IsBang = true)]
        public ISpread<ISpread<bool>> FConstruct;

        [Output("Output", Order = 0)]
        public ISpread<VObject> FOutput;

        public int CurrParent;
        public int CurrChild;
        public Spread<int> SliceCount = new Spread<int>();
        public int fc = 0;
        public virtual void SetSliceCount(int SpreadMax)
        {
            this.SliceCount.SliceCount = FConstruct.SliceCount;
            for (int i = 0; i < SliceCount.SliceCount; i++)
            {
                SliceCount[i] = FConstruct[i].SliceCount;
            }
        }

        public virtual void ConstructVObject(ParentObject Parent) { }

        public void Evaluate(int SpreadMax)
        {
            if (FParent.IsConnected)
            {
                this.SetSliceCount(SpreadMax);
                foreach(ISpread<bool> s in FConstruct)
                {
                    foreach (bool c in s)
                    {
                        if (c) fc = 0;
                    }
                }
                if(fc==0)
                {
                    FOutput.SliceCount = 0;
                }
                for (int i = 0; i < FParent.SliceCount; i++)
                {
                    this.CurrParent = i;
                    for (int j = 0; j < this.SliceCount[i]; j++)
                    {
                        this.CurrChild = j;
                        if (FConstruct[i][j])
                        {
                            ConstructVObject(FParent[i]);
                        }
                    }
                }
                fc++;
            }
        }
    }
    public abstract class AddVObjectNode<ParentObject> : IPluginEvaluate where ParentObject : VObject
    {
        [Input("Parent", Order = 0)]
        public Pin<ParentObject> FParent;
        [Input("Source", Order = 1)]
        public ISpread<ISpread<VObject>> FSource;
        [Input("Add", IsBang = true, Order = 2)]
        public ISpread<ISpread<bool>> FAdd;

        [Output("Added", IsBang = true)]
        public ISpread<bool> FAdded;

        public int CurrParent;
        public int CurrSource;

        public virtual void AddVObject(ParentObject Parent, VObject Source) { }
        public virtual void InitializeFrame() { }

        public void Evaluate(int SpreadMax)
        {
            if (FParent.IsConnected)
            {
                FAdded.SliceCount = FParent.SliceCount;
                this.InitializeFrame();
                for(int i=0; i<FParent.SliceCount; i++)
                {
                    this.CurrParent = i;
                    bool added = false;
                    for (int j = 0; j < FSource[i].SliceCount; j++ )
                    {
                        this.CurrSource = j;
                        if ((FSource[i][j] != null) && FAdd[i][j])
                        {
                            AddVObject(FParent[i], FSource[i][j]);
                            added = true;
                        }
                    }
                    FAdded[i] = added;
                }
            }
        }
    }
    public abstract class RemoveVObjectNode<ParentObject> : IPluginEvaluate where ParentObject : VObject
    {
        [Input("Parent", Order = 0)]
        public Pin<ParentObject> FParent;
        [Input("Remove", IsBang = true, Order = 1)]
        public ISpread<ISpread<bool>> FRemove;

        public int CurrParent;
        public int CurrChild;

        public Spread<int> SliceCount = new Spread<int>();
        public virtual void SetSliceCount(int SpreadMax)
        {
            this.SliceCount.SliceCount = FParent.SliceCount;
            for (int i = 0; i < SliceCount.SliceCount; i++)
            {
                SliceCount[i] = FRemove[i].SliceCount;
            }
        }

        public virtual void RemoveVObject(ParentObject Parent) { }

        public void Evaluate(int SpreadMax)
        {
            if (FParent.IsConnected)
            {
                this.SetSliceCount(SpreadMax);
                for (int i = 0; i < this.SliceCount.SliceCount; i++)
                {
                    this.CurrParent = i;
                    {
                        for (int j = 0; j < this.SliceCount[i]; j++)
                        {
                            this.CurrChild = j;
                            if (FRemove[i][j])
                                RemoveVObject(FParent[i]);
                        }
                    }
                }
            }
        }
    }
    public abstract class DestroyVObjectNode<SourceObject> : IPluginEvaluate where SourceObject : VObject
    {
        [Input("Source", Order = 0)]
        public Pin<SourceObject> FInput;
        [Input("Destroy", IsBang = true, Order = 1)]
        public ISpread<bool> FDestroy;

        public int CurrSource;

        public virtual void DestroyVObject(SourceObject Source) { }
        public virtual void InitializeFrame() { }

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                this.InitializeFrame();
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    CurrSource = i;
                    if(FDestroy[i]) DestroyVObject(FInput[i]);
                }
            }
        }
    }

    public abstract class ToSpreadNode<SourceObject> : IPluginEvaluate where SourceObject : VObject
    {
        [Input("Source", Order = 0)]
        public Pin<SourceObject> FInput;

        [Output("Output", Order = 0)]
        public ISpread<ISpread<VObject>> FOutput;

        public virtual Spread<VObject> ToSpread(SourceObject Source)
        {
            return new Spread<VObject>();
        }
        public virtual void InitializeFrame() { }

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = FInput.SliceCount;
                this.InitializeFrame();

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    FOutput[i] = ToSpread(FInput[i]);
                }
            }
            else
            {
                FOutput.SliceCount = 0;
            }
        }
    }

    public abstract class SiftNode<SourceObject> : IPluginEvaluate where SourceObject : VObject
    {
        [Input("Source", Order = 0)]
        public Pin<SourceObject> FInput;
        [Input("Filter", Order = 1)]
        public ISpread<ISpread<string>> FFilter;
        [Input("Contains", Order = 2)]
        public ISpread<bool> FContains;
        [Input("Exclude", Order = 3)]
        public ISpread<bool> FExclude;
        [Input("Enabled", DefaultBoolean = true, Order = 5)]
        public ISpread<bool> FEnabled;

        [Output("Output", Order = 0)]
        public ISpread<ISpread<VObject>> FOutput;
        [Output("Filter Index", Order = 1)]
        public ISpread<ISpread<int>> FFormerIndex;

        public virtual void Sift(SourceObject Source, string Filter, bool Contains, bool Exclude, List<int> MatchingIndices, List<VObject> Output) { }
        private List<VObject> Objects = new List<VObject>();
        private List<int> Indices = new List<int>();
        public virtual void InitializeFrame() { }
        public int CurrentAbsIndex = 0;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = FInput.SliceCount;
                FFormerIndex.SliceCount = FInput.SliceCount;

                this.InitializeFrame();

                CurrentAbsIndex = 0;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FEnabled[i])
                    {
                        FOutput[i].SliceCount = 0;
                        FFormerIndex[i].SliceCount = 0;

                        for(int j=0; j<FFilter[i].SliceCount; j++)
                        {
                            this.Indices.Clear();
                            this.Objects.Clear();
                            this.Sift(FInput[i], FFilter[i][j], FContains[i], FExclude[i], Indices, Objects);
                            foreach (int index in Indices)
                            {
                                FFormerIndex[i].Add(index);
                            }
                            foreach (VObject o in Objects)
                            {
                                FOutput[i].Add(o);
                            }
                            CurrentAbsIndex++;
                        }
                    }

                }
            }
            else
            {
                FOutput.SliceCount = 0;
                FFormerIndex.SliceCount = 0;
            }
        }
    }

    public abstract class VPathNode : IPluginEvaluate
    {
        [Input("Source", Order = 0)]
        public Pin<VObject> FInput;
        [Input("Path", Order = 1)]
        public ISpread<ISpread<string>> FFilter;
        [Input("Separator", Order = 2, IsSingle = true, DefaultString="¦")]
        public ISpread<string> FSeparator;
        [Input("Enabled", DefaultBoolean = true, Order = 5)]
        public ISpread<bool> FEnabled;

        [Output("Output", Order = 0)]
        public ISpread<ISpread<VObject>> FOutput;
        [Output("Path Index", Order = 1)]
        public ISpread<ISpread<int>> FFormerIndex;

        public virtual void Sift(VObject Source, string Filter, List<int> MatchingIndices, List<VObject> Output) { }
        private List<VObject> Objects = new List<VObject>();
        private List<int> Indices = new List<int>();
        public virtual void InitializeFrame() { }
        public int CurrentAbsIndex = 0;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = FInput.SliceCount;
                FFormerIndex.SliceCount = FInput.SliceCount;

                this.InitializeFrame();

                CurrentAbsIndex = 0;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FEnabled[i])
                    {
                        FOutput[i].SliceCount = 0;
                        FFormerIndex[i].SliceCount = 0;

                        for (int j = 0; j < FFilter[i].SliceCount; j++)
                        {
                            this.Indices.Clear();
                            this.Objects.Clear();
                            this.Sift(FInput[i], FFilter[i][j], Indices, Objects);
                            foreach (int index in Indices)
                            {
                                FFormerIndex[i].Add(index);
                            }
                            foreach (VObject o in Objects)
                            {
                                FOutput[i].Add(o);
                            }
                            CurrentAbsIndex++;
                        }
                    }

                }
            }
            else
            {
                FOutput.SliceCount = 0;
                FFormerIndex.SliceCount = 0;
            }
        }
    }
}