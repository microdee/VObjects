using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.PDDN;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(Name = "GetType", Category = "Object", Tags = "microdee")]
    #endregion PluginInfo
    public class ObjectGetTypeNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Import] public IPluginHost2 FPluginHost;

        protected GenericInput FInput;

        [Output("Object Type")]
        public ISpread<string> FType;

        public void OnImportsSatisfied()
        {
            FInput = new GenericInput(FPluginHost, new InputAttribute("Input"));
        }

        public void Evaluate(int SpreadMax)
        {
            if(FInput.Connected)
            {
                FType.SliceCount = FInput.Pin.SliceCount;
                for(int i=0; i<FInput.Pin.SliceCount; i++)
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
    public class ObjectFilterTypeNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Import]
        public IPluginHost2 FPluginHost;

        protected GenericInput FInput;

        [Input("Type")]
        public ISpread<string> FType;
        [Input("Exclude")]
        public ISpread<bool> FExclude;

        [Output("Output")]
        public ISpread<ISpread<object>> FOutput;

        public void OnImportsSatisfied()
        {
            FInput = new GenericInput(FPluginHost, new InputAttribute("Input"));
        }
        public void Evaluate(int SpreadMax)
        {
            if (FInput.Connected)
            {
                FOutput.SliceCount = FType.SliceCount;
                for (int i = 0; i < FType.SliceCount; i++)
                {
                    FOutput[i].SliceCount = 0;
                    for(int j = 0; j<FInput.Pin.SliceCount; j++)
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
            if (Source is IVPathQueryable)
            {
                var Content = Source as IVPathQueryable;
                var result = Content.VPath(Filter, FSeparator[0]);
                foreach (object o in result)
                {
                    Output.Add(o);
                    MatchingIndices.Add(this.CurrentAbsIndex);
                }
            }
        }
    }
}