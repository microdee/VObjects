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

using WebSocketSharp;

namespace VVVV.Nodes.VObjects
{

    #region PluginInfo
    [PluginInfo(
        Name = "Log",
        Category = "VebSocket",
        Version = "Client",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketClientLogNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VebSocketClientWrap> FInput;

        [Output("Output")]
        public ISpread<ClientMessageWrap> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    VebSocketClient vs = FInput[i].Content as VebSocketClient;
                }
            }
        }
    }
}
