using System;
using System.Collections.Generic;
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
    public class Smoothing
    {
        public IHDEHost HDEHost;
        public List<ObjectTypePair> TargetValues = new List<ObjectTypePair>();
        public List<List<double>> CurrentValues = new List<List<double>>();
        private Stopwatch InternalTime = new Stopwatch();
        private double LastTotalSeconds = 0;
        public double FilterTime = 1;
        public string VPath = "";
        public string VPathSeparator = "¦";
        public VObjectCollection Root;
        public SmoothingAlgorithm Algorithm;

        public Smoothing(IHDEHost hde, string vpath, string vpathsep, VObjectCollection root)
        {
            this.HDEHost = hde;
            this.HDEHost.MainLoop.OnPrepareGraph += this.EveryFrame;
            this.VPath = vpath;
            this.VPathSeparator = vpathsep;
            this.Root = root;
            this.CheckPath();
            this.InternalTime.Start();
        }
        public void CheckPath()
        {
            List<object> VPathRes = this.Root.VPath(this.VPath, this.VPathSeparator);
            foreach (ObjectTypePair otp in VPathRes)
            {
                if (otp is ObjectTypePair)
                {
                    if((otp.Type == typeof(double)) || (otp.Type == typeof(float)))
                    {
                        List<double> tmplist = new List<double>();
                        this.TargetValues.Add(otp);
                        foreach(object obj in otp.Objects)
                        {
                            if(obj is float) tmplist.Add((float)obj);
                            if(obj is double) tmplist.Add((double)obj);
                        }
                        this.CurrentValues.Add(tmplist);
                    }
                }
            }
        }
        public void EveryFrame(object sender, EventArgs args)
        {
            double frametime = this.InternalTime.Elapsed.TotalSeconds - this.LastTotalSeconds;
            for(int i=0; i<this.CurrentValues.Count; i++)
            {
                ObjectTypePair otp = this.TargetValues[i];
                for(int j=0; j<this.CurrentValues[i].Count; j++)
                {
                    if (this.Algorithm == null)
                    {
                        if (this.TargetValues[i].Objects[j] is double)
                        {
                            double target = (double)this.TargetValues[i].Objects[j];
                            double dist = target - this.CurrentValues[i][j];
                            this.CurrentValues[i][j] += dist * frametime / FilterTime * 6;
                        }
                        if (this.TargetValues[i].Objects[j] is float)
                        {
                            float target = (float)this.TargetValues[i].Objects[j];
                            float dist = target - (float)this.CurrentValues[i][j];
                            this.CurrentValues[i][j] += dist * frametime / FilterTime * 6;
                        }
                    }
                    else
                    {
                        if (this.TargetValues[i].Objects[j] is double)
                        {
                            this.CurrentValues[i][j] =
                                this.Algorithm.Algorithm(
                                    (double)this.TargetValues[i].Objects[j],
                                    this.CurrentValues[i][j],
                                    frametime,
                                    this.FilterTime
                                );
                        }
                        if (this.TargetValues[i].Objects[j] is float)
                        {
                            float TempVal = (float)this.TargetValues[i].Objects[j];
                            this.CurrentValues[i][j] =
                                this.Algorithm.Algorithm(
                                    (double)TempVal,
                                    this.CurrentValues[i][j],
                                    frametime,
                                    this.FilterTime
                                );
                        }
                    }
                }
            }
            this.LastTotalSeconds = this.InternalTime.Elapsed.TotalSeconds;
        }
    }
    public class SmoothingWrap : VObject
    {
        public SmoothingWrap() : base() { }
        public SmoothingWrap(Smoothing o) : base(o) { }

        public SmoothingWrap(Stream s) : base(s) { }

        public override void DeSerialize(Stream Input)
        {
            base.DeSerialize(Input);
            Stopwatch st = new Stopwatch();
            this.Content = st;
        }
        public override void Dispose()
        {
            Smoothing tc = this.Content as Smoothing;
            tc.HDEHost.MainLoop.OnPrepareGraph -= tc.EveryFrame;
            base.Dispose();
        }

        public override VObject DeepCopy()
        {
            Stopwatch st = new Stopwatch();
            StopwatchWrap stw = new StopwatchWrap(st);
            return stw;
        }
    }
}
