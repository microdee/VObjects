using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(Name = "GetType", Category = "Object", Tags = "microdee")]
    #endregion PluginInfo
    public class ObjectGetTypeNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<object> FInput;

        [Output("Object Type")]
        public ISpread<string> FType;

        public void Evaluate(int SpreadMax)
        {
            if(FInput.IsConnected)
            {
                FType.SliceCount = FInput.SliceCount;
                for(int i=0; i<FInput.SliceCount; i++)
                {
                    FType[i] = FInput[i].GetType().ToString();
                }
            }
            else
            {
                FType.SliceCount = 0;
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "FilterType", Category = "Object", Help = "Filter objects by type", Tags = "microdee")]
    #endregion PluginInfo
    public class ObjectFilterTypeNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<object> FInput;
        [Input("Type")]
        public ISpread<string> FType;
        [Input("Exclude")]
        public ISpread<bool> FExclude;

        [Output("Output")]
        public ISpread<ISpread<object>> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOutput.SliceCount = FType.SliceCount;
                for (int i = 0; i < FType.SliceCount; i++)
                {
                    FOutput[i].SliceCount = 0;
                    for(int j = 0; j<FInput.SliceCount; j++)
                    {
                        if (FExclude[i])
                        {
                            if (FType[i] != FInput[j].GetType().ToString())
                            {
                                FOutput[i].Add(FInput[j]);
                            }
                        }
                        else
                        {
                            if ((FType[i] == FInput[j].GetType().ToString()) || (FType[i] == FInput[j].GetType().Name))
                            {
                                FOutput[i].Add(FInput[j]);
                            }
                        }
                    }
                }
            }
            else
            {
                FOutput.SliceCount = 0;
            }
        }
    }
    
    [PluginInfo(Name = "VPath", Category = "VObject")]
    public class VObjectGenericVPathNode : VPathNode
    {
        public override void Sift(object Source, string Filter, List<int> MatchingIndices, List<object> Output)
        {
            if (Source is VPathQueryable)
            {
                VPathQueryable Content = Source as VPathQueryable;
                List<object> result = Content.VPath(Filter, FSeparator[0]);
                foreach (object o in result)
                {
                    Output.Add(o);
                    MatchingIndices.Add(this.CurrentAbsIndex);
                }
            }
        }
    }
}