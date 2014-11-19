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
    public abstract class EditPrimitiveObjectNode<T> : IPluginEvaluate
    {
        [Input("Primitive Object", Order = 0)]
        public Pin<VObject> FPrimObject;
        [Input("Key", Order = 1)]
        public ISpread<ISpread<string>> FKey;
        [Input("Source", Order = 2)]
        public ISpread<ISpread<T>> FSource;
        [Input("Edit", IsBang = true, Order = 3)]
        public ISpread<ISpread<bool>> FEdit;

        [Input("Manage Existing Key", DefaultEnumEntry = "Overwrite", Order = 12)]
        public ISpread<ManageExistingObject> FExistKeyMan;
        [Input("Manage Not-Existing Key", DefaultEnumEntry = "Create", Order = 14)]
        public ISpread<ManageNotExisting> FNotExistKeyMan;

        [Output("Valid", AutoFlush = false)]
        public ISpread<bool> FValid;

        public virtual T Copy(T source)
        {
            return source;
        }

        public int CurrParent;
        public int CurrSource;

        public virtual void InitializeFrame() { }

        public void Evaluate(int SpreadMax)
        {
            if (FPrimObject.IsConnected)
            {
                FValid.SliceCount = FPrimObject.SliceCount;
                for (int i = 0; i < FPrimObject.SliceCount; i++)
                {
                    FValid[i] = false;
                    this.CurrParent = i;
                    if (FPrimObject[i] is PrimitiveObjectWrap)
                    {
                        FValid[i] = true;
                        PrimitiveObject po = FPrimObject[i].Content as PrimitiveObject;
                        for (int j = 0; j < FSource[i].SliceCount; j++)
                        {
                            this.CurrSource = j;
                            ObjectTypePair current = po[FKey[i][j]];
                            T currsrc = Copy(FSource[i][j]);
                            if((current == null) && (FNotExistKeyMan[i] == ManageNotExisting.Create))
                            {
                                po.Add(FKey[i][j], currsrc);
                            }
                            if(current != null)
                            {
                                if (po[FKey[i][j]].Type == typeof(T))
                                {
                                    if (FExistKeyMan[i] == ManageExistingObject.Overwrite)
                                    {
                                        po[FKey[i][j]].Objects.Clear();
                                        po[FKey[i][j]].Objects.Add(currsrc);
                                    }
                                    if (FExistKeyMan[i] == ManageExistingObject.Extend)
                                    {
                                        po[FKey[i][j]].Objects.Add(currsrc);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
