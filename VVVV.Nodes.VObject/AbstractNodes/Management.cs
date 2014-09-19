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
    public abstract class ServerNode<WrappedObject> : IPluginEvaluate
    {
        [Input("Clear", IsBang = true)]
        public IDiffSpread<bool> FClear;

        [Output("Output")]
        public ISpread<VObject> FOutput;

        public virtual void Cast();
        public virtual void Clear();
        public virtual void Remove();

        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = 1;
            this.Clear();
            this.Remove();
            this.Cast();
        }
    }
}
