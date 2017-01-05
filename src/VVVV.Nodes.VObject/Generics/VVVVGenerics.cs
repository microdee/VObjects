using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Nodes.PDDN;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(Name = "Cons",
        Category = "Object",
        Help = "Concatenates all input spreads to one output spread.",
        Tags = "generic, spreadop"
        )]
    public class VObjectConsNode : ConfigurableDynamicPinNode<int>, IPluginEvaluate
    {
        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;

        [Config("Count", DefaultValue = 0)] public IDiffSpread<int> FCount;
        [Output("Output")] public ISpread<ISpread<object>> FOut;
        
        List<GenericInput> Inputs = new List<GenericInput>();

        protected override void PreInitialize()
        {
            ConfigPinCopy = FCount;
        }

        protected override bool IsConfigDefault()
        {
            return FCount[0] == 0;
        }

        protected override void Initialize()
        {
            CreatePins(FCount[0]);
        }

        protected override void OnConfigPinChanged()
        {
            if(Inputs.Count == FCount[0]) return;
            CreatePins(FCount[0]);
        }


        protected void CreatePins(int count)
        {
            if (Inputs.Count < count)
            {
                for (int i = Inputs.Count; i < count; i++)
                {
                    int ii = i + 1;
                    var attr = new InputAttribute("Input" + ii);
                    attr.Order = i;
                    var pin = new GenericInput(FPluginHost, attr);
                    Inputs.Add(pin);
                }
            }
            if (Inputs.Count > count)
            {
                for (int i = Inputs.Count-1; i >= count; i--)
                {
                    FPluginHost.DeletePin(Inputs[i].Pin);
                    Inputs.RemoveAt(i);
                }
            }
        }

        public void Evaluate(int SpreadMax)
        {
            FOut.SliceCount = Inputs.Count;
            for (int i = 0; i < Inputs.Count; i++)
            {
                FOut[i].SliceCount = Inputs[i].Pin.SliceCount;
                for (int j = 0; j < Inputs[i].Pin.SliceCount; j++)
                {
                    FOut[i][j] = Inputs[i][j];
                }
            }
        }
    }

    [PluginInfo(Name = "Zip",
                Category = "Object",
                Help = "Zips spreads together.",
                Tags = "join, generic, spreadop"
               )]
    public class VObjectZipNode : ConfigurableDynamicPinNode<int>, IPluginEvaluate
    {
        [Import]
        protected IPluginHost2 FPluginHost;
        [Import]
        protected IIOFactory FIOFactory;

        [Config("Count", DefaultValue = 0)]
        public IDiffSpread<int> FCount;
        [Config("Allow Empty", DefaultBoolean = true)]
        public IDiffSpread<bool> FAllowEmtpy;

        [Output("Output")]
        public ISpread<ISpread<object>> FOut;

        List<GenericInput> Inputs = new List<GenericInput>();

        protected override void PreInitialize()
        {
            ConfigPinCopy = FCount;
        }

        protected override bool IsConfigDefault()
        {
            return FCount[0] == 0;
        }

        protected override void Initialize()
        {
            CreatePins(FCount[0]);
        }

        protected override void OnConfigPinChanged()
        {
            if (Inputs.Count == FCount[0]) return;
            CreatePins(FCount[0]);
        }


        protected void CreatePins(int count)
        {
            if (Inputs.Count < count)
            {
                for (int i = Inputs.Count; i < count; i++)
                {
                    int ii = i + 1;
                    var attr = new InputAttribute("Input " + ii);
                    attr.Order = i;
                    var pin = new GenericInput(FPluginHost, attr);
                    Inputs.Add(pin);
                }
            }
            if (Inputs.Count > count)
            {
                for (int i = Inputs.Count-1; i >= count; i--)
                {
                    FPluginHost.DeletePin(Inputs[i].Pin);
                    Inputs.RemoveAt(i);
                }
            }
        }

        public void Evaluate(int SpreadMax)
        {
            int bsc = 0;
            int sc = 0;
            if (FAllowEmtpy[0])
            {
                for (int i = 0; i < Inputs.Count; i++)
                {
                    bool valid = (Inputs[i].Pin.SliceCount != 0) && (Inputs[i].Pin.IsConnected);
                    if (valid) bsc++;
                    sc = Math.Max(sc, Inputs[i].Pin.SliceCount);
                }
                FOut.SliceCount = sc;
            }
            else
            {
                for (int i = 0; i < Inputs.Count; i++)
                {
                    if (Inputs[i].Pin.SliceCount == 0)
                    {
                        FOut.SliceCount = 0;
                        return;
                    }
                    sc = Math.Max(sc, Inputs[i].Pin.SliceCount);
                }
                bsc = Inputs.Count;
            }
            for (int i = 0; i < sc; i++)
            {
                FOut[i].SliceCount = bsc;
                int ii = 0;
                for (int j = 0; j < Inputs.Count; j++)
                {
                    bool valid = (Inputs[j].Pin.SliceCount != 0) && (Inputs[j].Pin.IsConnected);
                    if (!valid) continue;
                    FOut[i][ii] = Inputs[j][i];
                    ii++;
                }
            }
        }
    }

    [PluginInfo(Name = "Unzip",
        Category = "Object",
        Tags = "split, generic, spreadop"
        )]
    public class VObjectUnzipNode : ConfigurableDynamicPinNode<int>, IPluginEvaluate
    {
        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;

        [Config("Count", DefaultValue = 0)] public IDiffSpread<int> FCount;

        public GenericInput FInput;

        private PinDictionary pd;

        protected override void PreInitialize()
        {
            ConfigPinCopy = FCount;
            var attr = new InputAttribute("Input");
            attr.Order = 0;
            FInput = new GenericInput(FPluginHost, attr);
            pd = new PinDictionary(FIOFactory);
        }

        protected override bool IsConfigDefault()
        {
            return FCount[0] == 0;
        }

        protected override void Initialize()
        {
            CreatePins(FCount[0]);
        }

        protected override void OnConfigPinChanged()
        {
            if (pd.OutputPins.Count == FCount[0]) return;
            CreatePins(FCount[0]);
        }

        protected void CreatePins(int count)
        {
            if (pd.OutputPins.Count < count)
            {
                for (int i = pd.OutputPins.Count; i < count; i++)
                {
                    int ii = i + 1;
                    var attr = new OutputAttribute("Output " + ii);
                    attr.Order = i*2;
                    attr.BinOrder = i*2 + 1;
                    pd.AddOutput(typeof(object), attr);
                }
            }
            if (pd.OutputPins.Count > count)
            {
                for (int i = pd.OutputPins.Count - 1; i >= count; i--)
                {
                    int ii = i + 1;
                    string name = "Output " + ii;
                    pd.RemoveOutput(name);
                }
            }
        }

        public void Evaluate(int SpreadMax)
        {
            if (pd.OutputPins.Count == 0) return;
            foreach (var P in pd.OutputPins.Values)
            {
                P.Spread.SliceCount = (int)Math.Ceiling((float)FInput.Pin.SliceCount / (float)pd.OutputPins.Count);
            }
            int step = 0;
            int slice = 0;
            while (step*pd.OutputPins.Count <= FInput.Pin.SliceCount)
            {
                for (int i = 0; i < pd.OutputPins.Count; i++)
                {
                    int ii = i + 1;
                    string name = "Output " + ii;
                    pd.OutputPins[name].Spread[step] = FInput[slice];
                    slice++;
                }
                step++;
            }
        }
    }
}
