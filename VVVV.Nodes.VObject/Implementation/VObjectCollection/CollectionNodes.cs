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
    [PluginInfo(Name = "Construct", Category = "VObjectCollection", AutoEvaluate = true)]
    public class VObjectCollectionConstructNode : ConstructVObjectNode<VObjectDictionary>
    {
        [Input("Name")]
        public ISpread<ISpread<string>> FName;

        [Input("Manage Existing Object", DefaultEnumEntry = "Extend", Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<ManageExistingObject> FExistObjMan;
        [Input("Manage Not-Existing Object", DefaultEnumEntry = "Create", Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<ManageNotExisting> FNotExistObjMan;

        public override void ConstructVObject(VObjectDictionary Parent)
        {
            if (Parent.Objects.ContainsKey(FName[this.CurrParent][this.CurrChild]))
            {
                if (FNotExistObjMan[this.CurrParent] == ManageNotExisting.Create)
                {
                    VObjectCollection NewObj = new VObjectCollection();
                    VObjectCollectionWrap NewWrap = new VObjectCollectionWrap(NewObj);
                }
            }
        }
    }
}
