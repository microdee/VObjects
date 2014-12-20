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
    public class SmoothingAlgorithm
    {
        // for custom parameters inherit this class

        /* double Algorithm(
         *      double Target,
         *      double Previous,
         *      double FrameTime,
         *      double FilterTime
         *      )
         */
        public Func<double, double, double, double, double> Algorithm;

        public SmoothingAlgorithm() { }
    }
    public class DamperFilter : SmoothingAlgorithm
    {
        private double Damper(double Target, double Previous, double FrameTime, double FilterTime)
        {
            double dist = Target - Previous;
            return Previous + dist * FrameTime / FilterTime * 6;
        }

        public DamperFilter()
        {
            this.Algorithm = this.Damper;
        }
    }
    public class LinearFilter : SmoothingAlgorithm
    {
        public double Epsilon = 0.01;
        private double Linear(double Target, double Previous, double FrameTime, double FilterTime)
        {
            double dist = Target - Previous;
            if (Math.Abs(dist) < this.Epsilon) return Target;
            else return Previous + FrameTime / FilterTime * Math.Sign(dist);
        }

        public LinearFilter()
        {
            this.Algorithm = this.Linear;
        }
    }
}
