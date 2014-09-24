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
    public abstract class ConstructVObjectNode<ParentObject> : IPluginEvaluate where ParentObject : VObject
    {
        [Input("Parent")]
        public Pin<ParentObject> FParent;
        [Input("Construct", IsBang = true)]
        public ISpread<ISpread<bool>> FConstruct;

        public int CurrParent;
        public int CurrChild;

        public virtual void ConstructVObject(ParentObject Parent) { }

        public void Evaluate(int SpreadMax)
        {
            if (FParent.IsConnected)
            {
                for (int i = 0; i < FParent.SliceCount; i++)
                {
                    this.CurrParent = i;
                    for (int j = 0; j < FParent.SliceCount; j++)
                    {
                        this.CurrParent = i;
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
        public ISpread<bool> FAdd;

        public int CurrParent;
        public int CurrSource;

        public virtual void AddVObject(ParentObject Parent, VObject Source) { }

        public void Evaluate(int SpreadMax)
        {
            if (FParent.IsConnected)
            {
                for(int i=0; i<FParent.SliceCount; i++)
                {
                    this.CurrParent = i;
                    if((FSource[i][0] != null) && FAdd[i])
                    {
                        for (int j = 0; j < FSource[i].SliceCount; j++ )
                        {
                            this.CurrSource = j;
                            AddVObject(FParent[i], FSource[i][j]);
                        }
                    }
                }
            }
        }
    }
    public abstract class RemoveVObjectNode<SourceObject> : IPluginEvaluate where SourceObject : VObject
    {
        [Input("Source")]
        public Pin<SourceObject> FInput;
        [Input("Remove", IsBang = true)]
        public ISpread<bool> FRemove;

        public int CurrSource;

        public virtual void RemoveVObject(SourceObject Source) { }

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    CurrSource = i;
                    RemoveVObject(FInput[i]);
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

        public virtual ISpread<VObject> ToSpread(SourceObject Source) { }

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = FInput.SliceCount;
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

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = FInput.SliceCount;
                FFormerIndex.SliceCount = FInput.SliceCount;
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