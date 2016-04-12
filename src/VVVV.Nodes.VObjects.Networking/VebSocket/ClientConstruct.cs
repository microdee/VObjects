using System.ComponentModel.Composition;
using System.Net.Security;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

using WebSocketSharp;
using WebSocketSharp.Net;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(
        Name = "Client",
        Category = "VebSocket",
        Help = "Construct a websocket client",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketClientConstructor : ConstructObjectNode
    {
        [Import]
        public IHDEHost FHDEHost;

        [Input("Url", DefaultString = "ws://localhost")]
        public ISpread<string> FUrl;
        [Input("Origin Header", DefaultString = "")]
        public ISpread<string> FOrigin;
        [Input("Subprotocols", DefaultString = "")]
        public ISpread<ISpread<string>> FProtocols;
        [Input("Username", DefaultString = "")]
        public ISpread<string> FUser;
        [Input("Password", DefaultString = "")]
        public ISpread<string> FPassword;
        [Input("PreAuth")]
        public ISpread<bool> FPreAuth;
        [Input("Compression", DefaultEnumEntry = "None")]
        public ISpread<CompressionMethod> FCompression;
        [Input("Cookie")]
        public Pin<Cookie> FCookie;
        [Input("Server Certificate Validation Callback")]
        public Pin<RemoteCertificateValidationCallback> FServerCertValidCallback;
        
        [Input("Auto Connect", DefaultBoolean = true)]
        public ISpread<bool> FAutoConnect;

        public override object ConstructObject()
        {
            string[] protocols;
            if(FProtocols[this.CurrObj][0] != "")
            {
                protocols = new string[FProtocols[this.CurrObj].SliceCount];
                for(int i=0; i<FProtocols[this.CurrObj].SliceCount; i++)
                {
                    protocols[i] = FProtocols[this.CurrObj][i];
                }
            }
            else protocols = new string[0];

            WebSocketSharp.WebSocket newWebSocket = new WebSocketSharp.WebSocket(FUrl[this.CurrObj], protocols);
            VebSocketHostedClient newVebSocketClient = new VebSocketHostedClient(newWebSocket);

            newVebSocketClient.SubscribeToMainloop(this.FHDEHost);

            if(FOrigin[this.CurrObj] != "")
                newWebSocket.Origin = FOrigin[this.CurrObj];

            if (FCookie.IsConnected && (FCookie[this.CurrObj] != null))
                newWebSocket.SetCookie(FCookie[this.CurrObj]);

            if(FServerCertValidCallback.IsConnected && (FServerCertValidCallback[this.CurrObj] != null))
                newWebSocket.SslConfiguration.ServerCertificateValidationCallback = FServerCertValidCallback[this.CurrObj];

            if((FUser[this.CurrObj] != "") || (FPassword[this.CurrObj] != ""))
                newWebSocket.SetCredentials(FUser[this.CurrObj], FPassword[this.CurrObj], FPreAuth[this.CurrObj]);

            if (FAutoConnect[this.CurrObj])
                newWebSocket.ConnectAsync();

            return newVebSocketClient;
        }
    }
}