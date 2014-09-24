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

        [Output("Type")]
        public ISpread<string> FType;

        public void Evaluate(int SpreadMax)
        {
            if(FInput.IsConnected)
            {
                FType.SliceCount = FInput.SliceCount;
                for(int i=0; i<FInput.SliceCount; i++)
                {
                    FType[i] = FInput[i].ObjectType.ToString();
                }
            }
            else
            {
                FType.SliceCount = 0;
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
                            if (FType[i] != FInput[j].ObjectType.ToString())
                            {
                                FOutput[i].Add(FInput[j]);
                            }
                        }
                        else
                        {
                            if (FType[i] == FInput[j].ObjectType.ToString())
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

        // List<int> ToBeRemoved = new List<int>();

        public void Evaluate(int SpreadMax)
        {
            FOut.SliceCount = FInput.SliceCount;

            // ToBeRemoved.Clear();
            for(int i=0; i<FInput.SliceCount; i++)
            {
                if (FDeSerialize[i])
                {
                    try
                    {
                        FInput[i].Position = 0;
                        uint typeL = FInput[i].ReadUint();
                        Type ObjType = Type.GetType(FInput[i].ReadUnicode((int)typeL));

                        FInput[i].Position = 0;
                        Object[] ConstrArgs = new Object[] { FInput[i] };
                        VObject NewObject = Activator.CreateInstance(ObjType, ConstrArgs) as VObject;

                        FOut[i] = NewObject;
                    }
                    catch (Exception e)
                    {
                        FError[0] = e.Message;
                    }
                }
                // else ToBeRemoved.Add(i);
            }

            /*
            for(int i=0; i<ToBeRemoved.Count; i++)
            {
                FOut.RemoveAt(ToBeRemoved[i]);
            }
            */
        }
    }
}