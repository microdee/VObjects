using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using VVVV.Packs.VObjects;

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
        public WebSocketServiceHost Service;
        public Dictionary<string, IWebSocketSession> Sessions;
        public Dictionary<string, VebSocketClient> Clients;

        public VebSocketService() { }

        public void Add(IWebSocketSession session)
        {
            Sessions.Add(session.ID, session);
            VebSocketClient c = new VebSocketClient(session.Context.WebSocket);
            Clients.Add(session.ID, c);
        }
    }
    class VebSocketServer
    {
        public WebSocketServer Server;
        public Dictionary<string, VebSocketService> Services;
        public VebSocketServer(int port, bool secure)
        {
            this.Server = new WebSocketServer(port, secure);
        }
        public void AddService(string Path)
        {
            this.Server.AddWebSocketService<VebSocketBehavior>(Path);
            foreach(WebSocketServiceHost h in this.Server.WebSocketServices.Hosts)
            {
                if (h.Path == Path)
                {
                    VebSocketService s = new VebSocketService();
                    s.Service = h;
                    this.Services.Add(h.Path, s);
                }
            }
        }
        public void AddService(string Path, Action<WebSocketServer, string> CustomBehavior)
        {
            CustomBehavior(this.Server, Path);
            foreach (WebSocketServiceHost h in this.Server.WebSocketServices.Hosts)
            {
                if (h.Path == Path)
                {
                    VebSocketService s = new VebSocketService();
                    s.Service = h;
                    this.Services.Add(h.Path, s);
                }
            }
        }

        public void TemplateCustomBehavior(WebSocketServer server, string path)
        {
            // add your own behavior by inheriting from WebSocketBehavior class
            this.Server.AddWebSocketService<VebSocketBehavior>(path);
        }
    }
}
