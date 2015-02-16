using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Packs.VObjects;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.IO;

namespace VVVV.Nodes.VObjects
{
    public class TypeWriterWrap : VObject
    {
        public TypeWriterWrap() : base() { }
        public TypeWriterWrap(TypeWriter o) : base(o) { }
        public TypeWriterWrap(Stream s) : base(s) { }

        public override void Serialize()
        {
            base.Serialize();

        }
        public override VObject DeepCopy()
        {
            return new TypeWriterWrap(new TypeWriter());
        }
        public override void Dispose()
        {
            TypeWriter tw = this.Content as TypeWriter;
            tw.Dispose();
            base.Dispose();
        }
    }

    [PluginInfo(Name = "TypeWriter", Category = "VObject")]
    public class TypeWriterWrapNode : ConstructVObjectNode<TypeWriterWrap>
    {
        //input pin declaration
        [Input("Keyboard", Order = 10)]
        public IDiffSpread<Keyboard> FKeyboardIn;

        [Input("Set Keyboard", Order = 11)]
        public ISpread<bool> FSetKeyboard;
        [Input("Disable Keyboard", Order = 12)]
        public ISpread<bool> FDisableKeyboard;

        [Input("Text", DefaultString = "", Order = 13)]
        public ISpread<string> FInputText;

        [Input("Insert Text", IsBang = true, Order = 14)]
        public ISpread<bool> FInsertText;

        [Input("Initial Text", Order = 15)]
        public ISpread<string> FInitialText;

        [Input("Initialize", IsBang = true, Order = 16)]
        public ISpread<bool> FInitialize;

        [Input("Max Length", MinValue = -1, MaxValue = int.MaxValue, DefaultValue = -1, Visibility = PinVisibility.True, Order = 17)]
        public ISpread<int> FMaxLength;

        [Input("Set Max Length", Order = 18)]
        public ISpread<bool> FSetMaxLength;

        [Input("Cursor Position", MinValue = 0, MaxValue = int.MaxValue, Visibility = PinVisibility.True, Order = 19)] //ASK ELIAS ABOUT Min and Default
        public ISpread<int> FNewCursorPosition;

        [Input("Set Cursor Position", IsBang = true, Visibility = PinVisibility.True, Order = 20)]
        public ISpread<bool> FSetCursorPosition;

        [Input("Ignore Navigation Keys", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector, Order = 21)]
        public ISpread<bool> FIgnoreNavigationKeys;

        [Input("Ignore new line", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector, Order = 22)]
        public ISpread<bool> FIgnoreNewLine;

        public override TypeWriterWrap ConstructVObject()
        {
            int i = this.CurrObj;
            TypeWriter tw = new TypeWriter();
            tw.CursorPosition = FNewCursorPosition[i];
            tw.IgnoreNavigationKeys = FIgnoreNavigationKeys[i];
            tw.IgnoreNewLine = FIgnoreNewLine[i];
            tw.Keyboard = FKeyboardIn[i];
            tw.MaxLength = FMaxLength[i];
            return new TypeWriterWrap(tw);
        }
        public override void InitializeFrame()
        {
            for (int i = 0; i < FOutput.SliceCount; i++)
            {
                if (FOutput[i] != null)
                {
                    TypeWriter tw = FOutput[i].Content as TypeWriter;
                    tw.IgnoreNavigationKeys = FIgnoreNavigationKeys[i];

                    if (FSetKeyboard[i])
                        tw.Keyboard = FKeyboardIn[i];
                    if (FDisableKeyboard[i])
                        tw.Keyboard = null;
                    if (FSetMaxLength[i])
                        tw.MaxLength = FMaxLength[i];
                    if (FSetCursorPosition[i])
                        tw.CursorPosition = FNewCursorPosition[i];
                    if (FInitialize[i])
                        tw.Initialize(FInitialText[i]);
                    if (FInsertText[i])
                        tw.InsertText(FInputText[i]);
                }
            }
        }
    }

    [PluginInfo(Name = "Restrict", Category = "VObject", Version = "Typewriter", AutoEvaluate = true)]
    public class RestrictTypeWriterWrapNode : IPluginEvaluate
    {
        [Input("TypeWriter")]
        public Pin<VObject> FTypeWriterWrap;

        [Input("White List", BinSize = 0)]
        public ISpread<ISpread<string>> FWhite;

        [Input("Black List", BinSize = 0)]
        public ISpread<ISpread<string>> FBlack;

        [Input("Set", IsBang = true)]
        public ISpread<bool> FSet;

        [Output("Valid")]
        public ISpread<bool> FValid;

        public void Evaluate(int SpreadMax)
        {
            if (FTypeWriterWrap.IsConnected)
            {
                FValid.SliceCount = FTypeWriterWrap.SliceCount;
                for (int i = 0; i < FTypeWriterWrap.SliceCount; i++)
                {
                    FValid[i] = FTypeWriterWrap[i] is TypeWriterWrap;
                    if (FValid[i])
                    {
                        if (FSet[i])
                        {
                            TypeWriter tw = FTypeWriterWrap[i].Content as TypeWriter;
                            tw.BlackList.Clear();
                            tw.WhiteList.Clear();
                            foreach (string s in FWhite[i])
                                tw.WhiteList.Add(s);
                            foreach (string s in FBlack[i])
                                tw.BlackList.Add(s);
                        }

                    }
                }
            }
            else FValid.SliceCount = 0;
        }
    }

    [PluginInfo(Name = "TypeWriter", Category = "VObject", Version = "Set", AutoEvaluate = true)]
    public class SetTypeWriterWrapNode : IPluginEvaluate
    {
        [Input("TypeWriter", Order = 9)]
        public Pin<VObject> FTypeWriterWrap;

        //input pin declaration
        [Input("Keyboard", Order = 10)]
        public IDiffSpread<Keyboard> FKeyboardIn;

        [Input("Set Keyboard", Order = 11)]
        public ISpread<bool> FSetKeyboard;
        [Input("Disable Keyboard", Order = 12)]
        public ISpread<bool> FDisableKeyboard;

        [Input("Text", DefaultString = "", Order = 13)]
        public ISpread<string> FInputText;

        [Input("Insert Text", IsBang = true, Order = 14)]
        public ISpread<bool> FInsertText;

        [Input("Initial Text", Order = 15)]
        public ISpread<string> FInitialText;

        [Input("Initialize", IsBang = true, Order = 16)]
        public ISpread<bool> FInitialize;

        [Input("Max Length", MinValue = -1, MaxValue = int.MaxValue, DefaultValue = -1, Visibility = PinVisibility.True, Order = 17)]
        public ISpread<int> FMaxLength;

        [Input("Set Max Length", Order = 18)]
        public ISpread<bool> FSetMaxLength;

        [Input("Cursor Position", MinValue = 0, MaxValue = int.MaxValue, Visibility = PinVisibility.True, Order = 19)] //ASK ELIAS ABOUT Min and Default
        public ISpread<int> FNewCursorPosition;

        [Input("Set Cursor Position", IsBang = true, Visibility = PinVisibility.True, Order = 20)]
        public ISpread<bool> FSetCursorPosition;

        [Output("Valid")]
        public ISpread<bool> FValid;

        public void Evaluate(int SpreadMax)
        {
            if (FTypeWriterWrap.IsConnected)
            {
                FValid.SliceCount = FTypeWriterWrap.SliceCount;
                for (int i = 0; i < FTypeWriterWrap.SliceCount; i++)
                {
                    FValid[i] = FTypeWriterWrap[i] is TypeWriterWrap;
                    if (FValid[i])
                    {
                        TypeWriter tw = FTypeWriterWrap[i].Content as TypeWriter;

                        if (FSetKeyboard[i])
                            tw.Keyboard = FKeyboardIn[i];
                        if (FDisableKeyboard[i])
                            tw.Keyboard = null;
                        if (FSetMaxLength[i])
                            tw.MaxLength = FMaxLength[i];
                        if (FSetCursorPosition[i])
                            tw.CursorPosition = FNewCursorPosition[i];
                        if (FInitialize[i])
                            tw.Initialize(FInitialText[i]);
                        if (FInsertText[i])
                            tw.InsertText(FInputText[i]);
                    }
                }
            }
            else FValid.SliceCount = 0;
        }
    }

    [PluginInfo(Name = "TypeWriter", Category = "VObject", Version = "Split")]
    public class SplitTypeWriterWrapNode : IPluginEvaluate
    {
        [Input("TypeWriter", Order = 9)]
        public Pin<VObject> FTypeWriterWrap;

        [Output("Output")]
        public ISpread<string> FOutput;

        [Output("Cursor Position")]
        public ISpread<int> FCursorPosition;

        [Output("Valid")]
        public ISpread<bool> FValid;

        public void Evaluate(int SpreadMax)
        {
            if (FTypeWriterWrap.IsConnected)
            {
                FValid.SliceCount = FTypeWriterWrap.SliceCount;
                FOutput.SliceCount = FTypeWriterWrap.SliceCount;
                FCursorPosition.SliceCount = FTypeWriterWrap.SliceCount;

                for (int i = 0; i < FTypeWriterWrap.SliceCount; i++)
                {
                    FValid[i] = FTypeWriterWrap[i] is TypeWriterWrap;
                    if (FValid[i])
                    {
                        TypeWriter tw = FTypeWriterWrap[i].Content as TypeWriter;
                        FOutput[i] = tw.Output;
                        FCursorPosition[i] = tw.CursorPosition;
                    }
                }
            }
            else
            {
                FValid.SliceCount = 0;
                FOutput.SliceCount = 0;
                FCursorPosition.SliceCount = 0;
            }
        }
    }
}
