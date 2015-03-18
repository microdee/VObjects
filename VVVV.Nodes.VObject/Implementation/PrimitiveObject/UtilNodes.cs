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
    [PluginInfo(Name = "Info",
                Category = "PrimitiveObject",
                Author = "microdee"
                )]
    public class PrimitiveObjectInfoNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VObject> FInput;

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
                    if (FInput[i] is PrimitiveObjectWrap)
                    {
                        PrimitiveObjectWrap prow = FInput[i] as PrimitiveObjectWrap;
                        PrimitiveObject pro = prow.Content as PrimitiveObject;

                        FChildren[i].SliceCount = pro.Fields.Count;
                        FType[i].SliceCount = pro.Fields.Count;
                        List<ObjectTypePair> otpl = pro.Fields.Values.ToList();
                        List<string> names = pro.Fields.Keys.ToList();
                        for(int j=0; j<pro.Fields.Count; j++)
                        {
                            FChildren[i][j] = names[j];
                            FType[i][j] = otpl[j].Type.ToString();
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
