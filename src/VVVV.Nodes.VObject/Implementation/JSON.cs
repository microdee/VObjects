// Credits: Sanch's original JObject implementation: 

using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using Newtonsoft.Json.Linq;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(Name = "JsonParser", Category = "JSON", Help = "parse json string", Tags = "")]
    public class JsonParser : IPluginEvaluate
    {
        #region fields & pins
        [Input("JSON", DefaultString = "hello")]
        public IDiffSpread<string> FInput;

        [Output("Output JToken")]
        public ISpread<JToken> FJOutput;

        [Output("Valid")]
        public ISpread<bool> FValid;

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
                        FJOutput.Add(JToken.Parse(FInput[i]));
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
        [Input("Input JToken")]
        public Pin<object> FInput;

        [Input("path")]
        public ISpread<ISpread<string>> FInputp;

        [Input("Parse", DefaultBoolean = true)]
        public ISpread<bool> FParse;

        [Output("Output")]
        public ISpread<ISpread<string>> FOutput;

        [Output("Output JToken")]
        public ISpread<ISpread<JToken>> FObjectOut;

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
                        if (FInput[i] is JToken)
                        {
                            JToken ThisContent = FInput[i] as JToken;
                            FOutput[i].SliceCount = FInputp[i].SliceCount;
                            FObjectOut[i].SliceCount = 0;
                            for (int j = 0; j < FInputp[i].SliceCount; j++)
                            {
                                if (ThisContent.SelectToken(FInputp[i][j]) != null && ThisContent != null)
                                {
                                    JToken st = ThisContent.SelectToken(FInputp[i][j]);
                                    FOutput[i][j] = st.ToString();
                                    FObjectOut[i].Add(st);
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
    }

    [PluginInfo(Name = "JsonArray", Category = "JSON", Help = "list json array content", Tags = "")]
    public class JsonArray : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input JToken")]
        public Pin<object> FInput;

        [Input("path")]
        public IDiffSpread<string> FInputp;

        [Input("key")]
        public IDiffSpread<string> FInputk;

        [Input("Parse", DefaultBoolean = true)]
        public ISpread<bool> FParse;

        [Output("Output")]
        public ISpread<ISpread<string>> FOutput;

        [Output("Output JToken")]
        public ISpread<ISpread<JToken>> FObjectOut;

        // [Output("Output json")]
        //ISpread<Vson> FJOutput;

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
                        if (FInput[i] is JToken)
                        {
                            JToken ThisContent = FInput[i] as JToken;
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
                                        FObjectOut[i].Add(st);
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
        [Input("Input JToken")]
        public Pin<object> FInput;
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
                        if (FInput[i] is JToken)
                        {
                            JToken ThisContent = FInput[i] as JToken;
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
}
