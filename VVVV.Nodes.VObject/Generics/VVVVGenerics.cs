using System;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    #region basics
    [PluginInfo(Name = "Cons",
                Category = "Object",
                Help = "Concatenates all input spreads to one output spread.",
                Tags = "generic, spreadop"
                )]
    public class VObjectConsNode : Cons<object> { }

    [PluginInfo(Name = "CAR",
                Category = "Object",
                Version = "Bin",
                Help = "Splits a given spread into first slice and remainder.",
                Tags = "split, generic, spreadop"
               )]
    public class VObjectCARBinNode : CARBin<object> { }

    [PluginInfo(Name = "CDR",
                Category = "Object",
                Version = "Bin",
                Help = "Splits a given spread into remainder and last slice.",
                Tags = "split, generic, spreadop"
               )]
    public class VObjectCDRBinNode : CDRBin<object> { }

    [PluginInfo(Name = "Reverse",
                Category = "Object",
                Version = "Bin",
                Help = "Reverses the order of slices in a given spread.",
                Tags = "invert, generic, spreadop"
               )]
    public class VObjectReverseBinNode : ReverseBin<object> { }

    [PluginInfo(Name = "Shift",
                Category = "Object",
                Version = "Bin",
                Help = "Shifts the slices in a spread upwards by the given phase.",
                Tags = "generic, spreadop"
               )]
    public class VObjectShiftBinNode : ShiftBin<object> { }

    [PluginInfo(Name = "SetSlice",
                Category = "Object",
                Version = "Bin",
                Help = "Replaces individual slices of a spread with the given input",
                Tags = "generic, spreadop"
               )]
    public class VObjectSetSliceNode : SetSlice<object> { }

    [PluginInfo(Name = "Select",
                Category = "Object",
                Help = "Select which slices and how many form the output spread.",
                Tags = "resample, generic, spreadop"
               )]
    public class VObjectSelectNode : Select<object> { }

    [PluginInfo(Name = "Select",
                Category = "Object",
                Version = "Bin",
                Help = "Select the slices which form the new spread.",
                Tags = "repeat, generic, spreadop"
            )]
    public class VObjectSelectBinNode : SelectBin<object> { }

    [PluginInfo(Name = "Unzip",
                Category = "Object",
                Help = "Unzips a spread into multiple spreads.",
                Tags = "split, generic, spreadop"
               )]
    public class VObjectUnzipNode : Unzip<object> { }

    [PluginInfo(Name = "Unzip",
                Category = "Object",
                Version = "Bin",
                Help = "Unzips a spread into multiple spreads.",
                Tags = "split, generic, spreadop"
               )]
    public class VObjectUnzipBinNode : Unzip<IInStream<object>> { }

    [PluginInfo(Name = "Zip",
                Category = "Object",
                Help = "Zips spreads together.",
                Tags = "join, generic, spreadop"
               )]
    public class VObjectZipNode : Zip<object> { }

    [PluginInfo(Name = "Zip",
                Category = "Object",
                Version = "Bin",
                Help = "Zips spreads together.",
                Tags = "join, generic, spreadop"
               )]
    public class VObjectZipBinNode : Zip<IInStream<object>> { }

    [PluginInfo(Name = "GetSpread",
                Category = "Object",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop"
            )]
    public class VObjectGetSpreadNode : GetSpreadAdvanced<object> { }

    [PluginInfo(Name = "SetSpread",
                Category = "Object",
                Version = "Bin",
                Help = "Allows to set sub-spreads into a given spread.",
                Tags = "generic, spreadop"
               )]
    public class VObjectSetSpreadNode : SetSpread<object> { }

    [PluginInfo(Name = "Pairwise",
                Category = "Object",
                Help = "Returns all pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class VObjectPairwiseNode : Pairwise<object> { }

    [PluginInfo(Name = "SplitAt",
                Category = "Object",
                Help = "Splits a spread at the given index.",
                Tags = "generic, spreadop"
                )]
    public class VObjectSplitAtNode : SplitAtNode<object> { }

    [PluginInfo(Name = "Buffer",
                Category = "Object",
                Help = "Inserts the input at the given index.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class VObjectBufferNode : BufferNode<object> { }

    [PluginInfo(Name = "Queue",
                Category = "Object",
                Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class VObjectQueueNode : QueueNode<object> { }

    [PluginInfo(Name = "RingBuffer",
                Category = "Object",
                Help = "Inserts the input at the ringbuffer position.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class VObjectRingBufferNode : RingBufferNode<object> { }

    [PluginInfo(Name = "Store",
                Category = "Object",
                Help = "Stores a spread and sets/removes/inserts slices.",
                Tags = "add, insert, remove, generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class VObjectStoreNode : Store<object> { }

    [PluginInfo(Name = "Stack",
                Category = "Object",
                Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
                Tags = "generic, spreadop, collection"
                )]
    public class VObjectStackNode : StackNode<object> { }
    #endregion basics

    [PluginInfo(Name = "FrameDelay",
                Category = "Object"
                )]
    public class VObjectFrameDelayNode : FrameDelayNode<object>
    {
        protected override object CloneSlice(object slice)
        {
            if (slice is ICloneable)
            {
                var t = slice as ICloneable;
                return t.Clone();
            }
            else
            {
                throw new ObjectIsNotCloneableException("Object does not implement ICloneable.");
            }
        }
    }
}
