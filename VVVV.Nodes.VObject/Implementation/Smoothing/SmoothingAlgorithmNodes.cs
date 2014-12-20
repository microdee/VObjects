using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public abstract class SmoothingAlgorithmNode<SmoothingT> : IPluginEvaluate where SmoothingT : SmoothingAlgorithm, new()
    {
        [Input("Reset", IsBang = true)]
        public ISpread<bool> FReset;
        [Output("Algorithm")]
        public ISpread<SmoothingT> FOutput;

        private int fcr = 0;

        public virtual void SetParameters(SmoothingT Algorithm, int i) { }
        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = SpreadMax;
            if (FReset[0]) fcr = 0;
            if (fcr==0)
            {
                for(int i=0; i<SpreadMax; i++)
                {
                    FOutput[i] = new SmoothingT();
                }
            }
            for (int i = 0; i < SpreadMax; i++)
            {
                this.SetParameters(FOutput[i], i);
            }
            fcr++;
        }
    }

    [PluginInfo(Name = "Damper", Category = "SmoothingAlgorithm")]
    public class DamperSmoothingAlgorithmNode : SmoothingAlgorithmNode<DamperFilter> { }
}
