using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Nodes.PDDN;
using VVVV.Packs.VObjects;
using VVVV.PluginInterfaces.V2;
using NGISpread = VVVV.PluginInterfaces.V2.NonGeneric.ISpread;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(
        Name = "Split",
        Category = "VObject",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class VObjectSplitNode : ConfigurableDynamicPinNode<string>, IPluginEvaluate
    {
        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;

        [Config("Formular", DefaultString = "")] public IDiffSpread<string> FFormular;

        [Input("VObject", Order = 0)] public Pin<object> FInput;
        Dictionary<string, bool> Preserve = new Dictionary<string, bool>();

        [Output("Connected Config", Visibility = PinVisibility.OnlyInspector, BinVisibility = PinVisibility.OnlyInspector, Order = 0, BinOrder = 1)]
        public ISpread<ISpread<string>> FConfig;

        private PinDictionary pd;
        protected override void PreInitialize()
        {
            pd = new PinDictionary(FIOFactory);
            ConfigPinCopy = FFormular;
        }

        protected override void Initialize()
        {
            UpdatePins();
        }

        protected void UpdatePins()
        {
            string[] fields = FFormular[0].Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Trim();
            }
            var pkeys = Preserve.Keys.ToList();

            for (int i=0; i<pkeys.Count; i++)
            {
                Preserve[pkeys[i]] = false;
            }
            int order = 1;
            foreach (var f in fields)
            {
                if (pd.OutputPins.ContainsKey(f))
                {
                    Preserve.Set(f, true);
                }
                var attr = new OutputAttribute(f);
                attr.Order = order * 2;
                attr.BinOrder = order * 2 + 1;
                pd.AddOutputBinSized(typeof(object), attr);
                Preserve.Set(f, true);
                order++;
            }
            foreach (var k in Preserve.Keys)
            {
                if(pd.OutputPins.ContainsKey(k))
                    if(!Preserve[k])
                        pd.RemoveOutput(k);
            }
            Preserve.Clear();
            foreach (var k in pd.OutputPins.Keys)
            {
                Preserve.Add(k, true);
            }
        }

        protected override void OnConfigPinChanged()
        {
            if(Initialized)
                UpdatePins();
        }

        protected override bool IsConfigDefault()
        {
            return FFormular[0] == "";
        }

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                foreach (var p in pd.OutputPins.Values)
                {
                    p.Spread.SliceCount = FInput.SliceCount;
                }
                FConfig.SliceCount = FInput.SliceCount;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is VObject)
                    {
                        var vobj = (VObject) FInput[i];
                        foreach (var k in pd.OutputPins.Keys)
                        {
                            var cspread = pd.OutputPins[k].Spread[i] as NGISpread;
                            if (vobj.Fields.ContainsKey(k))
                            {
                                var vfield = vobj.Get(k);
                                if (cspread == null) continue;
                                cspread.SliceCount = vfield.Objects.Count;
                                for (int j = 0; j < cspread.SliceCount; j++)
                                {
                                    cspread[j] = vfield[j];
                                }
                            }
                            else
                            {
                                if (cspread != null) cspread.SliceCount = 0;
                            }
                        }
                        FConfig[i].SliceCount = vobj.Fields.Count;
                        var klist = vobj.VPathQueryKeys();
                        for (int j = 0; j < klist.Length; j++)
                        {
                            FConfig[i][j] = klist[j];
                        }
                    }
                    else
                    {
                        foreach (var cspread in pd.OutputPins.Values.Select(p => p.Spread[i]).OfType<NGISpread>())
                        {
                            cspread.SliceCount = 0;
                        }
                        FConfig[i].SliceCount = 0;
                    }
                }
            }
        }
    }
}
