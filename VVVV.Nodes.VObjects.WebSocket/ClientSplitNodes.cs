using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Packs.VObjects;

using WebSocketSharp;
using WebSocketSharp.Net;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(
        Name = "ReceivedMessages",
        Category = "VebSocket",
        Version = "Client Simple",
        Help = "Get Messages from a hosted or served client",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketClientReceivedMessagesNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VebSocketClientWrap> FInput;

        [Output("Text Message")]
        public ISpread<ISpread<string>> FTextMessage;
        [Output("Raw Message")]
        public ISpread<ISpread<Stream>> FRawMessage;
        [Output("Message Type")]
        public ISpread<ISpread<string>> FMessageType;
        [Output("Error")]
        public ISpread<ISpread<string>> FError;

        public Dictionary<WebSocketSharp.WebSocket, ReceivingBuffer> Receiving = new Dictionary<WebSocketSharp.WebSocket, ReceivingBuffer>();
        public List<WebSocketSharp.WebSocket> ToBeRemoved = new List<WebSocketSharp.WebSocket>();
        
        public void AddNewClient()
        {
            foreach(VebSocketClientWrap vsw in FInput)
            {
                VebSocketClient vs = vsw.Content as VebSocketClient;
                if(!Receiving.ContainsKey(vs.Client))
                {
                    ReceivingBuffer rb = new ReceivingBuffer();
                    rb.Parent = vs;
                    vs.Client.OnMessage += onMessage;
                    vs.Client.OnError += onError;
                    Receiving.Add(vs.Client, rb);
                }
            }
        }
        public void RemoveExpiredClient()
        {
            ToBeRemoved.Clear();
            foreach (VebSocketClientWrap vsw in FInput)
            {
                VebSocketClient vs = vsw.Content as VebSocketClient;
                if (Receiving.ContainsKey(vs.Client))
                    Receiving[vs.Client].PresentInCurrentContext = true;
            }
            foreach (KeyValuePair<WebSocketSharp.WebSocket, ReceivingBuffer> kvp in Receiving)
            {
                if(!kvp.Value.PresentInCurrentContext)
                    ToBeRemoved.Add(kvp.Key);
            }
            foreach (WebSocketSharp.WebSocket vs in ToBeRemoved)
                Receiving.Remove(vs);
        }
        public void ClearBuffers()
        {
            foreach(KeyValuePair<WebSocketSharp.WebSocket, ReceivingBuffer> kvp in Receiving)
            {
                kvp.Value.Errors.Clear();
                kvp.Value.Messages.Clear();
            }
        }


        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FTextMessage.SliceCount = FInput.SliceCount;
                FRawMessage.SliceCount = FInput.SliceCount;
                FMessageType.SliceCount = FInput.SliceCount;
                FError.SliceCount = FInput.SliceCount;

                RemoveExpiredClient();
                AddNewClient();

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    FTextMessage[i].SliceCount = 0;
                    FRawMessage[i].SliceCount = 0;
                    FMessageType[i].SliceCount = 0;
                    FError[i].SliceCount = 0;
                    VebSocketClient vs = FInput[i].Content as VebSocketClient;

                    foreach (ClientMessage cm in Receiving[vs.Client].Messages)
                    {
                        FTextMessage[i].Add(cm.Text);
                        FRawMessage[i].Add(cm.Raw.ToStream());
                        FMessageType[i].Add(cm.Type.ToString());
                    }
                    foreach (string e in Receiving[vs.Client].Errors)
                    {
                        FError[i].Add(e);
                    }
                }
            }
            else
            {
                FTextMessage.SliceCount = 0;
                FRawMessage.SliceCount = 0;
                FMessageType.SliceCount = 0;
                FError.SliceCount = 0;
            }
            ClearBuffers();
        }
        private void onMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            WebSocketSharp.WebSocket Sender = sender as WebSocketSharp.WebSocket;
            this.Receiving[Sender].Messages.Add(e.ToClientMessage());
        }
        private void onError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            WebSocketSharp.WebSocket Sender = sender as WebSocketSharp.WebSocket;
            this.Receiving[Sender].Errors.Add(e.Message);
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "ReceivedMessages",
        Category = "VebSocket",
        Version = "Client",
        Help = "Get Messages from a hosted or served client",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketClientReceivedMessagesAdvancedNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VebSocketClientWrap> FInput;

        [Output("Message")]
        public ISpread<ISpread<ClientMessageWrap>> FMessage;
        [Output("Error")]
        public ISpread<ISpread<string>> FError;

        public Dictionary<WebSocketSharp.WebSocket, ReceivingBuffer> Receiving = new Dictionary<WebSocketSharp.WebSocket, ReceivingBuffer>();
        public List<WebSocketSharp.WebSocket> ToBeRemoved = new List<WebSocketSharp.WebSocket>();

        public void AddNewClient()
        {
            foreach (VebSocketClientWrap vsw in FInput)
            {
                VebSocketClient vs = vsw.Content as VebSocketClient;
                if (!Receiving.ContainsKey(vs.Client))
                {
                    ReceivingBuffer rb = new ReceivingBuffer();
                    rb.Parent = vs;
                    vs.Client.OnMessage += onMessage;
                    vs.Client.OnError += onError;
                    Receiving.Add(vs.Client, rb);
                }
            }
        }
        public void RemoveExpiredClient()
        {
            ToBeRemoved.Clear();
            foreach (VebSocketClientWrap vsw in FInput)
            {
                VebSocketClient vs = vsw.Content as VebSocketClient;
                if (Receiving.ContainsKey(vs.Client))
                    Receiving[vs.Client].PresentInCurrentContext = true;
            }
            foreach (KeyValuePair<WebSocketSharp.WebSocket, ReceivingBuffer> kvp in Receiving)
            {
                if (!kvp.Value.PresentInCurrentContext)
                    ToBeRemoved.Add(kvp.Key);
            }
            foreach (WebSocketSharp.WebSocket vs in ToBeRemoved)
                Receiving.Remove(vs);
        }
        public void ClearBuffers()
        {
            foreach (KeyValuePair<WebSocketSharp.WebSocket, ReceivingBuffer> kvp in Receiving)
            {
                kvp.Value.Errors.Clear();
                kvp.Value.Messages.Clear();
            }
        }


        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FMessage.SliceCount = FInput.SliceCount;
                FError.SliceCount = FInput.SliceCount;

                RemoveExpiredClient();
                AddNewClient();

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    FMessage[i].SliceCount = 0;
                    FError[i].SliceCount = 0;
                    VebSocketClient vs = FInput[i].Content as VebSocketClient;

                    foreach (ClientMessage cm in Receiving[vs.Client].Messages)
                    {
                        ClientMessageWrap cmw = new ClientMessageWrap(cm);
                        FMessage[i].Add(cmw);
                    }
                    foreach (string e in Receiving[vs.Client].Errors)
                    {
                        FError[i].Add(e);
                    }
                }
            }
            else
            {
                FMessage.SliceCount = 0;
                FError.SliceCount = 0;
            }
            ClearBuffers();
        }
        private void onMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            WebSocketSharp.WebSocket Sender = sender as WebSocketSharp.WebSocket;
            this.Receiving[Sender].Messages.Add(e.ToClientMessage());
        }
        private void onError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            WebSocketSharp.WebSocket Sender = sender as WebSocketSharp.WebSocket;
            this.Receiving[Sender].Errors.Add(e.Message);
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "Info",
        Category = "VebSocket",
        Version = "Client",
        Help = "Get hosted or served client information",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketClientInfoNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VebSocketClientWrap> FInput;

        [Output("Url")]
        public ISpread<string> FUrl;
        [Output("Origin")]
        public ISpread<string> FOrigin;
        [Output("Protocol")]
        public ISpread<string> FProtocol;
        [Output("Extensions")]
        public ISpread<string> FExtensions;
        [Output("Compression")]
        public ISpread<string> FCompression;
        [Output("State")]
        public ISpread<string> FState;
        //[Output("Is Alive")]
        //public ISpread<bool> FIsAlive;
        [Output("Is Secure")]
        public ISpread<bool> FIsSecure;
        
        [Output("Credentials")]
        public ISpread<NetworkCredential> FCredentials;
        [Output("Cookies")]
        public ISpread<ISpread<Cookie>> FCookies;
         

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FUrl.SliceCount = FInput.SliceCount;
                FOrigin.SliceCount = FInput.SliceCount;
                FProtocol.SliceCount = FInput.SliceCount;
                FExtensions.SliceCount = FInput.SliceCount;
                FCompression.SliceCount = FInput.SliceCount;
                FState.SliceCount = FInput.SliceCount;
                //FIsAlive.SliceCount = FInput.SliceCount;
                FIsSecure.SliceCount = FInput.SliceCount;
                FCredentials.SliceCount = FInput.SliceCount;
                FCookies.SliceCount = FInput.SliceCount;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    VebSocketClient vs = FInput[i].Content as VebSocketClient;
                    FUrl[i] = vs.Client.Url.ToString();
                    FOrigin[i] = vs.Client.Origin;
                    FProtocol[i] = vs.Client.Protocol;
                    FExtensions[i] = vs.Client.Extensions;
                    FCompression[i] = vs.Client.Compression.ToString();
                    FState[i] = vs.Client.ReadyState.ToString();
                    //FIsAlive[i] = vs.Client.IsAlive;
                    FIsSecure[i] = vs.Client.IsSecure;
                    FCredentials[i] = vs.Client.Credentials;
                    FCookies[i].SliceCount = 0;
                    
                    foreach(Cookie c in vs.Client.Cookies)
                    {
                        FCookies[i].Add(c);
                    }
                     
                }
            }
            else
            {
                FUrl.SliceCount = 0;
                FOrigin.SliceCount = 0;
                FProtocol.SliceCount = 0;
                FExtensions.SliceCount = 0;
                FCompression.SliceCount = 0;
                FState.SliceCount = 0;
                //FIsAlive.SliceCount = 0;
                FIsSecure.SliceCount = 0;
                FCredentials.SliceCount = 0;
                FCookies.SliceCount = 0;
            }
        }
    }
}