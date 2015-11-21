using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(Name = "Formular", AutoEvaluate = true, Category = "PrimitiveObject", Help = "Define a high level Template for Primitive Objects", Author = "microdee")]
    #endregion PluginInfo
    public class VObjectPrimitiveTemplateNode : IPluginEvaluate
    {
        [Input("Formular Name", DefaultString = "Formular")]
        public ISpread<string> FName;

        [Input("Definition", DefaultString = "string Foo")]
        public ISpread<string> FDefinition;

        [Input("Update", IsSingle = true, IsBang = true, DefaultBoolean = false)]
        public IDiffSpread<bool> FUpdate;

        [Import()]
        public INode ThisNode;

        int fcr = 0;

        public void Evaluate(int SpreadMax)
        {
            if (FUpdate[0] || (fcr == 0))
            {
                SpreadMax = FName.SliceCount;

                FormularDictionary.Change(ThisNode);

                for (int i = 0; i < SpreadMax; i++)
                {
                    var dict = FormularDictionary.Instance;

                    if (dict.ContainsKey(FName[i]))
                        dict[FName[i]] = FDefinition[i];
                    else dict.Add(FName[i], FDefinition[i]);
                }
                FormularDictionary.Change(ThisNode);
            }
            fcr++;
        }

    }
}
