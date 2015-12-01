using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;

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
    public abstract class ParentNode<ParentObject> : IPluginEvaluate
    {
        [Input("Clear", IsBang = true, IsSingle = true, Order = 0)]
        public IDiffSpread<bool> FClear;

        [Output("Output Parent", Order = 0)]
        public ISpread<object> FOutput;

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
    public abstract class ConstructObjectNode : IPluginEvaluate
    {
        [Input("Construct", IsBang = true, Order = 0)]
        public ISpread<bool> FConstruct;
        [Input("Auto Clear", DefaultBoolean = true, Order = 1)]
        public ISpread<bool> FAutoClear;
        [Output("Output Object", Order = 0)]
        public ISpread<object> FOutput;

        private List<int> Removables = new List<int>();
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

        public virtual object ConstructObject()
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
                    object ro = ConstructObject();
                    if (ro != null) FOutput.Add(ro);
                }
            }
        }
    }
    public abstract class ConstructAndSetObjectNode : IPluginEvaluate
    {
        [Input("Construct", IsBang = true, Order = 0)]
        public ISpread<bool> FConstruct;
        [Input("Auto Clear", DefaultBoolean = true, Order = 1)]
        public ISpread<bool> FAutoClear;
        [Input("Set", IsBang = true, Order = 0)]
        public ISpread<bool> FSet;
        [Input("Dispose Disposable", Order = 100)]
        public ISpread<bool> FDisposeDisposable;
        [Output("Output Object", Order = 0)]
        public ISpread<object> FOutput;

        private List<int> Removables = new List<int>();
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

        public virtual object ConstructObject()
        {
            return null;
        }
        public virtual void SetVObject(object Obj)
        {
            return;
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
            if (fc == 0)
            {
                for (int i = 0; i < FOutput.SliceCount; i++)
                {
                    if (FOutput[i] != null && FDisposeDisposable[0])
                        ObjectHelper.DisposeDisposable(FOutput[i]);
                }
                FOutput.SliceCount = 0;
            }
            fc++;

            bool empty = FOutput.SliceCount == 0;
            for (int i = 0; i < this.SliceCount; i++)
            {
                this.CurrObj = i;
                if (FConstruct[i] || (FSet[i] && empty))
                {
                    object ro = ConstructObject();
                    if (ro != null) FOutput.Add(ro);
                }
            }
        }
    }
    public abstract class ConstructToParentObjectNode : IPluginEvaluate
    {
        [Input("Parent", Order = 0)]
        public Pin<object> FParent;
        [Input("Construct", Order = 1, BinOrder = 2, IsBang = true)]
        public ISpread<ISpread<bool>> FConstruct;

        [Output("Output Object", Order = 0)]
        public ISpread<object> FOutput;

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

        public virtual void ConstructObject(object Parent) { }

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
                            ConstructObject(FParent[i]);
                        }
                    }
                }
                fc++;
            }
        }
    }
    public abstract class AddObjectNode : IPluginEvaluate
    {
        [Input("Parent", Order = 0)]
        public Pin<object> FParent;
        [Input("Source", Order = 1, BinOrder = 2)]
        public ISpread<ISpread<object>> FSource;
        [Input("Add", IsBang = true, Order = 3, BinOrder = 4)]
        public ISpread<ISpread<bool>> FAdd;

        [Output("Added", IsBang = true)]
        public ISpread<bool> FAdded;

        public int CurrParent;
        public int CurrSource;

        public virtual void AddObject(object Parent, object Source) { }
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
                            AddObject(FParent[i], FSource[i][j]);
                            added = true;
                        }
                    }
                    FAdded[i] = added;
                }
            }
        }
    }
    public abstract class RemoveObjectNode : IPluginEvaluate
    {
        [Input("Parent", Order = 0)]
        public Pin<object> FParent;
        [Input("Remove", IsBang = true, Order = 1, BinOrder = 2)]
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

        public virtual void RemoveObject(object Parent) { }

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
                                RemoveObject(FParent[i]);
                        }
                    }
                }
            }
        }
    }

    public abstract class ToSpreadNode : IPluginEvaluate
    {
        [Input("Source", Order = 0)]
        public Pin<object> FInput;

        [Output("Output", Order = 0, BinOrder = 1)]
        public ISpread<ISpread<object>> FOutput;

        public virtual Spread<object> ToSpread(object Source)
        {
            return new Spread<object>();
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

    public abstract class GetObjectNode : IPluginEvaluate
    {
        [Input("Source", Order = 0)]
        public Pin<object> FInput;
        [Input("Filter", Order = 1, BinOrder = 2)]
        public ISpread<ISpread<string>> FFilter;
        [Input("Contains", Order = 3)]
        public ISpread<bool> FContains;
        [Input("Exclude", Order = 4)]
        public ISpread<bool> FExclude;
        [Input("Enabled", DefaultBoolean = true, Order = 5)]
        public ISpread<bool> FEnabled;

        [Output("Output", Order = 0, BinOrder = 1)]
        public ISpread<ISpread<object>> FOutput;
        [Output("Filter Index", Order = 2, BinOrder = 3)]
        public ISpread<ISpread<int>> FFormerIndex;

        public virtual void Sift(object Source, string Filter, bool Contains, bool Exclude, List<int> MatchingIndices, List<object> Output) { }
        private List<object> Objects = new List<object>();
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
                            foreach (object o in Objects)
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
        public Pin<object> FInput;
        [Input("Path", Order = 1, BinOrder = 2)]
        public ISpread<ISpread<string>> FFilter;
        [Input("Separator", Order = 3, IsSingle = true, DefaultString="¦")]
        public ISpread<string> FSeparator;
        [Input("Enabled", DefaultBoolean = true, Order = 4)]
        public ISpread<bool> FEnabled;

        [Output("Output", Order = 0, BinOrder = 1)]
        public ISpread<ISpread<object>> FOutput;
        [Output("Path Index", Order = 2, BinOrder = 3)]
        public ISpread<ISpread<int>> FFormerIndex;

        public virtual void Sift(object Source, string Filter, List<int> MatchingIndices, List<object> Output) { }
        private List<object> Objects = new List<object>();
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
                            foreach (object o in Objects)
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