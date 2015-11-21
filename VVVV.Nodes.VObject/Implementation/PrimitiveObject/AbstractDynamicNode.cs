using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using NGISpread = VVVV.PluginInterfaces.V2.NonGeneric.ISpread;
using NGIDiffSpread = VVVV.PluginInterfaces.V2.NonGeneric.IDiffSpread;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    public static class NodeUtils
    {
        public static NGISpread ToISpread(this IIOContainer pin)
        {
            return (NGISpread)(pin.RawIOObject);
        }

        public static NGIDiffSpread ToIDiffSpread(this IIOContainer pin)
        {
            return (NGIDiffSpread)(pin.RawIOObject);
        }
        public static ISpread<T> ToGenericISpread<T>(this IIOContainer pin)
        {
            return (ISpread<T>)(pin.RawIOObject);
        }
    }

    public abstract class DynamicPrimitiveObjectNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Config("Definition", DefaultString = "string Foo")]
        public IDiffSpread<string> FDefinition;

        [Input("Formular", EnumName = "PrimitiveObjectFormularSelector", Visibility = PinVisibility.OnlyInspector, Order = -1)]
        public ISpread<EnumEntry> FFormular;

        [Import()]
        protected IIOFactory FIOFactory;

        public List<string[]> TypesAndFields = new List<string[]>();
        public Dictionary<string, NGISpread> Spreads = new Dictionary<string, NGISpread>();
        public Dictionary<string, IIOContainer> Pins = new Dictionary<string, IIOContainer>();

        public virtual IOAttribute PinAttribute(string name, int order)
        {
            InputAttribute attr = new InputAttribute(name);
            attr.BinVisibility = PinVisibility.True;
            attr.BinSize = 1;
            attr.Order = order * 2;
            attr.BinOrder = order * 2 + 1;

            return attr;
        }

        public void ChangePins()
        {
            TypesAndFields.Clear();
            
            string def = "";
            if(FDefinition[0] != null)
                def = FDefinition[0];
            string[] tempfields = def.Trim().Split(',');

            int order = 100;
            List<string> fieldnames = new List<string>();
            foreach (string field in tempfields)
            {
                string[] typefieldpair = field.Trim().Split(' ');
                if (typefieldpair.Length == 2)
                {
                    typefieldpair[0] = typefieldpair[0].Trim();
                    typefieldpair[0] = typefieldpair[0].ToLower();
                    typefieldpair[1] = typefieldpair[1].Trim();

                    if (IdentityType.Instance.ContainsKey(typefieldpair[0]) && (!fieldnames.Contains(typefieldpair[1].ToLower())))
                    {
                        fieldnames.Add(typefieldpair[1].ToLower());
                        TypesAndFields.Add(typefieldpair);

                        if(!Spreads.ContainsKey(typefieldpair[1]))
                        {
                            Type currtype = IdentityType.Instance[typefieldpair[0]];
                            Type pinType = typeof(ISpread<>).MakeGenericType((typeof(ISpread<>)).MakeGenericType(currtype));
                            IIOContainer currpin = FIOFactory.CreateIOContainer(pinType, PinAttribute(typefieldpair[1], order));
                            NGISpread currspread = currpin.ToISpread();
                            Spreads.Add(typefieldpair[1], currspread);
                            Pins.Add(typefieldpair[1], currpin);
                        }
                    }
                }
                order++;
            }
            List<string> Removable = new List<string>();
            foreach(string k in Spreads.Keys)
            {
                if (!fieldnames.Contains(k.ToLower()))
                    Removable.Add(k);
            }
            foreach(string k in Removable)
            {
                Spreads.Remove(k);
                Pins[k].Dispose();
                Pins.Remove(k);
            }
        }

        public void Init()
        {
            OnFormularChanged(null, EventArgs.Empty);
            ChangePins();
        }

        void OnConfiguring(object sender, ConfigEventArgs e)
        {
            if (e.PluginConfig.Name == "Definition") ChangePins();
        }

        protected void FormularUpdate()
        {
            FormularDictionary dict = FormularDictionary.Instance;

            if (FFormular.IsChanged)
            {
                List<string> fields = new List<string>();
                List<string> fieldslower = new List<string>();
                for (int i = 0; i < FFormular.SliceCount; i++)
                {
                    if (FFormular[i].Name != "None")
                    {
                        string currform = dict[FFormular[i].Name];
                        string[] splittedform = currform.Trim().Split(',');
                        foreach (string f in splittedform)
                        {
                            string ft = f.Trim();
                            string ftl = ft.ToLower();
                            string[] sftl = ftl.Split(' ');
                            if (sftl.Length == 2)
                            {
                                if (!fieldslower.Contains(sftl[1].Trim()))
                                {
                                    fields.Add(ft);
                                    fieldslower.Add(sftl[1].Trim());
                                }
                            }
                        }
                    }
                }
                string definition = "";
                for (int i = 0; i < fields.Count; i++)
                {
                    if (i == 0) definition = fields[0];
                    else definition += ", " + fields[i];
                }
                if(definition != "") FDefinition[0] = definition;
                ChangePins();
            }
        }

        public virtual void OnEval()
        {

        }

        public int fcr = 0;

        public void OnImportsSatisfied()
        {
            Init();
            FormularDictionary.Changed += OnFormularChanged;
            FIOFactory.Configuring += OnConfiguring;
        }

        void OnFormularChanged(object sender, EventArgs e)
        {
            FormularDictionary dict = FormularDictionary.Instance;
            if (dict.Count == 0)
            {
                EnumManager.UpdateEnum("PrimitiveObjectFormularSelector", "None", new string[1] { "None" });
            }

            List<string> formulars = new List<string>() { "None" };
            foreach (string k in dict.Keys)
                formulars.Add(k);
            EnumManager.UpdateEnum("PrimitiveObjectFormularSelector", "None", formulars.ToArray());
        }
        public void Evaluate(int SpreadMax)
        {
            if (fcr == 0) Init();
            FormularUpdate();
            OnEval();
            fcr++;
        }

    }
}
