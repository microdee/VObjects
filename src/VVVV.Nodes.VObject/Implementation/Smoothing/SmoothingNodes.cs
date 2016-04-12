using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(Name = "Construct", Category = "Smoothing", AutoEvaluate = true)]
    public class SmoothingConstructNode : ConstructObjectNode
    {
        [Import]
        public IHDEHost FHDEHost;

        [Input("Filter Time", DefaultValue = 1.0, Order = 10)]
        public ISpread<double> FFilterTime;
        [Input("VPath", Order = 11)]
        public ISpread<string> FVPath;
        [Input("VPath Separator", Order = 12, DefaultString = "¦")]
        public ISpread<string> FVPathSep;
        [Input("Root Collection", Order = 13)]
        public ISpread<object> FRoot;
        [Input("Smoothing Algorithm", Order = 14)]
        public ISpread<SmoothingAlgorithm> FAlgorithm;

        public override object ConstructObject()
        {
            if (FRoot[this.CurrObj] is VObjectCollection)
            {
                VObjectCollection vc = FRoot[this.CurrObj] as VObjectCollection;
                Smoothing NewObj = new Smoothing(FHDEHost, FVPath[this.CurrObj], FVPathSep[this.CurrObj], vc);
                NewObj.Algorithm = FAlgorithm[this.CurrObj];
                NewObj.FilterTime = FFilterTime[this.CurrObj];
                return NewObj;
            }
            else return null;
        }
    }

    [PluginInfo(Name = "Smoothing", Category = "Smoothing", AutoEvaluate = true)]
    public class SmoothingSmoothingNode : IPluginEvaluate
    {
        [Input("Input Smoothing")]
        public Pin<object> FInput;
        [Input("Filter Time", IsBang = true)]
        public ISpread<double> FFilterTime;
        [Input("Set", IsBang = true)]
        public ISpread<bool> FSet;
        [Input("Reset", IsBang = true)]
        public ISpread<bool> FReset;

        [Output("Position")]
        public ISpread<double> FCurrent;
        [Output("Target")]
        public ISpread<double> FTarget;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FCurrent.SliceCount = 0;
                FTarget.SliceCount = 0;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is Smoothing)
                    {
                        Smoothing Content = FInput[i] as Smoothing;
                        for (int j = 0; j < Content.CurrentValues.Count; j++)
                        {
                            for (int k = 0; k < Content.CurrentValues[j].Count; k++)
                            {
                                if (Content.TargetValues[j][k] is double)
                                {
                                    FTarget.Add((double)Content.TargetValues[j][k]);
                                    FCurrent.Add(Content.CurrentValues[j][k]);
                                }
                                if (Content.TargetValues[j][k] is float)
                                {
                                    FTarget.Add((float)Content.TargetValues[j][k]);
                                    FCurrent.Add(Content.CurrentValues[j][k]);
                                }
                            }
                        }
                        if (FSet[i]) Content.FilterTime = FFilterTime[i];
                        if (FReset[i]) Content.CheckPath();
                    }
                }
            }
            else
            {
                FCurrent.SliceCount = 0;
                FTarget.SliceCount = 0;
            }
        }
    }
}
