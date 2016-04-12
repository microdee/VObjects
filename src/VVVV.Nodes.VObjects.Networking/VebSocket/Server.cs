using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Principal;
using VVVV.Packs.VObjects;
using VVVV.PluginInterfaces.V2;

using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace VVVV.Nodes.VObjects
{
    public class VebSocketBehavior : WebSocketBehavior
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

    public class UserPassAuthentication : AuthenticationMethod
    {
        // user, password
        public Dictionary<string, NetworkCredential> Users = new Dictionary<string, NetworkCredential>();

        public NetworkCredential UserCredentialFinder(IIdentity id)
        {
            if (this.Users.ContainsKey(id.Name)) return this.Users[id.Name];
            else return null;
        }
        public UserPassAuthentication() { }
    }
    public class AuthenticationMethod
    {
        public NetworkCredential UserCredentialFinder(IIdentity id)
        {
            return null;
        }
        public AuthenticationMethod() { }
    }
    public class VebSocketService : IVPathQueryable
    {
        public IHDEHost HDEHost;

        public WebSocketServiceHost Service;
        public Dictionary<string, IWebSocketSession> Sessions = new Dictionary<string, IWebSocketSession>();
        public Dictionary<string, VebSocketClient> Clients = new Dictionary<string, VebSocketClient>();
        public Dictionary<string, IWebSocketSession> NewSessions = new Dictionary<string, IWebSocketSession>();
        public Dictionary<string, VebSocketClient> NewClients = new Dictionary<string, VebSocketClient>();
        public List<string> ClosedClients = new List<string>();

        public VebSocketService(IHDEHost hdehost, WebSocketServiceHost service)
        {
            this.Service = service;
            this.HDEHost = hdehost;
            this.Service.Sessions.OnConnectedClient += onConnectedClient;
            this.Service.Sessions.OnClosedClient += onClosedClient;
            this.HDEHost.MainLoop.OnUpdateView += onUpdateView;
        }

        private void onUpdateView(object sender, EventArgs e)
        {
            NewSessions.Clear();
            NewClients.Clear();
            ClosedClients.Clear();
        }

        private void onClosedClient(object sender, ClientCloseEventArgs e)
        {
            this.Sessions.Remove(e.ID);
            this.Clients[e.ID].Dispose();
            this.Clients.Remove(e.ID);
            this.ClosedClients.Add(e.ID);
        }

        private void onConnectedClient(object sender, ClientConnectEventArgs e)
        {
            this.Sessions.Add(e.ID, e.Session);
            this.NewSessions.Add(e.ID, e.Session);
            VebSocketClient vc = e.Session.Context.WebSocket.ToVebSocketClient();
            vc.SubscribeToMainloop(this.HDEHost);
            this.Clients.Add(e.ID, vc);
            this.NewClients.Add(e.ID, vc);
        }
        public object VPathGetItem(string key)
        {
            if (this.Clients.ContainsKey(key))
                return this.Clients[key];
            else return null;
        }
        public string[] VPathQueryKeys()
        {
            return this.Clients.Keys.ToArray();
        }
    }

    public class VebSocketServer : IVPathQueryable
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
            WebSocketServiceHost wssh = null;
            foreach(WebSocketServiceHost cwssh in this.Server.WebSocketServices.Hosts)
            {
                if (cwssh.Path == e.Path) wssh = cwssh;
            }
            if (wssh != null)
            {
                VebSocketService s = new VebSocketService(this.HDEHost, wssh);
                this.Services.Add(e.Path, s);
            }
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

        public object VPathGetItem(string key)
        {
            if (this.Services.ContainsKey(key))
            {
                return this.Services[key];
            }
            else return null;
        }
        public string[] VPathQueryKeys()
        {
            return this.Services.Keys.ToArray();
        }
    }
}
