using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.VObjects
{
    public delegate void ChangedEventHandler(object sender, EventArgs e);
    public class FormularDictionary : Dictionary<string, string>
    {
        private static FormularDictionary instance;
        public static event ChangedEventHandler Changed;

        public static void Change(INode sender)
        {
            Changed(sender, EventArgs.Empty);
        }
        public static bool IsChanged
        {
            get;
            set;
        }

        public static FormularDictionary Instance
        {
            get
            {
                if (instance == null) instance = new FormularDictionary();
                return instance;
            }
        }
    }
}
