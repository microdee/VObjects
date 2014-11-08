using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Collections.Generic;
using VVVV.Packs.VObjects;

using WebSocketSharp;

namespace VVVV.Nodes.VObjects
{
    public class ClientMessage
    {
        public WebSocketSharp.Opcode Type;
        public byte[] Raw;
        public string Text;
        public ClientMessage() { }
    }
    public class VebSocketClient
    {
        public string Url;
        public WebSocketSharp.WebSocket Client;
        public bool Connected = false;
        public bool Connecting = false;
        /*
        public List<ClientMessage> ReceivedMessages = new List<ClientMessage>();
        public List<ClientMessage> SendingMessages = new List<ClientMessage>();
        public List<ClientMessage> SentMessages = new List<ClientMessage>();
        public List<string> ReceivedErrors = new List<string>();
         */
        public string CloseReason = "";

        public VebSocketClient(string url)
        {
            this.Url = url;
            this.Client = new WebSocketSharp.WebSocket(url);
            this.Client.OnOpen += onOpen;
            //this.Client.OnMessage += onMessage;
            //this.Client.OnError += onError;
            this.Client.OnClose += onClose;
        }
        public VebSocketClient(WebSocketSharp.WebSocket w)
        {
            this.Client = w;
            this.Client.OnOpen += onOpen;
            //this.Client.OnMessage += onMessage;
            //this.Client.OnError += onError;
            this.Client.OnClose += onClose;
            this.Url = w.Url.ToString();
        }
        
        private void onOpen(object sender, EventArgs e)
        {
            this.Connected = true;
            this.Connecting = false;
        }
        /*
        private void onMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            ClientMessage m = new ClientMessage();
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
         */
        private void onClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            this.CloseReason = e.Reason;
        }
        /*
        public void Send(byte[] data)
        {
            ClientMessage m = new ClientMessage();
            m.Type = Opcode.Binary;
            m.Raw = data;
            m.Text = "";
            this.SendingMessages.Add(m);

            this.Client.SendAsync(data, onSendComplete);
        }
        private void onSendComplete(bool sent)
        {
            if(!sent)
            {
                string error = "message not sent:" + this.SendingMessages[0].GetHashCode().ToString();
                this.ReceivedErrors.Add(error);
            }
            else
            {
                this.SentMessages.Add(this.SendingMessages[0]);
            }
            this.SendingMessages.RemoveAt(0);
        }
         */
    }
    public class VebSocketHostedClient : VebSocketClient
    {
        public void Connect()
        {
            this.Client.ConnectAsync();
            this.Connecting = true;
        }
    }
    public class VebSocketClientWrap : VObject
    {
        public VebSocketClientWrap() : base() { }
        public VebSocketClientWrap(VebSocketClient o) : base(o) { }
        
        public override void Dispose()
        {
            VebSocketClient hc = this.Content as VebSocketClient;
            hc.Client.Close();
 	        base.Dispose();
        }
    }
}
