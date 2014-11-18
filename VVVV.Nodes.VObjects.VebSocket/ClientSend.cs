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
        Name = "Send",
        Category = "VebSocket",
        Version = "Client",
        Help = "Send messages from hosted clients or to served clients",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class VebSocketClientSendMessagesNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VebSocketClientWrap> FInput;
        [Input("Message")]
        public ISpread<ISpread<ClientMessageWrap>> FMessage;
        [Input("Send", IsBang = true)]
        public ISpread<ISpread<bool>> FSend;
        /*
        [Output("Sending Messages GUID")]
        public ISpread<ISpread<int>> FSending;
        [Output("Sent Messages GUID")]
        public ISpread<ISpread<int>> FSent;
         */
        [Output("Error")]
        public ISpread<ISpread<string>> FError;

        public Dictionary<WebSocketSharp.WebSocket, SendingBuffer> Sending = new Dictionary<WebSocketSharp.WebSocket, SendingBuffer>();
        public List<WebSocketSharp.WebSocket> ToBeRemoved = new List<WebSocketSharp.WebSocket>();
        public List<WebSocketSharp.WebSocket> ToBeSent = new List<WebSocketSharp.WebSocket>();
        
        public void AddNewClient()
        {
            foreach(VebSocketClientWrap vsw in FInput)
            {
                VebSocketClient vs = vsw.Content as VebSocketClient;
                if(!Sending.ContainsKey(vs.Client))
                {
                    SendingBuffer rb = new SendingBuffer();
                    rb.Parent = vs;
                    vs.Client.OnError += onError;
                    Sending.Add(vs.Client, rb);
                }
            }
        }
        public void RemoveExpiredClient()
        {
            ToBeRemoved.Clear();
            foreach (VebSocketClientWrap vsw in FInput)
            {
                VebSocketClient vs = vsw.Content as VebSocketClient;
                if (Sending.ContainsKey(vs.Client))
                    Sending[vs.Client].PresentInCurrentContext = true;
            }
            foreach (KeyValuePair<WebSocketSharp.WebSocket, SendingBuffer> kvp in Sending)
            {
                if(!kvp.Value.PresentInCurrentContext)
                    ToBeRemoved.Add(kvp.Key);
            }
            foreach (WebSocketSharp.WebSocket vs in ToBeRemoved)
                Sending.Remove(vs);
        }
        public void ClearBuffers()
        {
            foreach (KeyValuePair<WebSocketSharp.WebSocket, SendingBuffer> kvp in Sending)
            {
                kvp.Value.Errors.Clear();
                //kvp.Value.SendingMessages.Clear();
                //kvp.Value.SentMessages.Clear();
            }
        }


        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                //FSending.SliceCount = FInput.SliceCount;
                //FSent.SliceCount = FInput.SliceCount;
                FError.SliceCount = FInput.SliceCount;

                RemoveExpiredClient();
                AddNewClient();

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    //FSending[i].SliceCount = 0;
                    //FSent[i].SliceCount = 0;
                    FError[i].SliceCount = 0;
                    VebSocketClient vs = FInput[i].Content as VebSocketClient;

                    for (int j = 0; j < FMessage.SliceCount; j++)
                    {
                        if (FSend[i][j])
                        {
                            ClientMessage cm = FMessage[i][j].Content as ClientMessage;
                            if (cm.Type == Opcode.Text)
                            {
                                //Sending[vs.Client].SendingMessages.Add(cm);
                                //ToBeSent.Add(vs.Client);
                                vs.Send(cm.Text, onSent);
                            }
                            else
                            {
                                //Sending[vs.Client].SendingMessages.Add(cm);
                                //ToBeSent.Add(vs.Client);
                                vs.Send(cm.Raw, onSent);
                            }
                        }
                    }

                    foreach (string e in Sending[vs.Client].Errors)
                        FError[i].Add(e);
                    /*
                    foreach (ClientMessage cm in Sending[vs.Client].SendingMessages)
                        FSending[i].Add(cm.GetHashCode());

                    foreach (ClientMessage cm in Sending[vs.Client].SentMessages)
                        FSent[i].Add(cm.GetHashCode());
                     */
                }
            }
            else
            {
                //FSending.SliceCount = 0;
                //FSent.SliceCount = 0;
                FError.SliceCount = 0;
            }
            ClearBuffers();
        }
        private void onSent(bool sent)
        {
            /*
            if (this.ToBeSent.Count > 0)
            {
                WebSocketSharp.WebSocket ws = this.ToBeSent[0];
                if (sent)
                {
                    this.Sending[ws].SentMessages.Add(this.Sending[ws].SendingMessages[0]);
                }
                else
                {
                    string error = "message not sent:" + this.Sending[ws].SendingMessages[0].GetHashCode().ToString();
                    this.Sending[ws].Errors.Add(error);
                }
                this.Sending[ws].SendingMessages.RemoveAt(0);
            }
            */
            return;
        }
        private void onError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            WebSocketSharp.WebSocket Sender = sender as WebSocketSharp.WebSocket;
            string ErrorMessage = e.Message + ":\r\n" + e.Exception.Message;
            this.Sending[Sender].Errors.Add(ErrorMessage);
        }
    }
}
