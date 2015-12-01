using System.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;

using WebSocketSharp;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(
        Name = "ClientMessage",
        Category = "Join",
        Version = "String",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketClientTextMessageJoinNode : IPluginEvaluate
    {
        [Input("Input")]
        public IDiffSpread<string> FInput;

        [Output("Output")]
        public ISpread<ClientMessage> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if(FInput.IsChanged)
            {
                FOutput.SliceCount = SpreadMax;
                for(int i=0; i<SpreadMax; i++)
                {
                    ClientMessage cm = new ClientMessage();
                    cm.Type = Opcode.Text;
                    cm.Text = FInput[i];
                    cm.Raw = new byte[0];
                    FOutput[i] = cm;
                }
            }
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "ClientMessage",
        Category = "Join",
        Version = "Raw",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketClientRawMessageJoinNode : IPluginEvaluate
    {
        [Input("Input")]
        public IDiffSpread<Stream> FInput;

        [Output("Output")]
        public ISpread<ClientMessage> FOutput;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsChanged)
            {
                FOutput.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    ClientMessage cm = new ClientMessage();
                    cm.Type = Opcode.Text;
                    cm.Text = "";
                    long sl = FInput[i].Length;
                    byte[] tmp = new byte[(int)sl];
                    FInput[i].Read(tmp, 0, (int)sl);
                    cm.Raw = tmp;
                    FOutput[i] = cm;
                }
            }
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "ClientMessage",
        Category = "Split",
        Tags = "microdee"
    )]
    #endregion PluginInfo
    public class VebSocketClientMessageSplitNode : IPluginEvaluate
    {
        [Input("Input Message")]
        public Pin<object> FInput;

        [Output("Text")]
        public ISpread<string> FText;
        [Output("Binary")]
        public ISpread<Stream> FBinary;
        [Output("Type")]
        public ISpread<string> FType;

        public void Evaluate(int SpreadMax)
        {
            if (FInput.IsConnected)
            {
                FText.SliceCount = SpreadMax;
                FBinary.SliceCount = SpreadMax;
                FType.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FInput[i] is ClientMessage)
                    {
                        ClientMessage cm = FInput[i] as ClientMessage;
                        FText[i] = cm.Text;
                        FBinary[i] = cm.Raw.ToStream();
                        FType[i] = cm.Type.ToString();
                    }
                }
            }
        }
    }
}
