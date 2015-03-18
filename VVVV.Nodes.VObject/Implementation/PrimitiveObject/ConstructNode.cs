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
using NGISpread = VVVV.PluginInterfaces.V2.NonGeneric.ISpread;
using NGIDiffSpread = VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(Name = "PrimitiveObject",
                Category = "VObject",
                Version = "Construct",
                AutoEvaluate = true,
                Author = "microdee"
                )]
    public class ConstructPrimitiveObjectNode : DynamicPrimitiveObjectNode
    {
        [Input("Construct", IsBang = true, Order = 0)]
        public ISpread<bool> FConstruct;
        [Input("Auto Clear", DefaultBoolean = true, Order = 1)]
        public ISpread<bool> FAutoClear;
        [Input("Set", IsBang = true, Order = 2)]
        public ISpread<bool> FSet;
        [Output("Output", Order = 0)]
        public ISpread<VObject> FOutput;

        public int CurrObj;
        public int SliceCount = 0;
        public int fc = 0;

        public PrimitiveObjectWrap ConstructVObject()
        {
            int i = CurrObj;
            PrimitiveObject pro = new PrimitiveObject();
            for (int ii = 0; ii < Spreads.Count; ii++)
            {
                ObjectTypePair otp = new ObjectTypePair();
                string type = TypesAndFields[ii][0];
                string name = TypesAndFields[ii][1];
                otp.Type = IdentityType.Instance[type];
                NGISpread currspread = (NGISpread)Spreads[name][i];
                otp.Objects.Clear();
                for (int j = 0; j < currspread.SliceCount; j++)
                {
                    otp.Objects.Add(currspread[j]);
                }
                pro.Fields.Add(TypesAndFields[ii][1], otp);
            }
            return new PrimitiveObjectWrap(pro);
        }

        public void SetVObject(PrimitiveObjectWrap Obj)
        {
            int i = CurrObj;
            PrimitiveObject pro = Obj.Content as PrimitiveObject;
            for (int ii = 0; ii < Spreads.Count; ii++)
            {
                string name = TypesAndFields[ii][1];
                NGISpread currspread = (NGISpread)Spreads[name][i];

                ObjectTypePair otp = pro[name];
                if (currspread.SliceCount != otp.Objects.Count)
                {
                    otp.Objects.Clear();
                    for (int j = 0; j < currspread.SliceCount; j++)
                    {
                        otp.Objects.Add(currspread[j]);
                    }
                }
                else
                {
                    for (int j = 0; j < currspread.SliceCount; j++)
                    {
                        otp.Objects[j] = currspread[j];
                    }
                }
            }
        }

        public void SetSliceCount()
        {
            int smax = FConstruct.SliceCount;
            foreach (NGISpread ngis in Spreads.Values)
                smax = Math.Max(smax, ngis.SliceCount);

            this.SliceCount = smax;
        }

        private List<int> Removables = new List<int>();
        public override void OnEval()
        {
            this.SetSliceCount();

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

            bool empty = FOutput.SliceCount == 0;
            for (int i = 0; i < this.SliceCount; i++)
            {
                this.CurrObj = i;
                if (FConstruct[i] || (FSet[i] && empty))
                {
                    PrimitiveObjectWrap ro = ConstructVObject();
                    if (ro != null) FOutput.Add(ro);
                }
            }
            Removables.Clear();
            for (int i = 0; i < FOutput.SliceCount; i++)
            {
                if (FOutput[i].Disposed)
                    Removables.Add(i);

                this.CurrObj = i;
                if (FSet[i])
                {
                    SetVObject(FOutput[i] as PrimitiveObjectWrap);
                }
            }

            for (int i = 0; i < Removables.Count; i++)
                FOutput.RemoveAt(Removables[i]);
        }
    }
}
