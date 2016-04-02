using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.PDDN;
using NGISpread = VVVV.PluginInterfaces.V2.NonGeneric.ISpread;
using NGIDiffSpread = VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(Name = "GetType", Category = "Object", Tags = "microdee")]
    #endregion PluginInfo
    public class ObjectGetTypeNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Import] public IPluginHost2 FPluginHost;

        protected GenericInput FInput;

        [Output("Object Type")]
        public ISpread<string> FType;

        public void OnImportsSatisfied()
        {
            FInput = new GenericInput(FPluginHost, new InputAttribute("Input"));
        }

        public void Evaluate(int SpreadMax)
        {
            if(FInput.Connected)
            {
                FType.SliceCount = FInput.Pin.SliceCount;
                for(int i=0; i<FInput.Pin.SliceCount; i++)
                {
                    FType[i] = FInput[i].GetType().AssemblyQualifiedName;
                }
            }
            else
            {
                FType.SliceCount = 0;
            }
        }

    }

    #region PluginInfo
    [PluginInfo(
        Name = "FilterType",
        Category = "Object",
        Help = "Filter objects by type",
        Author = "microdee",
        AutoEvaluate = true)]
    #endregion PluginInfo
    public class ObjectFilterTypeNode : ConfigurableDynamicPinNode<string>, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Import]
        public IPluginHost2 FPluginHost;
        [Import]
        public IIOFactory FIOFactory;

        protected GenericInput FInput;
        protected GenericInput FTypeRef;

        [Config("Type", DefaultString = "")]
        public IDiffSpread<string> FType;

        public PinDictionary pd;

        protected override void PreInitialize()
        {
            pd = new PinDictionary(FIOFactory);
            FInput = new GenericInput(FPluginHost, new InputAttribute("Input"));
            FInput.Pin.Order = 0;
            FTypeRef = new GenericInput(FPluginHost, new InputAttribute("Type Reference Object"));
            FTypeRef.Pin.Order = 1;
            ConfigPinCopy = FType;
        }
        private Type CType;
        protected override bool IsConfigDefault()
        {
            return FType[0] == "";
        }

        protected override void Initialize()
        {
            if (FType[0] != "")
            {
                CType = Type.GetType(FType[0], true);

                if (FTypeRef.Pin.SliceCount != 0)
                {
                    if (FTypeRef[0] != null)
                    {
                        Type T = FTypeRef[0].GetType();
                        if (T != CType)
                        {
                            pd.RemoveAllOutput();
                            pd.AddOutputBinSized(T, new OutputAttribute("Output"));
                            CType = T;
                            FType[0] = T.AssemblyQualifiedName;
                        }
                    }
                }

                //RemoveAllOutput();
                pd.AddOutputBinSized(CType, new OutputAttribute("Output"));
            }
        }

        public void Evaluate(int SpreadMax)
        {
            if (FInput.Connected && FTypeRef.Connected)
            {
                if (FTypeRef.Pin.SliceCount != 0)
                {
                    Type T = FTypeRef[0].GetType();
                    bool valid = false;
                    if (CType == null)
                        valid = true;
                    else
                        valid = T != CType;
                    if (valid)
                    {
                        pd.RemoveAllOutput();
                        pd.AddOutputBinSized(T, new OutputAttribute("Output"));
                        CType = T;
                        FType[0] = T.AssemblyQualifiedName;
                    }
                }
                if (pd.OutputPins.ContainsKey("Output"))
                {
                    pd.OutputPins["Output"].Spread.SliceCount = FInput.Pin.SliceCount;
                    for (int i = 0; i < FInput.Pin.SliceCount; i++)
                    {
                        var cspread = (NGISpread) pd.OutputPins["Output"].Spread[i];
                        if (CType == FInput[i].GetType())
                        {
                            cspread.SliceCount = 1;
                            cspread[0] = FInput[i];
                        }
                        else
                        {
                            cspread.SliceCount = 0;
                        }
                    }
                }
            }
            else
            {
                if (pd.OutputPins.ContainsKey("Output"))
                {
                    pd.OutputPins["Output"].Spread.SliceCount = 0;
                }
            }
        }
    }
    
    [PluginInfo(Name = "VPath", Category = "VObject")]
    public class VObjectGenericVPathNode : VPathNode
    {
        public override void Sift(object Source, string Filter, List<int> MatchingIndices, List<object> Output)
        {
            if (Source is IVPathQueryable)
            {
                var Content = Source as IVPathQueryable;
                var result = Content.VPath(Filter, FSeparator[0]);
                foreach (object o in result)
                {
                    Output.Add(o);
                    MatchingIndices.Add(this.CurrentAbsIndex);
                }
            }
        }
    }
}