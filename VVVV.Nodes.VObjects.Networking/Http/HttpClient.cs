using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    public enum SendMethod
    {
        Delete,
        Get,
        Head,
        Options,
        Post,
        Put,
        Trace
    }
    public class HttpClientContainer
    {
        public HttpClient Client = new HttpClient();
        public List<Task<HttpResponseMessage>> OngoingRequests = new List<Task<HttpResponseMessage>>();

        public HttpClientContainer() { }

        public Task<HttpResponseMessage> Send(HttpRequestMessage hrm, HttpCompletionOption hco)
        {
            Task<HttpResponseMessage> returntask = this.Client.SendAsync(hrm, hco);
            this.OngoingRequests.Add(returntask);
            return returntask;
        }

        public void Dispose()
        {
            this.OngoingRequests.Clear();
            this.Client.CancelPendingRequests();
            this.Client.Dispose();
        }
    }

    public class HttpClientWrap : VObject
    {
        public HttpClientWrap() : base() { }
        public HttpClientWrap(HttpClientContainer o) : base(o) { }

        public override void Dispose()
        {
            HttpClientContainer hc = this.Content as HttpClientContainer;
            hc.Dispose();
            base.Dispose();
        }
    }
}
