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
    [PluginInfo(Name = "PrimitiveTemplate", AutoEvaluate = true, Category = "VObject", Help = "Define a high level Template for Primitive Objects", Tags = "Dynamic, Bin, microdee")]
    #endregion PluginInfo
    public class VObjectPrimitiveTemplateNode : IPluginEvaluate
    {
        [Input("Template Name", DefaultString = "Template")]
        public ISpread<string> FName;

        [Input("Configuration", DefaultString = "string Foo")]
        public ISpread<string> FConfig;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        public void Evaluate(int SpreadMax)
        {
            if (!FUpdate[0])
            {
                if (FUpdate.IsChanged) TemplateDictionary.IsChanged = false; // has updated last frame, but not anymore
                return;
            }
            SpreadMax = FName.SliceCount;

            TemplateDictionary.IsChanged = true;
            for (int i = 0; i < SpreadMax; i++)
            {
                var dict = TemplateDictionary.Instance;

                if (dict.ContainsKey(FName[i]))
                    dict[FName[i]] = FConfig[i];
                else dict.Add(FName[i], FConfig[i]);
            }
        }
    }
}
