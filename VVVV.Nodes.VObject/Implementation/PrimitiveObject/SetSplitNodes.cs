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
                Version = "Set",
                AutoEvaluate = true,
                Author = "microdee"
                )]
    public class SetPrimitiveObjectNode : DynamicPrimitiveObjectNode
    {
        [Input("Primitive Object")]
        public Pin<object> FInput;
        [Input("Not-Existing Behavior", DefaultEnumEntry = "Create")]
        public IDiffSpread<ManageNotExisting> FNotExist;
        [Input("Set", IsBang = true)]
        public IDiffSpread<bool> FSet;

        [Output("Valid")]
        public ISpread<bool> FValid;

        public int CurrObj = 0;

        public void SetObject(PrimitiveObject pro)
        {
            int i = CurrObj;
            for (int ii = 0; ii < Spreads.Count; ii++)
            {
                Type type = IdentityType.Instance[TypesAndFields[ii][0]];
                string name = TypesAndFields[ii][1];
                NGISpread currspread = (NGISpread)Spreads[name][i];

                if (pro.Fields.ContainsKey(name))
                {
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
                else
                {
                    if (FNotExist[0] == ManageNotExisting.Create)
                    {
                        List<object> objl = new List<object>();
                        for (int j = 0; j < currspread.SliceCount; j++)
                        {
                            objl.Add(currspread[j]);
                        }
                        pro.Add(name, objl);
                    }
                }
            }
        }

        public override void OnEval()
        {
            if(FInput.IsConnected)
            {
                FValid.SliceCount = FInput.SliceCount;
                for(int i=0; i<FInput.SliceCount; i++)
                {
                    FValid[i] = FInput[i] is PrimitiveObject;
                    if(FValid[i])
                    {
                        CurrObj = i;
                        if(FSet[i])
                            SetObject(FInput[i] as PrimitiveObject);
                    }
                }
            }
        }
    }

    [PluginInfo(Name = "PrimitiveObject",
                Category = "VObject",
                Version = "Split",
                AutoEvaluate = true,
                Author = "microdee"
                )]
    public class GetPrimitiveObjectNode : DynamicPrimitiveObjectNode, IPluginEvaluate
    {
        [Input("Primitive Object")]
        public Pin<object> FInput;

        [Output("Valid")]
        public ISpread<bool> FValid;

        public override IOAttribute PinAttribute(string name, int order)
        {
            OutputAttribute attr = new OutputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.Order = order * 2;
            attr.BinOrder = order * 2 + 1;
            return attr;
        }

        public int CurrObj = 0;

        public void GetObject(PrimitiveObject pro)
        {
            int i = CurrObj;
            for (int ii = 0; ii < TypesAndFields.Count; ii++)
            {
                Type type = IdentityType.Instance[TypesAndFields[ii][0]];
                string name = TypesAndFields[ii][1];
                NGISpread currspread = Spreads[name];
                currspread.SliceCount = FInput.SliceCount;
                NGISpread currvalues = (NGISpread)currspread[i];

                if (pro.Fields.ContainsKey(name))
                {
                    List<object> objl = pro[name];
                    currvalues.SliceCount = objl.Count;

                    for (int j = 0; j < currvalues.SliceCount; j++)
                    {
                        currvalues[j] = objl[j];
                    }
                }
                else
                {
                    currvalues.SliceCount = 0;
                }
            }
        }

        public override void OnEval()
        {
            if (FInput.IsConnected && (FInput.SliceCount != 0))
            {
                FValid.SliceCount = FInput.SliceCount;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    FValid[i] = FInput[i] is PrimitiveObject;
                    if (FValid[i])
                    {
                        CurrObj = i;
                        GetObject(FInput[i] as PrimitiveObject);
                    }
                }
            }
            else
            {
                FValid.SliceCount = 0;
                for (int ii = 0; ii < TypesAndFields.Count; ii++)
                {
                    Type type = IdentityType.Instance[TypesAndFields[ii][0]];
                    string name = TypesAndFields[ii][1];
                    NGISpread currspread = Spreads[name];
                    currspread.SliceCount = 0;
                }
            }
        }
    }
}
