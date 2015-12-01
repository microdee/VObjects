using System;
using System.Net;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(
        Name = "Client",
        Category = "Http",
        Help = "Construct a http client",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class HttpClientConstructor : ConstructObjectNode
    {
        [Input("Base Url", DefaultString = "http://localhost")]
        public ISpread<string> FUrl;

        [Input("Additional Default Header Name")]
        public IDiffSpread<ISpread<string>> FHeaderName;
        [Input("Additional Default Header Values")]
        public IDiffSpread<ISpread<string>> FHeaderValues;
        [Input("Additional Default Headers")]
        public ISpread<bool> FHeaders;
        [Input("Allow Untrusted")]
        public ISpread<bool> FTrustAll;

        [Input("Time Out", DefaultValue = 86400)]
        public ISpread<double> FTimeOut;
        [Output("Error")]
        public ISpread<string> FError;

        public override object ConstructObject()
        {
            try
            {
                if (FTrustAll[this.CurrObj])
                {
                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                }
                HttpClientContainer hcc = new HttpClientContainer();
                hcc.Client.BaseAddress = new Uri(FUrl[this.CurrObj]);
                double milliseconds = FTimeOut[this.CurrObj] * 1000;
                hcc.Client.Timeout = new TimeSpan(0, 0, 0, 0, (int)milliseconds);

                int cSpreadMax = 0;
                cSpreadMax = Math.Max(cSpreadMax, FHeaderName[this.CurrObj].SliceCount);
                cSpreadMax = Math.Max(cSpreadMax, FHeaderValues[this.CurrObj].SliceCount);

                if (FHeaders[this.CurrObj])
                {
                    for (int i = 0; i < cSpreadMax; i++)
                        hcc.Client.DefaultRequestHeaders.Add(FHeaderName[this.CurrObj][i], FHeaderValues[this.CurrObj][i]);
                }
                return hcc;
            }
            catch(Exception e)
            {
                FError[0] = e.Message + e.InnerException.Message;
                return null;
            }
        }
    }
}
