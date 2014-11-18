// Credits: Sanch's original JObject implementation: 

using System;
using System.ComponentModel.Composition;
using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using VVVV.Core.Logging;
using System.Collections.Generic;
using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    public class JObjectWrap : VObject
    {
        public JObjectWrap() : base() { }
        public JObjectWrap(JObject o) : base(o) { }
        public JObjectWrap(Stream s) : base(s) { }

        public override void Serialize()
        {
            base.Serialize();

        }
        public override VObject DeepCopy()
        {
            JObject ThisContent = (JObject)this.Content;
            JObject NewObj = JObject.Parse(ThisContent.ToString());
            JObjectWrap NewWrap = new JObjectWrap(NewObj);
            return NewWrap;
        }
    }

    [PluginInfo(Name = "Cast", Category = "To", Version = "JSON")]
    public class ToJSONCastNode : CastToNode<JObjectWrap> { }

    [PluginInfo(Name = "JsonParser", Category = "JSON", Help = "parse json string", Tags = "")]
    public class JsonParser : IPluginEvaluate
    {
        #region fields & pins
        [Input("JSON", DefaultString = "hello")]
        public IDiffSpread<string> FInput;

        [Output("Output json")]
        public ISpread<JObjectWrap> FJOutput;

        [Output("Valid")]
        public ISpread<bool> FValid;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins
        //called when data for any output pin is requested

        public void Evaluate(int SpreadMax)
        {
            FValid.SliceCount = FInput.SliceCount;
            if (FInput.IsChanged)
            {
                FJOutput.SliceCount = 0;
                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (JObject.Parse(FInput[i]) != null)
                    {
                        FJOutput.Add(new JObjectWrap(JObject.Parse(FInput[i])));
                        FValid[i] = true;
                    }
                    else FValid[i] = false;
                }
            }
        }
    }

    [PluginInfo(Name = "SelectToken", Category = "JSON", Help = "select json token", Tags = "")]
    public class SelectToken : IPluginEvaluate
    {
        #region fields & pins
        [Input("JObject")]
        public Pin<JObjectWrap> FInput;

        [Input("path")]
        public ISpread<ISpread<string>> FInputp;

        [Input("Parse", DefaultBoolean = true)]
        public ISpread<bool> FParse;

        [Output("Output")]
        public ISpread<ISpread<string>> FOutput;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins
        //called when data for any output pin is requested



        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = FInput.SliceCount;
            if (FInput.IsConnected)
            {
                if (FParse[0])
                {
                    for (int i = 0; i < FInput.SliceCount; i++)
                    {
                        JObject ThisContent = FInput[i].Content as JObject;
                        FOutput[i].SliceCount = FInputp[i].SliceCount;
                        for (int j = 0; j < FInputp[i].SliceCount; j++)
                        {
                            if (ThisContent.SelectToken(FInputp[i][j]) != null && ThisContent != null)
                            {
                                FOutput[i][j] = ThisContent.SelectToken(FInputp[i][j]).ToString();
                            }
                            else
                            {
                                FOutput[i][j] = "";
                            }
                        }
                    }
                }
            }
        }
    }

    [PluginInfo(Name = "JsonArray", Category = "JSON", Help = "list json array content", Tags = "")]
    public class JsonArray : IPluginEvaluate
    {
        #region fields & pins
        [Input("Jobject")]
        public Pin<JObjectWrap> FInput;

        [Input("path")]
        public IDiffSpread<string> FInputp;

        [Input("key")]
        public IDiffSpread<string> FInputk;

        [Input("Parse", DefaultBoolean = true)]
        public ISpread<bool> FParse;

        [Output("Output")]
        public ISpread<ISpread<string>> FOutput;

        // [Output("Output json")]
        //ISpread<Vson> FJOutput;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = SpreadMax;
            if (FInput.IsConnected)
            {
                if (FParse[0])
                {
                    //if(FInputp.IsChanged || FInputk.IsChanged)
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        JObject ThisContent = FInput[i].Content as JObject;
                        FOutput[i].SliceCount = 0;
                        if (ThisContent.SelectToken(FInputp[i]) != null)
                        {
                            var results = ThisContent.SelectToken(FInputp[i]);
                            foreach (JToken child in results.Children())
                            {
                                if (child.SelectToken(FInputk[0]) != null)
                                {
                                    FOutput[i].Add(child.SelectToken(FInputk[0]).ToString());
                                }
                                else
                                {
                                    FOutput[i].Add("");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
