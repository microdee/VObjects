using System.Collections.Generic;
using System.Net.Http.Headers;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(
        Name = "Headers",
        Category = "Http",
        Version = "Split",
        Help = "Split Http headers",
        Tags = "microdee"
    )]
    public class HttpHeadersSplitNode : IPluginEvaluate
    {
        [Input("Output")]
        public Pin<HttpHeaders> FIn;

        [Output("Name")]
        public ISpread<ISpread<string>> FName;
        [Output("Values")]
        public ISpread<ISpread<string>> FValues;

        public void Evaluate(int SpreadMax)
        {
            if (FIn.IsConnected)
            {
                FName.SliceCount = FIn.SliceCount;
                FValues.SliceCount = FIn.SliceCount;

                for (int i = 0; i < FIn.SliceCount; i++)
                {
                    FName[i].SliceCount = 0;
                    FValues[i].SliceCount = 0;

                    if(FIn[i] != null)
                    {
                        foreach(KeyValuePair<string, IEnumerable<string>> kvp in FIn[i])
                        {
                            FName[i].Add(kvp.Key);
                            string values = "";
                            foreach(string s in kvp.Value) values += s;
                            FValues[i].Add(values);
                        }
                    }
                }
            }
        }
    }
}
