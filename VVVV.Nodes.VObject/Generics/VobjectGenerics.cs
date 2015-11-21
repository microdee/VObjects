using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(Name = "GetType", Category = "VObject", Help = "Get the actual type of the VObject", Tags = "microdee")]
    #endregion PluginInfo
    public class VObjectGetTypeNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VObject> FInput;

        [Output("Object Type")]
        public ISpread<string> FType;
        [Output("Wrapper Type")]
        public ISpread<string> FWrapType;

        public void Evaluate(int SpreadMax)
        {
            if(FInput.IsConnected)
            {
                FType.SliceCount = FInput.SliceCount;
                FWrapType.SliceCount = FInput.SliceCount;
                for(int i=0; i<FInput.SliceCount; i++)
                {
                    FType[i] = FInput[i].ObjectType.ToString();
                    FWrapType[i] = FInput[i].GetType().ToString();
                }
            }
            else
            {
                FType.SliceCount = 0;
                FWrapType.SliceCount = 0;
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "FilterType", Category = "VObject", Help = "Filter VObjects by type", Tags = "microdee")]
    #endregion PluginInfo
    public class VObjectFilterTypeNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VObject> FInput;
        [Input("Type")]
        public ISpread<string> FType;
        [Input("Exclude")]
        public ISpread<bool> FExclude;

        [Output("Output")]
        public ISpread<ISpread<VObject>> FOutput;

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
                            if ((FType[i] != FInput[j].ObjectType.ToString()) || (FType[i] != FInput[j].GetType().ToString()))
                            {
                                FOutput[i].Add(FInput[j]);
                            }
                        }
                        else
                        {
                            if ((FType[i] == FInput[j].ObjectType.ToString()) || (FType[i] == FInput[j].GetType().ToString()))
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
        public override void Sift(VObject Source, string Filter, List<int> MatchingIndices, List<VObject> Output)
        {
            if (Source.Content is VPathQueryable)
            {
                VPathQueryable Content = Source.Content as VPathQueryable;
                List<object> result = Content.VPath(Filter, FSeparator[0]);
                foreach (object o in result)
                {
                    if (o is VObject)
                    {
                        Output.Add(o as VObject);
                        MatchingIndices.Add(this.CurrentAbsIndex);
                    }
                }
            }
        }
    }
}