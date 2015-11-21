using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;
using WebSocketSharp.Server;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(
        Name = "Service",
        Category = "VebSocket",
        Version = "Split",
        Help = "Get connected clients",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketServiceSplitNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VObject> FInput;

        [Output("Clients")]
        public ISpread<ISpread<VebSocketClientWrap>> FClients;
        [Output("Client ID", BinVisibility = PinVisibility.OnlyInspector)]
        public ISpread<ISpread<string>> FClientID;
        [Output("Sessions", BinVisibility = PinVisibility.OnlyInspector)]
        public ISpread<ISpread<IWebSocketSession>> FSessions;

        [Output("New Clients")]
        public ISpread<ISpread<VebSocketClientWrap>> FNewClients;
        [Output("New Client ID", BinVisibility = PinVisibility.OnlyInspector)]
        public ISpread<ISpread<string>> FNewClientID;
        [Output("New Sessions", BinVisibility = PinVisibility.OnlyInspector)]
        public ISpread<ISpread<IWebSocketSession>> FNewSessions;

        [Output("Closed Client ID")]
        public ISpread<ISpread<string>> FClosedClientID;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FClients.SliceCount = FInput.SliceCount;
                FClientID.SliceCount = FInput.SliceCount;
                FSessions.SliceCount = FInput.SliceCount;
                FNewClients.SliceCount = FInput.SliceCount;
                FNewClientID.SliceCount = FInput.SliceCount;
                FNewSessions.SliceCount = FInput.SliceCount;
                FClosedClientID.SliceCount = FInput.SliceCount;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i].Content is VebSocketService)
                    {
                        VebSocketService vs = FInput[i].Content as VebSocketService;
                        FClients[i].SliceCount = vs.Clients.Count;
                        FClientID[i].SliceCount = vs.Clients.Count;
                        FSessions[i].SliceCount = vs.Sessions.Count;
                        int j = 0;
                        foreach(KeyValuePair<string, VebSocketClientWrap> kvp in vs.Clients)
                        {
                            FClients[i][j] = kvp.Value;
                            FClientID[i][j] = kvp.Key;
                            FSessions[i][j] = vs.Sessions[kvp.Key];
                            j++;
                        }

                        FNewClients[i].SliceCount = vs.NewClients.Count;
                        FNewClientID[i].SliceCount = vs.NewClients.Count;
                        FNewSessions[i].SliceCount = vs.NewSessions.Count;
                        j = 0;
                        foreach (KeyValuePair<string, VebSocketClientWrap> kvp in vs.NewClients)
                        {
                            FNewClients[i][j] = kvp.Value;
                            FNewClientID[i][j] = kvp.Key;
                            FNewSessions[i][j] = vs.NewSessions[kvp.Key];
                            j++;
                        }
                    }
                    else
                    {
                        FClients[i].SliceCount = 0;
                        FClientID[i].SliceCount = 0;
                        FSessions[i].SliceCount = 0;
                        FNewClients[i].SliceCount = 0;
                        FNewClientID[i].SliceCount = 0;
                        FNewSessions[i].SliceCount = 0;
                        FClosedClientID[i].SliceCount = 0;
                    }
                }
            }
            else
            {
                FClients.SliceCount = 0;
                FClientID.SliceCount = 0;
                FSessions.SliceCount = 0;
                FNewClients.SliceCount = 0;
                FNewClientID.SliceCount = 0;
                FNewSessions.SliceCount = 0;
                FClosedClientID.SliceCount = 0;
            }
        }
    }
    #region PluginInfo
    [PluginInfo(
        Name = "Manage",
        Category = "VebSocket",
        Version = "Service",
        Help = "Get service information",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketServiceInfoNode : IPluginEvaluate
    {
        [Input("Input")]
        public Pin<VObject> FInput;

        [Output("Path")]
        public ISpread<string> FPath;
        [Output("Active ID's")]
        public ISpread<ISpread<string>> FActiveIDs;
        [Output("Inactive ID's")]
        public ISpread<ISpread<string>> FInActiveIDs;


        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FPath.SliceCount = FInput.SliceCount;
                FActiveIDs.SliceCount = FInput.SliceCount;
                FInActiveIDs.SliceCount = FInput.SliceCount;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    FActiveIDs[i].SliceCount = 0;
                    FInActiveIDs[i].SliceCount = 0;

                    if (FInput[i].Content is VebSocketService)
                    {
                        VebSocketService vs = FInput[i].Content as VebSocketService;
                        FPath[i] = vs.Service.Path;

                        foreach (string id in vs.Service.Sessions.ActiveIDs)
                        {
                            FActiveIDs[i].Add(id);
                        }
                        foreach (string id in vs.Service.Sessions.InactiveIDs)
                        {
                            FInActiveIDs[i].Add(id);
                        }
                    }
                }
            }
            else
            {
                FPath.SliceCount = 0;
                FActiveIDs.SliceCount = 0;
                FInActiveIDs.SliceCount = 0;
            }
        }
    }
}
