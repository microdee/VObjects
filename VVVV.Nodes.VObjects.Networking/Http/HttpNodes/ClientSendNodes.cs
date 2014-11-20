using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects.Http.HttpNodes
{
    [PluginInfo(
        Name = "Send",
        Category = "Http",
        Help = "Send an HTTP message",
        AutoEvaluate = true,
        Tags = "microdee"
    )]
    public class HttpClientSendNode : IPluginEvaluate
    {
        [Input("Client")]
        public Pin<HttpClientWrap> FClient;

        [Input("Path")]
        public ISpread<ISpread<string>> FPath;
        [Input("Content")]
        public ISpread<ISpread<string>> FContentIn;
        [Input("Media Type")]
        public ISpread<ISpread<string>> FMediaType;
        [Input("Method", DefaultEnumEntry = "Get")]
        public ISpread<ISpread<SendMethod>> FMethod;

        [Input("Completion On", DefaultEnumEntry = "ResponseContentRead")]
        public ISpread<HttpCompletionOption> FCompletion;
        [Input("Send", IsBang = true)]
        public ISpread<ISpread<bool>> FSend;
        [Input("Long Polling")]
        public ISpread<bool> FLongPoll;

        [Output("Task")]
        public ISpread<ISpread<Task<HttpResponseMessage>>> FTask;
        [Output("Headers")]
        public ISpread<ISpread<HttpResponseHeaders>> FHeaders;
        [Output("Content")]
        public ISpread<ISpread<Stream>> FContent;
        [Output("Status Code")]
        public ISpread<ISpread<string>> FStatus;
        [Output("Reason")]
        public ISpread<ISpread<string>> FReason;
        [Output("Sending")]
        public ISpread<ISpread<bool>> FSending;
        [Output("Completed", IsBang = true)]
        public ISpread<ISpread<bool>> FCompleted;

        protected void Send(int i, int j, HttpClientContainer hcc)
        {
            FSending[i][j] = true;
            HttpMethod hm = new HttpMethod(FMethod[i][j].ToString());
            HttpRequestMessage hrm = new HttpRequestMessage(hm, FPath[i][j]);
            StringContent sc = new StringContent(FContentIn[i][j], Encoding.UTF8, FMediaType[i][j]);
            hrm.Content = sc;
            Task<HttpResponseMessage> requesttask = hcc.Send(hrm, FCompletion[i]);
            FTask[i][j] = requesttask;
        }

        public void Evaluate(int SpreadMax)
        {
            if (FClient.IsConnected)
            {
                FTask.SliceCount = FClient.SliceCount;
                FHeaders.SliceCount = FClient.SliceCount;
                FContent.SliceCount = FClient.SliceCount;
                FStatus.SliceCount = FClient.SliceCount;
                FReason.SliceCount = FClient.SliceCount;
                FSending.SliceCount = FClient.SliceCount;
                FCompleted.SliceCount = FClient.SliceCount;
                for (int i = 0; i < FClient.SliceCount; i++)
                {
                    int cSpreadMax = 0;
                    cSpreadMax = Math.Max(cSpreadMax, FPath[i].SliceCount);
                    cSpreadMax = Math.Max(cSpreadMax, FContentIn[i].SliceCount);
                    cSpreadMax = Math.Max(cSpreadMax, FMethod[i].SliceCount);
                    cSpreadMax = Math.Max(cSpreadMax, FMediaType[i].SliceCount);

                    FTask[i].SliceCount = cSpreadMax;
                    FHeaders[i].SliceCount = cSpreadMax;
                    FContent[i].SliceCount = cSpreadMax;
                    FStatus[i].SliceCount = cSpreadMax;
                    FReason[i].SliceCount = cSpreadMax;
                    FSending[i].SliceCount = cSpreadMax;
                    FCompleted[i].SliceCount = cSpreadMax;

                    HttpClientContainer hcc = FClient[i].Content as HttpClientContainer;
                    for (int j = 0; j < cSpreadMax; j++)
                    {
                        FCompleted[i][j] = false;

                        if(FSend[i][j])
                        {
                            this.Send(i, i, hcc);
                        }

                        if(FTask[i][j] != null)
                        {
                            if(FTask[i][j].IsCompleted)
                            {
                                Stream tmp = new MemoryStream();
                                if (FSending[i][j]) FCompleted[i][j] = true;
                                FHeaders[i][j] = FTask[i][j].Result.Headers;
                                FContent[i][j] = FTask[i][j].Result.Content.ReadAsStreamAsync().Result;
                                FStatus[i][j] = FTask[i][j].Result.StatusCode.ToString();
                                FReason[i][j] = FTask[i][j].Result.ReasonPhrase;
                                FSending[i][j] = false;

                                if(FLongPoll[i])
                                {
                                    this.Send(i, i, hcc);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
