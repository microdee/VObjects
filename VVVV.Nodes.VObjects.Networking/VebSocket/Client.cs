using System;
using System.Collections.Generic;
using System.Diagnostics;
using VVVV.Packs.VObjects;
using VVVV.PluginInterfaces.V2;

using WebSocketSharp;

namespace VVVV.Nodes.VObjects
{
    public static class VebSocketClientHelper
    {
        public static ClientMessage ToClientMessage(this WebSocketSharp.MessageEventArgs e)
        {
            ClientMessage m = new ClientMessage();
            m.Type = e.Type;
            m.GUID = Guid.NewGuid();
            if (e.Type == Opcode.Binary)
            {
                m.Raw = e.RawData;
                m.Text = "";
            }
            if (e.Type == Opcode.Text)
            {
                m.Raw = new byte[0];
                m.Text = e.Data;
            }
            return m;
        }
        public static VebSocketClient ToVebSocketClient(this WebSocket w)
        {
            return new VebSocketClient(w);
        }
    }
    public class ClientMessage
    {
        public Guid GUID = Guid.Empty;
        public WebSocketSharp.Opcode Type;
        public int Frames = 0;
        public Stopwatch Age = new Stopwatch();
        public byte[] Raw;
        public string Text;
        public ClientMessage()
        {
            this.Age.Start();
        }
    }
    public class VebSocketError
    {
        public int Frames = 0;
        public Stopwatch Age = new Stopwatch();
        public string Message = "";
        public System.Exception Exception;
        public VebSocketError()
        {
            this.Age.Start();
        }
    }
    public class ClientMessageWrap : VObject
    {
        public ClientMessageWrap() : base() { }
        public ClientMessageWrap(ClientMessage o) : base(o) { }
        
        public override void Dispose()
        {
 	        base.Dispose();
        }
    }

    public class VebSocketClient
    {
        public IHDEHost HDEHost;

        public string Url;
        public WebSocketSharp.WebSocket Client;
        public bool Connected = false;
        public bool Connecting = false;
        public bool AutoFlush = true;
        public bool AutoFlushLog = true;
        public int MaximumFramesForMessages = 1;
        public int MaximumAgeForErrors = 2;
        public int MaximumAgeForLog = 240;
        public double SendingMessageTimeout = 600;

        public Dictionary<Guid, ClientMessage> ReceivedMessages = new Dictionary<Guid, ClientMessage>();
        public Dictionary<Guid, ClientMessage> SendingMessages = new Dictionary<Guid, ClientMessage>();
        public Dictionary<Guid, ClientMessage> SentMessages = new Dictionary<Guid, ClientMessage>();
        public Dictionary<Stopwatch, string> LogMessages = new Dictionary<Stopwatch, string>();
        public List<VebSocketError> Errors = new List<VebSocketError>();
        public string CloseReason = "";

        private List<Guid> RemovableMessages = new List<Guid>();
        private List<int> RemovableErrors = new List<int>();
        private List<Stopwatch> RemovableLog = new List<Stopwatch>();

        public VebSocketClient(string url)
        {
            this.Url = url;
            this.Client = new WebSocketSharp.WebSocket(url);
            this.Client.OnOpen += onOpen;
            this.Client.OnMessage += onMessage;
            this.Client.OnError += onError;
            this.Client.OnClose += onClose;
            this.Client.OnAsyncMessageSent += onMessageSent;
            this.Client.Log.Output = this.OutputLog;
        }
        public VebSocketClient(WebSocketSharp.WebSocket w)
        {
            this.Client = w;
            this.Client.OnOpen += onOpen;
            this.Client.OnMessage += onMessage;
            this.Client.OnError += onError;
            this.Client.OnClose += onClose;
            this.Client.OnAsyncMessageSent += onMessageSent;
            this.Client.Log.Output = this.OutputLog;
            this.Url = w.Url.ToString();
        }
        public void SubscribeToMainloop(IHDEHost hdehost)
        {
            this.HDEHost = hdehost;
            this.HDEHost.MainLoop.OnPrepareGraph += this.ClearBuffers;
            this.HDEHost.MainLoop.OnUpdateView += this.IncrementAge;
        }
        public void Dispose()
        {
            this.HDEHost.MainLoop.OnPrepareGraph -= this.ClearBuffers;
            this.HDEHost.MainLoop.OnUpdateView -= this.IncrementAge;
        }

        private void IncrementAge(object sender, EventArgs e)
        {
            foreach (ClientMessage cm in this.ReceivedMessages.Values) cm.Frames++;
            foreach (ClientMessage cm in this.SendingMessages.Values) cm.Frames++;
            foreach (ClientMessage cm in this.SentMessages.Values) cm.Frames++;
            foreach (VebSocketError cm in this.Errors) cm.Frames++;
        }

        private void ClearBuffers(object sender, EventArgs e)
        {
            if(this.AutoFlush)
            {
                this.RemovableMessages.Clear();

                foreach (KeyValuePair<Guid, ClientMessage> kvp in this.ReceivedMessages)
                {
                    if (kvp.Value.Frames >= this.MaximumFramesForMessages)
                        this.RemovableMessages.Add(kvp.Key);
                }
                foreach (Guid g in this.RemovableMessages)
                    this.ReceivedMessages.Remove(g);
                this.RemovableMessages.Clear();

                foreach (KeyValuePair<Guid, ClientMessage> kvp in this.SendingMessages)
                {
                    if (kvp.Value.Age.Elapsed.TotalSeconds > this.SendingMessageTimeout)
                    {
                        this.RemovableMessages.Add(kvp.Key);
                        VebSocketError ve = new VebSocketError();
                        ve.Exception = null;
                        ve.Message = "Message " + kvp.Key.ToString() + " is expired.";
                    }
                }
                foreach (Guid g in this.RemovableMessages)
                    this.SendingMessages.Remove(g);
                this.RemovableMessages.Clear();

                foreach (KeyValuePair<Guid, ClientMessage> kvp in this.SentMessages)
                {
                    if (kvp.Value.Frames >= this.MaximumFramesForMessages)
                        this.RemovableMessages.Add(kvp.Key);
                }
                foreach (Guid g in this.RemovableMessages)
                    this.SentMessages.Remove(g);
                this.RemovableMessages.Clear();

                this.RemovableErrors.Clear();
                for(int i=0; i<this.Errors.Count; i++)
                {
                    if (this.Errors[i].Age.Elapsed.TotalSeconds > this.MaximumAgeForErrors)
                        this.RemovableErrors.Add(i);
                }
                foreach (int i in this.RemovableErrors)
                    this.Errors.RemoveAt(i);
                this.RemovableErrors.Clear();
            }

            if (this.AutoFlushLog)
            {
                this.RemovableMessages.Clear();
                foreach (Stopwatch sw in this.LogMessages.Keys)
                {
                    if (sw.Elapsed.TotalSeconds > this.MaximumAgeForLog)
                        this.RemovableLog.Add(sw);
                }
                foreach (Stopwatch sw in this.RemovableLog)
                    this.LogMessages.Remove(sw);
                this.RemovableMessages.Clear();
            }
        }
        
        private void onOpen(object sender, EventArgs e)
        {
            this.Connected = true;
            this.Connecting = false;
        }
        private void onMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            ClientMessage cm = e.ToClientMessage();
            this.ReceivedMessages.Add(cm.GUID, cm);
        }
        private void onError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            VebSocketError ve = new VebSocketError();
            ve.Exception = e.Exception;
            ve.Message = e.Message;
            this.Errors.Add(ve);
        }
        private void onMessageSent(object sender, SentMessageEventArgs e)
        {
            var cm = this.SendingMessages[e.GUID];
            cm.Frames = 0;
            cm.Age.Restart();
            if(e.Sent) this.SentMessages.Add(e.GUID, cm);
            else
            {
                VebSocketError ve = new VebSocketError();
                ve.Exception = null;
                ve.Message = "Message " + e.GUID.ToString() + " is not sent.";
            }
            this.SendingMessages.Remove(e.GUID);
        }

        private void onClose(object sender, WebSocketSharp.CloseEventArgs e)
        {
            this.CloseReason = e.Reason;
        }
        public ClientMessage Send(byte[] data, Action<bool> onSendComplete)
        {
            ClientMessage m = new ClientMessage();
            m.Type = Opcode.Binary;
            m.Raw = data;
            m.Text = "";
            m.GUID = this.Client.SendAsync(data, onSendComplete);
            this.SendingMessages.Add(m.GUID, m);
            return m;
        }
        public ClientMessage Send(string data, Action<bool> onSendComplete)
        {
            ClientMessage m = new ClientMessage();
            m.Type = Opcode.Text;
            m.Raw = new byte[0];
            m.Text = data;
            m.GUID = this.Client.SendAsync(data, onSendComplete);
            if(!this.SendingMessages.ContainsKey(m.GUID)) this.SendingMessages.Add(m.GUID, m);
            return m;
        }
        public ClientMessage Send(string data)
        {
            return this.Send(data, null);
        }
        public ClientMessage Send(byte[] data)
        {
            return this.Send(data, null);
        }

        public void OutputLog(LogData logdata, string file)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            this.LogMessages.Add(
                sw,
                logdata.Date.ToShortDateString() + " " +
                logdata.Date.ToShortTimeString() + ":" +
                logdata.Level.ToString() + "," +
                logdata.Message + "\n" +
                logdata.Caller.ToString()
            );
        }
    }
    public class VebSocketHostedClient : VebSocketClient
    {
        public VebSocketHostedClient(WebSocketSharp.WebSocket w) : base(w) { }
        public VebSocketHostedClient(string url) : base(url) { }
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
            hc.Dispose();
 	        base.Dispose();
        }
    }
}
