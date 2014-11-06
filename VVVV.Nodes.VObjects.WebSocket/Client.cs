using System;
using System.ComponentModel.Composition;
using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using System.Collections.Generic;
using VVVV.Packs.VObjects;

using WebSocketSharp;

namespace VVVV.Nodes.VObjects
{
    public class HostedClientMessage
    {
        public WebSocketSharp.Opcode Type;
        public byte[] Raw;
        public string Text;
        public HostedClientMessage() { }
    }
    public class wsHostedClient
    {
        public string Url;
        public WebSocketSharp.WebSocket Client;
        public bool Connected = false;
        public bool Connecting = false;
        public List<HostedClientMessage> ReceivedMessages = new List<HostedClientMessage>();
        public List<string> ReceivedErrors = new List<string>();
        public string CloseReason = "";

        public wsHostedClient(string url)
        {
            this.Url = url;
            this.Client = new WebSocketSharp.WebSocket(url);
            this.Client.OnOpen += onOpen;
            this.Client.OnMessage += onMessage;
            this.Client.OnError += onError;
            this.Client.OnClose += onClose;
        }
        
        private void onOpen(object sender, EventArgs e)
        {
            this.Connected = true;
            this.Connecting = false;
        }
        private void onMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            HostedClientMessage m = new HostedClientMessage();
            m.Type = e.Type;
            if(e.Type == Opcode.Binary)
            {
                m.Raw = e.RawData;
                m.Text = "";
            }
            if (e.Type == Opcode.Text)
            {
                m.Raw = new byte[0];
                m.Text = e.Data;
            }
            this.ReceivedMessages.Add(m);
        }
        private void onError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            this.ReceivedErrors.Add(e.Message);
        }
        private void onClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            this.CloseReason = e.Reason;
        }
        public void Connect()
        {
            this.Client.ConnectAsync();
            this.Connecting = true;
        }
    }
    public class HostedClientWrap : VObject
    {
        public HostedClientWrap() : base() { }
        public HostedClientWrap(wsHostedClient o) : base(o) { }
        
        public override void Dispose()
        {
            wsHostedClient hc = this.Content as wsHostedClient;
            hc.Client.Close();
 	        base.Dispose();
        }
    }
}
