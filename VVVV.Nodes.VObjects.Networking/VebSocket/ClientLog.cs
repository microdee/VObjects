using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

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
        public Pin<VObject> FInput;

        [Output("Output")]
        public ISpread<ISpread<string>> FOutput;
        [Output("File")]
        public ISpread<string> FFile;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = SpreadMax;
                FFile.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FInput[i] is VebSocketClientWrap)
                    {
                        VebSocketClient vs = FInput[i].Content as VebSocketClient;
                        FFile[i] = vs.Client.Log.File;
                        FOutput[i].SliceCount = 0;
                        foreach (string s in vs.LogMessages.Values)
                        {
                            FOutput[i].Add(s);
                        }
                    }
                }
            }
        }
    }
}
