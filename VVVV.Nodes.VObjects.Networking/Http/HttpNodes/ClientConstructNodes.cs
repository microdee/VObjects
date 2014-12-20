using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

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
    public class HttpClientConstructor : ConstructVObjectNode<HttpClientWrap>
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

        public override HttpClientWrap ConstructVObject()
        {
            try
            {
                if (FTrustAll[this.CurrObj])
                {
                    ServicePointManager.ServerCertificateValidationCallback +=
                        (sender, cert, chain, sslPolicyErrors) => true;
                }
                HttpClientContainer hcc = new HttpClientContainer();
                HttpClientWrap hcw = new HttpClientWrap(hcc);
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
                return hcw;
            }
            catch(Exception e)
            {
                FError[0] = e.Message + e.InnerException.Message;
                return null;
            }
        }
    }
}
