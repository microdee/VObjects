using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using VVVV.Packs.VObjects;
using VVVV.PluginInterfaces.V2;

using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace VVVV.Nodes.VObjects
{
    class VebSocketBehavior : WebSocketBehavior
    {
        /*
         * actual behavior is done in vvvv through the collection of services and clients
         * but you can add your own behavior (service) from your plugin which returns an
         * Action<WebSocketServer, string>
         * pointing to a method similar to
         * "TemplateCustomBehavior" (see below)
         */

        protected override void OnOpen()
        {
            base.OnOpen();
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            base.OnMessage(e);
        }
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);
        }
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
        }
    }
    class VebSocketService
    {
        public IHDEHost HDEHost;

        public WebSocketServiceHost Service;
        public Dictionary<string, IWebSocketSession> Sessions = new Dictionary<string, IWebSocketSession>();
        public Dictionary<string, VebSocketClient> Clients = new Dictionary<string, VebSocketClient>();

        public VebSocketService()
        {
            this.Service.Sessions.OnConnectedClient += onConnectedClient;
            this.Service.Sessions.OnClosedClient += onClosedClient;
        }

        private void onClosedClient(object sender, ClientCloseEventArgs e)
        {
            this.Sessions.Remove(e.ID);
            this.Clients[e.ID].Dispose();
            this.Clients.Remove(e.ID);
        }

        private void onConnectedClient(object sender, ClientConnectEventArgs e)
        {
            this.Sessions.Add(e.ID, e.Session);
            VebSocketClient vc = e.Session.Context.WebSocket.ToVebSocketClient();
            vc.SubscribeToMainloop(this.HDEHost);
            this.Clients.Add(e.ID, vc);
        }
    }
    public class VebSocketServiceWrap : VObject
    {
        public VebSocketServiceWrap() : base() { }
        public VebSocketServiceWrap(VebSocketService o) : base(o) { }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
    class VebSocketServer
    {
        public IHDEHost HDEHost;

        public WebSocketServer Server;
        public Dictionary<string, VebSocketService> Services = new Dictionary<string, VebSocketService>();
        public VebSocketServer(int port, bool secure)
        {
            this.Server = new WebSocketServer(port, secure);
            this.Server.WebSocketServices.ServiceAdded += OnNewService;
        }

        private void OnNewService(object sender, ServiceAddedEventArgs e)
        {
            VebSocketService s = new VebSocketService();
            s.Service = this.Server.WebSocketServices[e.Path];
            s.HDEHost = this.HDEHost;
            this.Services.Add(e.Path, s);
        }
        public void AddService(string Path)
        {
            this.Server.AddWebSocketService<VebSocketBehavior>(Path);
        }
        public void AddService(string Path, Action<WebSocketServer, string> CustomBehavior)
        {
            CustomBehavior(this.Server, Path);
        }

        public void TemplateCustomBehavior(WebSocketServer server, string path)
        {
            // add your own behavior by inheriting from WebSocketBehavior class
            this.Server.AddWebSocketService<VebSocketBehavior>(path);
        }
    }
}
