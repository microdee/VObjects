using System;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    #region basics
    [PluginInfo(Name = "Cons",
                Category = "VObject",
                Help = "Concatenates all input spreads to one output spread.",
                Tags = "generic, spreadop"
                )]
    public class VObjectConsNode : Cons<VObject> { }

    [PluginInfo(Name = "CAR",
                Category = "VObject",
                Version = "Bin",
                Help = "Splits a given spread into first slice and remainder.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class VObjectCARBinNode : CARBin<VObject> { }

    [PluginInfo(Name = "CDR",
                Category = "VObject",
                Version = "Bin",
                Help = "Splits a given spread into remainder and last slice.",
                Tags = "split, generic, spreadop",
                Author = "woei"
               )]
    public class VObjectCDRBinNode : CDRBin<VObject> { }

    [PluginInfo(Name = "Reverse",
                Category = "VObject",
                Version = "Bin",
                Help = "Reverses the order of slices in a given spread.",
                Tags = "invert, generic, spreadop",
                Author = "woei"
               )]
    public class VObjectReverseBinNode : ReverseBin<VObject> { }

    [PluginInfo(Name = "Shift",
                Category = "VObject",
                Version = "Bin",
                Help = "Shifts the slices in a spread upwards by the given phase.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class VObjectShiftBinNode : ShiftBin<VObject> { }

    [PluginInfo(Name = "SetSlice",
                Category = "VObject",
                Version = "Bin",
                Help = "Replaces individual slices of a spread with the given input",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class VObjectSetSliceNode : SetSlice<VObject> { }

    [PluginInfo(Name = "Select",
                Category = "VObject",
                Help = "Select which slices and how many form the output spread.",
                Tags = "resample, generic, spreadop"
               )]
    public class VObjectSelectNode : Select<VObject> { }

    [PluginInfo(Name = "Select",
                Category = "VObject",
                Version = "Bin",
                Help = "Select the slices which form the new spread.",
                Tags = "repeat, generic, spreadop",
                Author = "woei"
            )]
    public class VObjectSelectBinNode : SelectBin<VObject> { }

    [PluginInfo(Name = "Unzip",
                Category = "VObject",
                Help = "Unzips a spread into multiple spreads.",
                Tags = "split, generic, spreadop"
               )]
    public class VObjectUnzipNode : Unzip<VObject> { }

    [PluginInfo(Name = "Unzip",
                Category = "VObject",
                Version = "Bin",
                Help = "Unzips a spread into multiple spreads.",
                Tags = "split, generic, spreadop"
               )]
    public class VObjectUnzipBinNode : Unzip<IInStream<VObject>> { }

    [PluginInfo(Name = "Zip",
                Category = "VObject",
                Help = "Zips spreads together.",
                Tags = "join, generic, spreadop"
               )]
    public class VObjectZipNode : Zip<VObject> { }

    [PluginInfo(Name = "Zip",
                Category = "VObject",
                Version = "Bin",
                Help = "Zips spreads together.",
                Tags = "join, generic, spreadop"
               )]
    public class VObjectZipBinNode : Zip<IInStream<VObject>> { }

    [PluginInfo(Name = "GetSpread",
                Category = "VObject",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class VObjectGetSpreadNode : GetSpreadAdvanced<VObject> { }

    [PluginInfo(Name = "SetSpread",
                Category = "VObject",
                Version = "Bin",
                Help = "Allows to set sub-spreads into a given spread.",
                Tags = "generic, spreadop",
                Author = "woei"
               )]
    public class VObjectSetSpreadNode : SetSpread<VObject> { }

    [PluginInfo(Name = "Pairwise",
                Category = "VObject",
                Help = "Returns all pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class VObjectPairwiseNode : Pairwise<VObject> { }

    [PluginInfo(Name = "SplitAt",
                Category = "VObject",
                Help = "Splits a spread at the given index.",
                Tags = "generic, spreadop"
                )]
    public class VObjectSplitAtNode : SplitAtNode<VObject> { }

    [PluginInfo(Name = "Buffer",
                Category = "VObject",
                Help = "Inserts the input at the given index.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class VObjectBufferNode : BufferNode<VObject> { }

    [PluginInfo(Name = "Queue",
                Category = "VObject",
                Help = "Inserts the input at index 0 and drops the oldest slice in a FIFO (First In First Out) fashion.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class VObjectQueueNode : QueueNode<VObject> { }

    [PluginInfo(Name = "RingBuffer",
                Category = "VObject",
                Help = "Inserts the input at the ringbuffer position.",
                Tags = "generic, spreadop, collection",
                AutoEvaluate = true
               )]
    public class VObjectRingBufferNode : RingBufferNode<VObject> { }

    [PluginInfo(Name = "Store",
                Category = "VObject",
                Help = "Stores a spread and sets/removes/inserts slices.",
                Tags = "add, insert, remove, generic, spreadop, collection",
                Author = "woei",
                AutoEvaluate = true
               )]
    public class VObjectStoreNode : Store<VObject> { }

    [PluginInfo(Name = "Stack",
                Category = "VObject",
                Help = "Stack data structure implementation using the LIFO (Last In First Out) paradigm.",
                Tags = "generic, spreadop, collection",
                Author = "vux"
                )]
    public class VObjectStackNode : StackNode<VObject> { }
    #endregion basics

    [PluginInfo(Name = "FrameDelay",
                Category = "VObject",
                Author = "vux"
                )]
    public class VObjectFrameDelayNode : FrameDelayNode<VObject>
    {
        protected override VObject CloneSlice(VObject slice)
        {
            return slice.DeepCopy();
        }
    }
}
