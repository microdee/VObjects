using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.PDDN;
using NGISpread = VVVV.PluginInterfaces.V2.NonGeneric.ISpread;
using NGIDiffSpread = VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    [PluginInfo(Name = "VPath", Category = "VObject")]
    public class VObjectGenericVPathNode : VPathNode
    {
        public override void Sift(object Source, string Filter, List<int> MatchingIndices, List<object> Output)
        {
            if (Source is IVPathQueryable)
            {
                var Content = Source as IVPathQueryable;
                var result = Content.VPath(Filter, FSeparator[0]);
                foreach (object o in result)
                {
                    Output.Add(o);
                    MatchingIndices.Add(this.CurrentAbsIndex);
                }
            }
        }
    }
}