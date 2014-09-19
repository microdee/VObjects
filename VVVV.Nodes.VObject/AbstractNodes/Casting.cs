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

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    public abstract class SoucreCastNode<DerivedVObject> : IPluginEvaluate where DerivedVObject : VObject
    {
        [Input("Input")]
        public IDiffSpread<DerivedVObject> FInput;

        [Output("Output")]
        public ISpread<VObject> FOutput;

        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                FOutput[i] = (VObject)FInput[i];
            }
        }
    }
    public abstract class SinkCastNode<DerivedVObject> : IPluginEvaluate where DerivedVObject : VObject
    {
        [Input("Input")]
        public IDiffSpread<VObject> FInput;

        [Output("Output")]
        public ISpread<DerivedVObject> FOutput;
        [Output("Valid")]
        public ISpread<bool> FValid;

        private List<int> ValidID = new List<int>();

        public void Evaluate(int SpreadMax)
        {
            FValid.SliceCount = SpreadMax;

            int validcount = 0;
            ValidID.Clear();
            for(int i=0; i<SpreadMax; i++)
            {
                FValid[i] = false;
                if(FInput[i] is DerivedVObject)
                {
                    FValid[i] = true;
                    ValidID.Add(i);
                    validcount++;
                }
            }

            FOutput.SliceCount = validcount;
            for(int i=0; i<validcount; i++)
            {
                FOutput[i] = (DerivedVObject)FInput[ValidID[i]];
            }
        }
    }
}
