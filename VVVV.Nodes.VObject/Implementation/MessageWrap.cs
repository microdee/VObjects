using System.IO;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Packs.VObjects;
using VVVV.Packs.Messaging;
using VVVV.Packs.Messaging.Serializing;

namespace VVVV.Nodes.VObjects
{
    public class MessageWrap : VObject
    {
        public MessageWrap() : base() { }
        public MessageWrap(Message o) : base(o) { }
        public MessageWrap(Stream s) : base(s) { }

        public override void Serialize()
        {
            base.Serialize();
            Message m = this.Content as Message;
            m.Serialize().CopyTo(this.Serialized);
        }

        public override void DeSerialize(Stream Input)
        {
            base.DeSerialize(Input);
            Stream sm = new MemoryStream();
            Input.CopyTo(sm);
            Message m = sm.DeSerializeMessage();
            this.Content = m;
        }
        public override VObject DeepCopy()
        {
            Message m = this.Content as Message;
            VObject mc = new MessageWrap(m.Clone() as Message);
            return mc;
        }
    }
    
    [PluginInfo(Name = "Wrap", Category = "Message", Version = "VObject")]
    public class MessageWrapperNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<Message> FInput;

        [Output("Output")]
        public ISpread<MessageWrap> FOutput;
        #endregion fields & pins
        //called when data for any output pin is requested

        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = FInput.SliceCount;
            for (int i = 0; i < FInput.SliceCount; i++)
            {
                if(FOutput[i] == null)
                {
                    FOutput[i] = new MessageWrap(FInput[i]);
                }
                else
                {
                    FOutput[i].Content = FInput[i];
                }
            }
        }
    }

    [PluginInfo(Name = "UnWrap", Category = "VObject", Version = "Message")]
    public class MessageUnWrapperNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<VObject> FInput;

        [Output("Output")]
        public ISpread<Message> FOutput;

        [Output("Valid")]
        public ISpread<bool> FValid;
        #endregion fields & pins
        //called when data for any output pin is requested

        public void Evaluate(int SpreadMax)
        {
            FOutput.SliceCount = 0;
            FValid.SliceCount = FInput.SliceCount;

            for (int i = 0; i < FInput.SliceCount; i++)
            {
                if (FInput[i] is MessageWrap)
                {
                    FOutput.Add(FInput[i].Content as Message);
                    FValid[i] = true;
                }
                else
                {
                    FValid[i] = false;
                }
            }
        }
    }
}
