using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

using WebSocketSharp;

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
        [Input("Input Client")]
        public Pin<object> FInput;
        [Input("Messages")]
        public ISpread<ISpread<object>> FMessage;
        [Input("Send", IsBang = true)]  
        public ISpread<ISpread<bool>> FSend;

        [Output("Sending Messages")]
        public ISpread<ISpread<ClientMessage>> FSending;
        [Output("Sent Messages")]
        public ISpread<ISpread<ClientMessage>> FSent;

        [Output("Error")]
        public ISpread<ISpread<string>> FError;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FSending.SliceCount = FInput.SliceCount;
                FSent.SliceCount = FInput.SliceCount;
                FError.SliceCount = FInput.SliceCount;

                for (int i = 0; i < FInput.SliceCount; i++)
                {
                    if (FInput[i] is VebSocketClient)
                    {
                        FSending[i].SliceCount = 0;
                        FSent[i].SliceCount = 0;
                        FError[i].SliceCount = 0;
                        VebSocketClient vs = FInput[i] as VebSocketClient;

                        for (int j = 0; j < FMessage.SliceCount; j++)
                        {
                            if (FSend[i][j])
                            {
                                ClientMessage cm = FMessage[i][j] as ClientMessage;
                                if (cm.Type == Opcode.Text)
                                {
                                    vs.Send(cm.Text);
                                }
                                else
                                {
                                    vs.Send(cm.Raw);
                                }
                            }
                        }

                        FSending[i].SliceCount = 0;
                        foreach (ClientMessage cm in vs.SendingMessages.Values)
                        {
                            FSending[i].Add(cm);
                        }
                        FSent[i].SliceCount = 0;
                        foreach (ClientMessage cm in vs.SentMessages.Values)
                        {
                            FSent[i].Add(cm);
                        }
                        FError[i].SliceCount = 0;
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
                FSending.SliceCount = 0;
                FSent.SliceCount = 0;
                FError.SliceCount = 0;
            }
        }
    }
}
