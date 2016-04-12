using System.Diagnostics;
using System.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(Name = "Construct", Category = "Stopwatch", AutoEvaluate = true)]
    public class StopwatchConstructNode : ConstructObjectNode
    {
        [Input("Start", Order = 10)]
        public ISpread<bool> FStart;

        public override object ConstructObject()
        {
            Stopwatch NewObj = new Stopwatch();
            if (FStart[this.CurrObj]) NewObj.Start();
            return NewObj;
        }
    }

    [PluginInfo(Name = "Stopwatch", Category = "Stopwatch", AutoEvaluate = true)]
    public class StopwatchStopwatchNode : IPluginEvaluate
    {
        [Input("Input Stopwatch")]
        public Pin<object> FInput;
        [Input("Start", IsBang = true)]
        public ISpread<bool> FStart;
        [Input("Restart", IsBang = true)]
        public ISpread<bool> FRestart;
        [Input("Stop", IsBang = true)]
        public ISpread<bool> FStop;
        [Input("Reset", IsBang = true)]
        public ISpread<bool> FReset;

        [Output("Seconds")]
        public ISpread<double> FSeconds;
        [Output("Running")]
        public ISpread<bool> FRunning;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FSeconds.SliceCount = FInput.SliceCount;
                FRunning.SliceCount = FInput.SliceCount;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is Stopwatch)
                    {
                        Stopwatch Content = FInput[i] as Stopwatch;
                        FSeconds[i] = Content.Elapsed.TotalSeconds;
                        FRunning[i] = Content.IsRunning;
                        if (FStart[i]) Content.Start();
                        if (FRestart[i]) Content.Restart();
                        if (FStop[i]) Content.Stop();
                        if (FReset[i]) Content.Reset();
                    }
                }
            }
            else
            {
                FSeconds.SliceCount = 0;
                FRunning.SliceCount = 0;
            }
        }
    }
}
