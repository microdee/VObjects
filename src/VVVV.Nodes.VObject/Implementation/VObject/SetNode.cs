using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Nodes.PDDN;
using VVVV.Packs.VObjects;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(
        Name = "Set",
        Category = "VObject",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class VObjectSetNode: IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;

        [Config("Formular", DefaultString = "")] public IDiffSpread<string> FFormular;

        [Input("VObject", Order = 0)] public Pin<object> FInput;
        [Input("Create Not Existing", Visibility = PinVisibility.OnlyInspector, Order = 3)]
        public ISpread<bool> FCreateNotExist;
        [Input("Set", IsBang = true, Order = 4)] public ISpread<bool> FSet;

        [Output("Connected Config", Visibility = PinVisibility.OnlyInspector, BinVisibility = PinVisibility.OnlyInspector, Order = 0, BinOrder = 1)]
        public ISpread<ISpread<string>> FConfig;

        Dictionary<string, GenericBinSizedInput> Inputs = new Dictionary<string, GenericBinSizedInput>();
        Dictionary<string, bool> Preserve = new Dictionary<string, bool>();

        protected bool Initialized = false;

        protected void Initialize()
        {
            string[] fields = FFormular[0].Split(',');
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = fields[i].Trim();
            }

            int order = 5;
            foreach (var f in fields)
            {
                var attr = new InputAttribute(f);
                attr.Order = order * 2;
                attr.BinOrder = order * 2 + 1;
                Inputs.Add(f, new GenericBinSizedInput(FPluginHost, attr));
                Preserve.Set(f, true);
                order++;
            }
        }

        protected void OnConfigPinChanged(IDiffSpread<string> spread)
        {
            if (Initialized)
            {
                string[] fields = FFormular[0].Split(',');
                for (int i = 0; i < fields.Length; i++)
                {
                    fields[i] = fields[i].Trim();
                }
                var pkeys = Preserve.Keys.ToList();

                for (int i = 0; i < pkeys.Count; i++)
                {
                    Preserve[pkeys[i]] = false;
                }
                int order = 5;
                foreach (var f in fields)
                {
                    if (Inputs.ContainsKey(f))
                    {
                        Inputs[f].Pin.Order = order * 2;
                        Inputs[f].BinSizePin.Order = order * 2 + 1;
                    }
                    else
                    {
                        var attr = new InputAttribute(f);
                        attr.Order = order * 2;
                        attr.BinOrder = order * 2 + 1;
                        Inputs.Add(f, new GenericBinSizedInput(FPluginHost, attr));
                    }
                    Preserve.Set(f, true);
                    order++;
                }
                foreach (var k in Preserve.Keys)
                {
                    if (Inputs.ContainsKey(k))
                    {
                        if (!Preserve[k])
                        {
                            FPluginHost.DeletePin(Inputs[k].Pin);
                            FPluginHost.DeletePin(Inputs[k].BinSizePin);
                            Inputs.Remove(k);
                        }
                    }
                }
                Preserve.Clear();
                foreach (var k in Inputs.Keys)
                {
                    Preserve.Add(k, true);
                }
            }
            else
            {
                if (FFormular[0] == "") return;
                Initialize();
                Initialized = true;
            }
        }
        public void OnImportsSatisfied()
        {
            FFormular.Changed += OnConfigPinChanged;
        }

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FConfig.SliceCount = FInput.SliceCount;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is VObject)
                    {
                        var cvobj = (VObject)FInput[i];
                        if (FSet[i])
                        {
                            foreach (var k in Inputs.Keys)
                            {
                                cvobj.Set(k, Inputs[k][i].ToList());
                            }
                        }
                        FConfig[i].SliceCount = cvobj.Fields.Count;
                        var klist = cvobj.VPathQueryKeys();
                        for (int j = 0; j < klist.Length; j++)
                        {
                            FConfig[i][j] = klist[j];
                        }
                    }
                    else
                    {
                        FConfig[i].SliceCount = 0;
                    }
                }
            }
        }
    }
}
