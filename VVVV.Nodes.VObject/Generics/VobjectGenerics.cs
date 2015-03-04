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

    #region PluginInfo
    [PluginInfo(Name = "Serialize", Category = "VObject", Help = "Convert VObject to Raw", Tags = "microdee")]
    #endregion PluginInfo
    public class VObjectSerializeNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VObject> FInput;
        [Input("Serialize", IsBang = true)]
        public ISpread<bool> FSerialize;

        [Output("Output")]
        public ISpread<Stream> FOut;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FOut.SliceCount = FInput.SliceCount;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FSerialize[i]) FInput[i].Serialize();
                    FOut[i] = FInput[i].Serialized;
                }
            }
            else
            {
                FOut.SliceCount = 0;
            }
        }
    }
    /*
    #region PluginInfo
    [PluginInfo(Name = "DeSerialize", Category = "VObject", Version = "Generic", Help = "Convert Raw to VObject", Tags = "microdee")]
    #endregion PluginInfo
    public class GenericVObjectDeSerializeNode : IPluginEvaluate
    {
        [Input("Input")]
        public ISpread<Stream> FInput;
        [Input("DeSerialize", IsBang = true)]
        public ISpread<bool> FDeSerialize;

        [Output("Output")]
        public ISpread<VObject> FOut;
        [Output("Error")]
        public ISpread<string> FError;
        [Output("Type")]
        public ISpread<string> FType;

        int fc = 0;
        // List<int> ToBeRemoved = new List<int>();

        public void Evaluate(int SpreadMax)
        {
            FError.SliceCount = FInput.SliceCount;
            FType.SliceCount = FInput.SliceCount;
            bool clear = false;
            for (int i = 0; i < FInput.SliceCount; i++)
            {
                if (FDeSerialize[i]) clear = true;
            }
            if (clear || fc==0) FOut.SliceCount = 0;
            fc++;
            // ToBeRemoved.Clear();
            for(int i=0; i<FInput.SliceCount; i++)
            {
                if (FDeSerialize[i])
                {
                    FInput[i].Position = 0;
                    uint typeL = FInput[i].ReadUint();
                    FType[i] = FInput[i].ReadUnicode((int)typeL);
                    Type ObjType = Type.GetType(FType[i]);
                        
                    FInput[i].Position = 0;
                    Stream temp = new MemoryStream();
                    temp.SetLength(0);
                    FInput[i].CopyTo(temp);
                    temp.Position = 0;
                    VObject NewObject = DynamicConstruct.ActivatorCreateInstance(temp);

                    FOut.Add(NewObject);
                }
            }
        }
    }
    */
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