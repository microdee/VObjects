using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    public class Smoothing
    {
        public IHDEHost HDEHost;
        public List<List<object>> TargetValues = new List<List<object>>();
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
            foreach (object o in VPathRes)
            {
                if (o is List<object>)
                {
                    var t = o as List<object>;
                    if (t[0] is double || t[0] is float)
                    {
                        List<double> tmplist = new List<double>();
                        this.TargetValues.Add(t);
                        foreach(object obj in t)
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
                List<object> otp = this.TargetValues[i];
                for(int j=0; j<this.CurrentValues[i].Count; j++)
                {
                    if (this.Algorithm == null)
                    {
                        if (this.TargetValues[i][j] is double)
                        {
                            double target = (double)this.TargetValues[i][j];
                            double dist = target - this.CurrentValues[i][j];
                            this.CurrentValues[i][j] += dist * frametime / FilterTime * 6;
                        }
                        if (this.TargetValues[i][j] is float)
                        {
                            float target = (float)this.TargetValues[i][j];
                            float dist = target - (float)this.CurrentValues[i][j];
                            this.CurrentValues[i][j] += dist * frametime / FilterTime * 6;
                        }
                    }
                    else
                    {
                        if (this.TargetValues[i][j] is double)
                        {
                            this.CurrentValues[i][j] =
                                this.Algorithm.Algorithm(
                                    (double)this.TargetValues[i][j],
                                    this.CurrentValues[i][j],
                                    frametime,
                                    this.FilterTime
                                );
                        }
                        if (this.TargetValues[i][j] is float)
                        {
                            float TempVal = (float)this.TargetValues[i][j];
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
}
