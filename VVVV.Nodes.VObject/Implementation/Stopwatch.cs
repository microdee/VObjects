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
    public class StopwatchWrap : VObject
    {
        public StopwatchWrap(Stopwatch o) : base(o.GetType(), o) { }
        public StopwatchWrap(Stream s) : base(typeof(Stopwatch), s) { }
        public override void DeSerialize(Stream Input)
        {
            base.DeSerialize(Input);
            Stopwatch st = new Stopwatch();
            this.Content = st;
        }
        public override VObject DeepCopy()
        {
            Stopwatch st = new Stopwatch();
            StopwatchWrap stw = new StopwatchWrap(st);
            return stw;
        }
    }

    [PluginInfo(Name = "Cast", Category = "To", Version = "Stopwatch")]
    public class ToStopwatchCastNode : CastToNode<StopwatchWrap> { }

    [PluginInfo(Name = "Construct", Category = "Stopwatch")]
    public class StopwatchConstructNode : ConstructVObjectNode<StopwatchWrap>
    {
        [Input("Start", Order = 10)]
        public ISpread<bool> FStart;

        public override StopwatchWrap ConstructVObject()
        {
            Stopwatch NewObj = new Stopwatch();
            if(FStart[this.CurrObj]) NewObj.Start();
            StopwatchWrap NewWrap = new StopwatchWrap(NewObj);
            return NewWrap;
        }
    }

    [PluginInfo(Name = "Stopwatch", Category = "Stopwatch", AutoEvaluate = true)]
    public class StopwatchStopwatchNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<StopwatchWrap> FInput;
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
                    Stopwatch Content = FInput[i].Content as Stopwatch;
                    FSeconds[i] = Content.Elapsed.TotalSeconds;
                    FRunning[i] = Content.IsRunning;
                    if (FStart[i]) Content.Start();
                    if (FRestart[i]) Content.Restart();
                    if (FStop[i]) Content.Stop();
                    if (FReset[i]) Content.Reset();
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
