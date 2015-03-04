using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Configuration;
using System.Net.Security;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Packs.VObjects;

using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(
        Name = "Server",
        Category = "VebSocket",
        Help = "Construct a websocket server",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketServerConstructor : ConstructVObjectNode<VebSocketServerWrap>
    {
        [Import]
        public IHDEHost FHDEHost;

        [Input("Port", DefaultValue = 80)]
        public ISpread<int> FPort;
        [Input("Secure")]
        public ISpread<bool> FSecure;
        [Input("Certificate", DefaultString = "", StringType = StringType.Filename)]
        public ISpread<string> FCertPath;
        [Input("Certificate Password", DefaultString = "password")]
        public ISpread<string> FCertPass;
        [Input("Authentication Method")]
        public ISpread<AuthenticationMethod> FAuthMethod;
        [Input("Realm")]
        public ISpread<string> FRealm;
        [Input("Wait Time", DefaultValue = 10)]
        public ISpread<double> FWaitTime;
        [Input("Keep Clean", DefaultBoolean = true)]
        public ISpread<bool> FKeepClean;
        [Input("Services")]
        public ISpread<ISpread<string>> FServices;
        [Input("Custom Service Behaviors")]
        public ISpread<ISpread<VObject>> FServiceBehaviors;

        public override VebSocketServerWrap ConstructVObject()
        {
            VebSocketServer newServer = new VebSocketServer(FPort[this.CurrObj], FSecure[this.CurrObj]);
            VebSocketServerWrap newWrap = new VebSocketServerWrap(newServer);

            newServer.HDEHost = FHDEHost;
            newServer.Server.WaitTime = TimeSpan.FromSeconds(FWaitTime[this.CurrObj]);
            newServer.Server.KeepClean = FKeepClean[this.CurrObj];

            if(FSecure[this.CurrObj])
                newServer.Server.SslConfiguration.ServerCertificate = new X509Certificate2(FCertPath[this.CurrObj], FCertPass[this.CurrObj]);

            if(FAuthMethod[this.CurrObj] != null)
            {
                newServer.Server.Realm = FRealm[this.CurrObj];
                newServer.Server.AuthenticationSchemes = AuthenticationSchemes.Basic;
                newServer.Server.UserCredentialsFinder = FAuthMethod[this.CurrObj].UserCredentialFinder;
            }

            for (int i = 0; i < FServices[this.CurrObj].SliceCount; i++ )
            {
                if (FServiceBehaviors[this.CurrObj][i] != null)
                {
                    Action<WebSocketServer, string> sb = FServiceBehaviors[this.CurrObj][i].Content as Action<WebSocketServer, string>;
                    newServer.AddService(FServices[this.CurrObj][i], sb);
                }
                else newServer.AddService(FServices[this.CurrObj][i]);
            }

            newServer.Server.Start();
            return newWrap;
        }
    }
}
