using System.Collections.Generic;
using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(Name = "Info",
                Category = "PrimitiveObject",
                Author = "microdee"
                )]
    public class PrimitiveObjectInfoNode : IPluginEvaluate
    {
        [Input("Primitive Object")]
        public Pin<object> FInput;

        [Output("Children")]
        public ISpread<ISpread<string>> FChildren;
        [Output("Type")]
        public ISpread<ISpread<string>> FType;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FType.SliceCount = FInput.SliceCount;
                FChildren.SliceCount = FInput.SliceCount;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is PrimitiveObject)
                    {
                        PrimitiveObject pro = FInput[i] as PrimitiveObject;

                        FChildren[i].SliceCount = pro.Fields.Count;
                        FType[i].SliceCount = pro.Fields.Count;
                        List<List<object>> otpl = pro.Fields.Values.ToList();
                        List<string> names = pro.Fields.Keys.ToList();
                        for(int j=0; j<pro.Fields.Count; j++)
                        {
                            FChildren[i][j] = names[j];
                            FType[i][j] = otpl[j][0].GetType().ToString();
                        }
                    }
                    else
                    {
                        FChildren[i].SliceCount = 0;
                        FType[i].SliceCount = 0;
                    }
                }
            }
            else
            {
                FType.SliceCount = 0;
                FChildren.SliceCount = 0;
            }
        }
    }
}
