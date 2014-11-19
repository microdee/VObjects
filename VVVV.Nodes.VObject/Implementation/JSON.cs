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
        public JObjectWrap(JToken o) : base(o) { }
        public JObjectWrap(Stream s) : base(s) { }

        public override void Serialize()
        {
            base.Serialize();

        }
        public override VObject DeepCopy()
        {
            JToken ThisContent = (JToken)this.Content;
            JToken NewObj = JToken.Parse(ThisContent.ToString());
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
                    if (JToken.Parse(FInput[i]) != null)
                    {
                        FJOutput.Add(new JObjectWrap(JToken.Parse(FInput[i])));
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

        [Output("Output Object")]
        public ISpread<ISpread<JObjectWrap>> FObjectOut;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins
        //called when data for any output pin is requested



        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = FInput.SliceCount;
            FObjectOut.SliceCount = FInput.SliceCount;
            if (FInput.IsConnected)
            {
                if (FParse[0])
                {
                    for (int i = 0; i < FInput.SliceCount; i++)
                    {
                        JToken ThisContent = FInput[i].Content as JToken;
                        FOutput[i].SliceCount = FInputp[i].SliceCount;
                        FObjectOut[i].SliceCount = 0;
                        for (int j = 0; j < FInputp[i].SliceCount; j++)
                        {
                            if (ThisContent.SelectToken(FInputp[i][j]) != null && ThisContent != null)
                            {
                                JToken st = ThisContent.SelectToken(FInputp[i][j]);
                                FOutput[i][j] = st.ToString();
                                JObjectWrap jow = new JObjectWrap(st);
                                FObjectOut[i].Add(jow);
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

        [Output("Output Object")]
        public ISpread<ISpread<JObjectWrap>> FObjectOut;

        // [Output("Output json")]
        //ISpread<Vson> FJOutput;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins
        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = SpreadMax;
            FObjectOut.SliceCount = SpreadMax;
            if (FInput.IsConnected)
            {
                if (FParse[0])
                {
                    //if(FInputp.IsChanged || FInputk.IsChanged)
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        JObject ThisContent = FInput[i].Content as JObject;
                        FOutput[i].SliceCount = 0;
                        FObjectOut[i].SliceCount = 0;
                        if (ThisContent.SelectToken(FInputp[i]) != null)
                        {
                            var results = ThisContent.SelectToken(FInputp[i]);
                            foreach (JToken child in results.Children())
                            {
                                if (child.SelectToken(FInputk[0]) != null)
                                {
                                    JToken st = child.SelectToken(FInputk[0]);
                                    FOutput[i].Add(st.ToString());
                                    JObjectWrap jow = new JObjectWrap(st);
                                    FObjectOut[i].Add(jow);
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
    public enum JsonChildrenListingFilterMode
    {
        All,
        Exclude,
        Include
    }

    [PluginInfo(Name = "ListChildren", Category = "JSON", Tags = "")]
    public class ListTokens : IPluginEvaluate
    {
        #region fields & pins
        [Input("JObject")]
        public Pin<JObjectWrap> FInput;
        [Input("Path")]
        public IDiffSpread<string> FInputp;
        [Input("Parse", DefaultBoolean = true)]
        public ISpread<bool> FParse;
        [Input("Recursive")]
        public ISpread<bool> FRecursive;
        [Input("Filtering Paths")]
        public IDiffSpread<string> FFilterPath;
        [Input("Filter Mode")]
        public ISpread<JsonChildrenListingFilterMode> FFilterMode;

        [Output("Output")]
        public ISpread<ISpread<string>> FOutput;
        [Output("Type")]
        public ISpread<ISpread<string>> FType;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins
        //called when data for any output pin is requested

        public int CurrObj;

        public void WalkNode(JToken node)
        {
            if (node.Type == JTokenType.Object)
            {
                foreach (JProperty child in node.Children<JProperty>())
                {
                    if (FilterPath(child.Name))
                    {
                        string path = child.Path;
                        FOutput[CurrObj].Add(path);
                        FType[CurrObj].Add(child.Type.ToString());
                        WalkNode(child.Value);
                    }
                }
            }
            else if (node.Type == JTokenType.Array)
            {
                foreach (JToken child in node.Children())
                {
                    string path = child.Path;
                    FOutput[CurrObj].Add(path);
                    FType[CurrObj].Add(child.Type.ToString());
                    WalkNode(child);
                }
            }
        }

        public bool FilterPath(string s)
        {
            bool ret = true;
            if(FFilterMode[CurrObj] == JsonChildrenListingFilterMode.Exclude)
            {
                foreach (string ss in FFilterPath)
                    if (s == ss) ret = false;
            }
            if (FFilterMode[CurrObj] == JsonChildrenListingFilterMode.Include)
            {
                foreach (string ss in FFilterPath)
                    if (s != ss) ret = false;
            }
            return ret;
        }

        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = FInput.SliceCount;
            FType.SliceCount = FInput.SliceCount;
            if (FInput.IsConnected)
            {
                if (FParse[0])
                {
                    for (int i = 0; i < FInput.SliceCount; i++)
                    {
                        JObject ThisContent = FInput[i].Content as JObject;
                        JToken ThisPath = ThisContent.SelectToken(FInputp[i]);
                        FOutput[i].SliceCount = 0;
                        FType[i].SliceCount = 0;
                        if (ThisPath != null)
                        {
                            CurrObj = i;
                            if (FRecursive[i]) WalkNode(ThisPath);
                            else
                            {
                                foreach (JToken jt in ThisPath.Children())
                                {
                                    FOutput[i].Add(jt.Path);
                                    FType[i].Add(jt.Type.ToString());
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
