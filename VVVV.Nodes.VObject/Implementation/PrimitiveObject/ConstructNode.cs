using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using NGISpread = VVVV.PluginInterfaces.V2.NonGeneric.ISpread;

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
        [Output("Output Object", Order = 0)]
        public ISpread<object> FOutput;

        public int CurrObj;
        public int SliceCount = 0;
        public int fc = 0;

        public PrimitiveObject ConstructObject()
        {
            int i = CurrObj;
            PrimitiveObject pro = new PrimitiveObject();
            for (int ii = 0; ii < Spreads.Count; ii++)
            {
                List<object> objl = new List<object>();
                string type = TypesAndFields[ii][0];
                string name = TypesAndFields[ii][1];
                NGISpread currspread = (NGISpread)Spreads[name][i];
                objl.Clear();
                for (int j = 0; j < currspread.SliceCount; j++)
                {
                    objl.Add(currspread[j]);
                }
                pro.Fields.Add(TypesAndFields[ii][1], objl);
            }
            return pro;
        }

        public void SetObject(PrimitiveObject pro)
        {
            int i = CurrObj;
            for (int ii = 0; ii < Spreads.Count; ii++)
            {
                string name = TypesAndFields[ii][1];
                NGISpread currspread = (NGISpread)Spreads[name][i];

                List<object> objl = pro[name];
                if (currspread.SliceCount != objl.Count)
                {
                    objl.Clear();
                    for (int j = 0; j < currspread.SliceCount; j++)
                    {
                        objl.Add(currspread[j]);
                    }
                }
                else
                {
                    for (int j = 0; j < currspread.SliceCount; j++)
                    {
                        objl[j] = currspread[j];
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
                    PrimitiveObject ro = ConstructObject();
                    if (ro != null) FOutput.Add(ro);
                }
            }
            for (int i = 0; i < FOutput.SliceCount; i++)
            {
                this.CurrObj = i;
                if (FSet[i])
                {
                    SetObject(FOutput[i] as PrimitiveObject);
                }
            }
        }
    }
}
