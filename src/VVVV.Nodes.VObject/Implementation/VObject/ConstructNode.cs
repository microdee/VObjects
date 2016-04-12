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
        Name = "Construct",
        Category = "VObject",
        Author = "microdee",
        AutoEvaluate = true
    )]
    public class VObjectConstructNode : ConstructAndSetObjectNode, IPartImportsSatisfiedNotification
    {
        [Import] protected IPluginHost2 FPluginHost;
        [Import] protected IIOFactory FIOFactory;

        [Config("Formular", DefaultString = "")] public IDiffSpread<string> FFormular;

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
                attr.Order = order*2;
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
                        Inputs[f].Pin.Order = order*2;
                        Inputs[f].BinSizePin.Order = order*2 + 1;
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

        public override void SetSliceCount(int SpreadMax)
        {
            int smax = Inputs.Values.Select(p => p.SliceCount).Concat(new[] {0}).Max();
            SliceCount = smax;
        }

        public override object ConstructObject()
        {
            var vobj = new VObject();
            int i = CurrObj;
            foreach (var k in Inputs.Keys)
            {
                vobj.Add(k, Inputs[k][i]);
            }
            return vobj;
        }

        public override void SetObject()
        {
            int i = CurrObj;
            var vobj = FOutput[i] as VObject;
            if (vobj == null) return;
            foreach (var k in Inputs.Keys)
            {
                var objl = Inputs[k][i];
                vobj.Set(k, objl);
            }
        }

        public void OnImportsSatisfied()
        {
            FFormular.Changed += OnConfigPinChanged;
        }
    }
}
