using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Nodes.VObjects
{
    public class TemplateDictionary : Dictionary<string, string>
    {
        private static TemplateDictionary instance;

        public static bool IsChanged
        {
            get;
            set;
        }

        public static TemplateDictionary Instance
        {
            get
            {
                if (instance == null) instance = new TemplateDictionary();
                return instance;
            }
        }
    }
}
