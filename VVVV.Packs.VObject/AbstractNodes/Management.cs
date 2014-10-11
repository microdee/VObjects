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
        [Input("Clear", IsBang = true, IsSingle = true)]
        public IDiffSpread<bool> FClear;

        [Output("Output")]
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
        [Input("Construct", IsBang = true)]
        public ISpread<bool> FConstruct;
        [Input("Auto Clear", DefaultBoolean = true)]
        public ISpread<bool> FAutoClear;
        [Output("Output")]
        public ISpread<ResultObject> FOutput;

        public int CurrObj;
        public int SliceCount = 0;
        public int fc = 0;
        public virtual void SetSliceCount(int SpreadMax)
        {
            this.SliceCount = SpreadMax;
        }

        public virtual ResultObject ConstructVObject()
        {
            return null;
        }

        public void Evaluate(int SpreadMax)
        {
            this.SetSliceCount(SpreadMax);

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
                if(FConstruct[i]) FOutput.Add(ConstructVObject());
            }
        }
    }
    public abstract class ConstructToParentVObjectNode<ParentObject> : IPluginEvaluate where ParentObject : VObject
    {
        [Input("Parent")]
        public Pin<ParentObject> FParent;
        [Input("Construct", IsBang = true)]
        public ISpread<ISpread<bool>> FConstruct;

        public int CurrParent;
        public int CurrChild;
        public Spread<int> SliceCount = new Spread<int>();
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
                for (int i = 0; i < FParent.SliceCount; i++)
                {
                    this.CurrParent = i;
                    for (int j = 0; j < this.SliceCount[i]; j++)
                    {
                        this.CurrChild = j;
                        if (FConstruct[i][j]) ConstructVObject(FParent[i]);
                    }
                }
            }
        }
    }
    public abstract class AddVObjectNode<ParentObject> : IPluginEvaluate where ParentObject : VObject
    {
        [Input("Parent")]
        public Pin<ParentObject> FParent;
        [Input("Source")]
        public ISpread<ISpread<VObject>> FSource;
        [Input("Add", IsBang = true)]
        public ISpread<ISpread<bool>> FAdd;

        public int CurrParent;
        public int CurrSource;

        public virtual void AddVObject(ParentObject Parent, VObject Source) { }
        public virtual void InitializeFrame() { }

        public void Evaluate(int SpreadMax)
        {
            if (FParent.IsConnected)
            {
                this.InitializeFrame();
                for(int i=0; i<FParent.SliceCount; i++)
                {
                    this.CurrParent = i;
                    {
                        for (int j = 0; j < FSource[i].SliceCount; j++ )
                        {
                            this.CurrSource = j;
                            if ((FSource[i][j] != null) && FAdd[i][j])
                                AddVObject(FParent[i], FSource[i][j]);
                        }
                    }
                }
            }
        }
    }
    public abstract class RemoveVObjectNode<ParentObject> : IPluginEvaluate where ParentObject : VObject
    {
        [Input("Parent")]
        public Pin<ParentObject> FParent;
        [Input("Remove", IsBang = true)]
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
        [Input("Source")]
        public Pin<SourceObject> FInput;
        [Input("Destroy", IsBang = true)]
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
        [Input("Source")]
        public Pin<SourceObject> FInput;

        [Output("Output")]
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
        [Input("Source")]
        public Pin<SourceObject> FInput;
        [Input("Filter")]
        public ISpread<string> FFilter;
        [Input("Contains")]
        public ISpread<bool> FContains;
        [Input("Exclude")]
        public ISpread<bool> FExclude;
        [Input("Enabled", IsBang = true)]
        public ISpread<bool> FEnabled;

        [Output("Output")]
        public ISpread<ISpread<VObject>> FOutput;
        [Output("Former Index")]
        public ISpread<ISpread<int>> FFormerIndex;

        public virtual void Sift(SourceObject Source, string Filter, bool Contains, bool Exclude, List<int> MatchingIndices, List<VObject> Output) { }
        private List<VObject> Objects = new List<VObject>();
        private List<int> Indices = new List<int>();
        public virtual void InitializeFrame() { }

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = FInput.SliceCount;
                FFormerIndex.SliceCount = FInput.SliceCount;

                this.InitializeFrame();
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    FOutput[i].SliceCount = 0;
                    FFormerIndex[i].SliceCount = 0;

                    this.Indices.Clear();
                    this.Objects.Clear();
                    this.Sift(FInput[i], FFilter[i], FContains[i], FExclude[i], Indices, Objects);
                    foreach (int index in Indices)
                    {
                        FFormerIndex[i].Add(index);
                    }
                    foreach (VObject o in Objects)
                    {
                        FOutput[i].Add(o);
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