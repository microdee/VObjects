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

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(Name = "MainLoopEventProvider", Category = "VVVV", Help = "Provide events for other plugins", Tags = "microdee")]
    #endregion PluginInfo
    public class MainLoopEventProviderNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Import]
        IHDEHost FHDEHost;

        [Output("Events")]
        public ISpread<Dictionary<string, EventHandler>> FEvents;

        private Dictionary<string, EventHandler> events = new Dictionary<string, EventHandler>();

        [ImportingConstructor]
        public MainLoopEventProviderNode()
        {
            FHDEHost.MainLoop.OnDebug += _OnDebug;
            FHDEHost.MainLoop.OnNetworkSync += _OnNetworkSync;
            FHDEHost.MainLoop.OnPrepareGraph += _OnPrepareGraph;
            FHDEHost.MainLoop.OnPresent += _OnPresent;
            FHDEHost.MainLoop.OnRender += _OnRender;
            FHDEHost.MainLoop.OnResetCache += _OnResetCache;
            FHDEHost.MainLoop.OnUpdateView += _OnUpdateView;

            events.Add("OnDebug", this.OnDebug);
            events.Add("OnNetworkSync", this.OnNetworkSync);
            events.Add("OnPrepareGraph", this.OnPrepareGraph);
            events.Add("OnPresent", this.OnPresent);
            events.Add("OnRender", this.OnRender);
            events.Add("OnResetCache", this.OnResetCache);
            events.Add("OnUpdateView", this.OnUpdateView);
        }
        public event EventHandler OnDebug;
        public event EventHandler OnNetworkSync;
        public event EventHandler OnPrepareGraph;
        public event EventHandler OnPresent;
        public event EventHandler OnRender;
        public event EventHandler OnResetCache;
        public event EventHandler OnUpdateView;

        private void _OnDebug(object sender, EventArgs e)
        {
            OnDebug(sender, e);
        }
        private void _OnNetworkSync(object sender, EventArgs e)
        {
            OnNetworkSync(sender, e);
        }
        private void _OnPrepareGraph(object sender, EventArgs e)
        {
            OnPrepareGraph(sender, e);
        }
        private void _OnPresent(object sender, EventArgs e)
        {
            OnPresent(sender, e);
        }
        private void _OnRender(object sender, EventArgs e)
        {
            OnRender(sender, e);
        }
        private void _OnResetCache(object sender, EventArgs e)
        {
            OnResetCache(sender, e);
        }
        private void _OnUpdateView(object sender, EventArgs e)
        {
            OnUpdateView(sender, e);
        }

        public void Evaluate(int SpreadMax)
        {
            FEvents[1] = events;
        }
    }
}
