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
    [PluginInfo(Name = "PrimitiveObject", AutoEvaluate = true, Category = "Create", Help = "Create a VObject from primitive types", Tags = "Dynamic, Bin, microdee")]
    #endregion PluginInfo
    public class JoinPrimitiveObjectNode : DynamicNode
    {
#pragma warning disable 649, 169
        [Input("Create", IsBang = true, IsSingle = true)]
        ISpread<bool> FCreate;

        [Output("Output", AutoFlush = false)]
        Pin<VObject> FOutput;
#pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type)
        {
            var attr = new InputAttribute(name);
            attr.BinVisibility = PinVisibility.True;
            attr.BinSize = -1;
            attr.Order = FCount;
            attr.BinOrder = FCount + 1;
            attr.AutoValidate = false;  // need to sync all pins manually
            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            TemplateUpdate();

            SpreadMax = 0;
            if (!FCreate[0])
            {
                //				FLogger.Log(LogType.Debug, "skip join");
                FOutput.SliceCount = 0;
                FOutput.Flush();
                return;
            }

            foreach (string name in FPins.Keys)
            {
                var pin = ToISpread(FPins[name]);
                pin.Sync();
                SpreadMax = Math.Max(pin.SliceCount, SpreadMax);
            }


            FOutput.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                PrimitiveObject newobj = new PrimitiveObject();
                foreach(KeyValuePair<string, IIOContainer> kvp in FPins)
                {
                    ObjectTypePair otp = new ObjectTypePair();
                    PluginInterfaces.V2.NonGeneric.ISpread InputSpread = (PluginInterfaces.V2.NonGeneric.ISpread)ToISpread(kvp.Value)[i];

                    otp.Type = InputSpread[0].GetType();
                    for(int j=0; j<InputSpread.SliceCount; j++)
                    {
                        otp.Objects.Add(InputSpread[j]);
                    }

                    newobj.Add(kvp.Key, otp);
                }

                PrimitiveObjectWrap wrapper = new PrimitiveObjectWrap(newobj);

                FOutput[i] = wrapper;
            }
            FOutput.Flush();
        }
    }
    
    #region PluginInfo
    [PluginInfo(Name = "PrimitiveObject", AutoEvaluate = true, Category = "Split", Help = "Splits a Primitive Object into custom dynamic pins", Tags = "Dynamic, Bin, microdee")]
    #endregion PluginInfo
    public class SplitMessageNode : DynamicNode
    {
        public enum HoldEnum
        {
            Off,
            VObject,
            Pin
        }

#pragma warning disable 649, 169
        [Input("Primitive Object")]
        IDiffSpread<VObject> FInput;

        [Input("Match Rule", DefaultEnumEntry = "All", IsSingle = true)]
        IDiffSpread<SelectEnum> FSelect;

        [Input("Hold if Nil", IsSingle = true, DefaultEnumEntry = "Off")]
        ISpread<HoldEnum> FHold;

        [Output("Valid", AutoFlush = false)]
        ISpread<bool> FValid;

#pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type)
        {
            var attr = new OutputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.AutoFlush = false;

            attr.Order = FCount;
            attr.BinOrder = FCount + 1;
            return attr;
        }

        public override void Evaluate(int SpreadMax)
        {
            TemplateUpdate();

            SpreadMax = (FSelect[0] != SelectEnum.First) ? FInput.SliceCount : 1;

            if (!FInput.IsChanged)
            {
                //				FLogger.Log(LogType.Debug, "skip split");
                return;
            }

            bool empty = (FInput.SliceCount == 0) || (FInput[0] == null);

            if (empty && (FHold[0] == HoldEnum.Off))
            {
                foreach (string name in FPins.Keys)
                {
                    var pin = ToISpread(FPins[name]);
                    pin.SliceCount = 0;
                    pin.Flush();
                }
                FValid.SliceCount = 0;

                FValid.Flush();

                return;
            }

            if (!empty)
            {
                if (FSelect[0] == SelectEnum.All) FValid.SliceCount = SpreadMax;
                else FValid.SliceCount = 1;

                foreach (string pinName in FPins.Keys)
                {
                    if (FSelect[0] == SelectEnum.All)
                    {
                        ToISpread(FPins[pinName]).SliceCount = SpreadMax;
                    }
                    else
                    {
                        ToISpread(FPins[pinName]).SliceCount = 1;
                    }
                }

                for (int i = (FSelect[0] == SelectEnum.Last) ? SpreadMax - 1 : 0; i < SpreadMax; i++)
                {
                    FValid[i] = false;
                    if (FInput[i] is PrimitiveObjectWrap)
                    {

                        FValid[i] = true;

                        PrimitiveObjectWrap wrapped = (PrimitiveObjectWrap)FInput[i];
                        PrimitiveObject CurrentObj = (PrimitiveObject)wrapped.Content;

                        foreach (string name in FPins.Keys)
                        {
                            var bin = (VVVV.PluginInterfaces.V2.NonGeneric.ISpread)ToISpread(FPins[name])[i];


                            ObjectTypePair Field = CurrentObj[name];
                            int count = 0;

                            if (Field == null)
                            {
                                //if (FVerbose[0]) FLogger.Log(LogType.Debug, "\"" + FTypes[name] + " " + name + "\" is not defined in Message.");
                            }
                            else count = Field.Objects.Count;

                            if ((count > 0) || (FHold[0] != HoldEnum.Pin))
                            {
                                bin.SliceCount = count;
                                for (int j = 0; j < count; j++)
                                {
                                    bin[j] = Field[j];
                                }
                                ToISpread(FPins[name]).Flush();
                            }
                            else
                            {
                                // keep old values in pin. do not flush
                            }
                        }
                    }
                    FValid.Flush();
                }
            }
        }
    }

    #region PluginInfo
    [PluginInfo(Name = "PrimitiveObject", AutoEvaluate = true, Category = "Set", Help = "Adds or edits fields of a Primitive Object", Tags = "Dynamic, Bin, velcrome")]
    #endregion PluginInfo
    public class SetMessageNode : DynamicNode
    {
        public enum NotExistingBehavior
        {
            Ignore,
            Create
        }

#pragma warning disable 649, 169
        [Input("Primitive Object")]
        IDiffSpread<VObject> FInput;
        [Input("Not-Existing Behavior", DefaultEnumEntry="Create")]
        IDiffSpread<NotExistingBehavior> FNotExist;
        [Input("Set", IsBang = true)]
        IDiffSpread<bool> FSet;

        [Output("Valid", AutoFlush = false)]
        ISpread<bool> FValid;
#pragma warning restore

        protected override IOAttribute DefinePin(string name, Type type)
        {
            var attr = new InputAttribute(name);
            attr.BinVisibility = PinVisibility.Hidden;
            attr.BinSize = -1;
            attr.Order = FCount;
            attr.BinOrder = FCount + 1;
            //                attr.AutoValidate = false;  // need to sync all pins manually
            return attr;
        }


        public override void Evaluate(int SpreadMax)
        {
            SpreadMax = FInput.SliceCount;

            if (!FInput.IsChanged)
            {
                return;
            }

            if (FInput.SliceCount == 0 || FInput[0] == null)
            {
                return;
            }

            for (int i = 0; i < SpreadMax; i++)
            {
                FValid[i] = false;
                if (FInput[i] is PrimitiveObjectWrap) FValid[i] = true;
                if ((FInput[i] is PrimitiveObjectWrap) && FSet[i])
                {
                    PrimitiveObjectWrap wrapped = (PrimitiveObjectWrap)FInput[i];
                    PrimitiveObject CurrentObj = (PrimitiveObject)wrapped.Content;

                    foreach (string name in FPins.Keys)
                    {
                        PluginInterfaces.V2.NonGeneric.ISpread pin = (PluginInterfaces.V2.NonGeneric.ISpread)ToISpread(FPins[name])[i];
                        if(CurrentObj.Fields.ContainsKey(name))
                        {
                            ObjectTypePair curr = CurrentObj[name];
                            curr.Objects.Clear();
                            for(int j=0; j<pin.SliceCount; j++)
                            {
                                curr.Objects.Add(pin[j]);
                            }
                        }
                        else if(FNotExist[i] == NotExistingBehavior.Create)
                        {
                            ObjectTypePair otp = new ObjectTypePair();

                            otp.Type = pin[0].GetType();
                            for (int j = 0; j < pin.SliceCount; j++)
                            {
                                otp.Objects.Add(pin[j]);
                            }
                            CurrentObj.Add(name, otp);
                        }
                    }
                }
                FValid.Flush();
            }
        }
    }
}
