﻿using System.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;
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
        [Input("Input Client")]
        public Pin<object> FInput;

        [Output("Text Message")]
        public ISpread<ISpread<string>> FTextMessage;
        [Output("Raw Message")]
        public ISpread<ISpread<Stream>> FRawMessage;
        [Output("Message Type")]
        public ISpread<ISpread<string>> FMessageType;
        [Output("Error")]
        public ISpread<ISpread<string>> FError;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FTextMessage.SliceCount = FInput.SliceCount;
                FRawMessage.SliceCount = FInput.SliceCount;
                FMessageType.SliceCount = FInput.SliceCount;
                FError.SliceCount = FInput.SliceCount;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is VebSocketClient)
                    {
                        FTextMessage[i].SliceCount = 0;
                        FMessageType[i].SliceCount = 0;
                        FError[i].SliceCount = 0;
                        VebSocketClient vs = FInput[i] as VebSocketClient;
                        FRawMessage[i].ResizeAndDispose(vs.ReceivedMessages.Count, () => new MemoryStream());
                        int ii = 0;
                        foreach (ClientMessage cm in vs.ReceivedMessages.Values)
                        {
                            FTextMessage[i].Add(cm.Text);
                            FMessageType[i].Add(cm.Type.ToString());
                            FRawMessage[i][ii].Position = 0;
                            FRawMessage[i][ii].SetLength(cm.Raw.Length);
                            FRawMessage[i][ii].Write(cm.Raw, 0, cm.Raw.Length);
                            ii++;
                        }
                        foreach (VebSocketError e in vs.Errors)
                        {
                            if (e.Exception != null) FError[i].Add(e.Message + "\n" + e.Exception.Message);
                            else FError[i].Add(e.Message);
                        }
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
        [Input("Input Client")]
        public Pin<object> FInput;

        [Output("Raw Message")]
        public ISpread<ISpread<ClientMessage>> FMessage;
        [Output("Message Type")]
        public ISpread<ISpread<string>> FMessageType;
        [Output("Error")]
        public ISpread<ISpread<string>> FError;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FMessage.SliceCount = FInput.SliceCount;
                FMessageType.SliceCount = FInput.SliceCount;
                FError.SliceCount = FInput.SliceCount;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is VebSocketClient)
                    {
                        FMessage[i].SliceCount = 0;
                        FMessageType[i].SliceCount = 0;
                        FError[i].SliceCount = 0;
                        VebSocketClient vs = FInput[i] as VebSocketClient;

                        foreach (ClientMessage cm in vs.ReceivedMessages.Values)
                        {
                            FMessage[i].Add(cm);
                            FMessageType[i].Add(cm.Type.ToString());
                        }
                        foreach (VebSocketError e in vs.Errors)
                        {
                            if (e.Exception != null) FError[i].Add(e.Message + "\n" + e.Exception.Message);
                            else FError[i].Add(e.Message);
                        }
                    }
                }
            }
            else
            {
                FMessage.SliceCount = 0;
                FMessageType.SliceCount = 0;
                FError.SliceCount = 0;
            }
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
        public Pin<object> FInput;

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
        [Output("Close Reason")]
        public ISpread<string> FCloseReason;
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
                FCloseReason.SliceCount = FInput.SliceCount;
                FIsSecure.SliceCount = FInput.SliceCount;
                FCredentials.SliceCount = FInput.SliceCount;
                FCookies.SliceCount = FInput.SliceCount;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is VebSocketClient)
                    {
                        VebSocketClient vs = FInput[i] as VebSocketClient;
                        FUrl[i] = vs.Client.Url.ToString();
                        FOrigin[i] = vs.Client.Origin;
                        FProtocol[i] = vs.Client.Protocol;
                        FExtensions[i] = vs.Client.Extensions;
                        FCompression[i] = vs.Client.Compression.ToString();
                        FState[i] = vs.Client.ReadyState.ToString();
                        FCloseReason[i] = vs.CloseReason;
                        FIsSecure[i] = vs.Client.IsSecure;
                        FCredentials[i] = vs.Client.Credentials;
                        FCookies[i].SliceCount = 0;

                        foreach (Cookie c in vs.Client.Cookies)
                        {
                            FCookies[i].Add(c);
                        }
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
                FCloseReason.SliceCount = 0;
                FIsSecure.SliceCount = 0;
                FCredentials.SliceCount = 0;
                FCookies.SliceCount = 0;
            }
        }
    }
}