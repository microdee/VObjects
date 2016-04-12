using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Packs.VObjects
{
    public class TypeIdentity : Dictionary<Type, string>
    {
        private static TypeIdentity _instance;
        public static TypeIdentity Instance
        {
            get
            {
                if (_instance == null) _instance = new TypeIdentity();
                return _instance;
            }
            private set { throw new NotImplementedException(); }
        }

        public TypeIdentity()
        {
            // Add datatypes here and set (de)serialization and deepcopy in ObjectTypePair.cs

            Add(typeof(bool), "bool".ToLower());
            Add(typeof(int), "int".ToLower());
            Add(typeof(double), "double".ToLower());
            Add(typeof(float), "float".ToLower());
            Add(typeof(string), "string".ToLower());

            Add(typeof(RGBAColor), "Color".ToLower());
            Add(typeof(Matrix4x4), "Transform".ToLower());
            Add(typeof(Vector2D), "Vector2D".ToLower());
            Add(typeof(Vector3D), "Vector3D".ToLower());
            Add(typeof(Vector4D), "Vector4D".ToLower());

            Add(typeof(Stream), "Raw".ToLower());
        }
    }
    public class IdentityType : Dictionary<string, Type>
    {
        private static IdentityType _instance;
        public static IdentityType Instance
        {
            get
            {
                if (_instance == null) _instance = new IdentityType();
                return _instance;
            }
            private set { throw new NotImplementedException(); }
        }

        public IdentityType()
        {
            // Add datatypes here and set (de)serialization and deepcopy in ObjectTypePair.cs

            Add("bool".ToLower(), typeof(bool));
            Add("int".ToLower(), typeof(int));
            Add("double".ToLower(), typeof(double));
            Add("float".ToLower(), typeof(float));
            Add("string".ToLower(), typeof(string));

            Add("Color".ToLower(), typeof(RGBAColor));
            Add("Transform".ToLower(), typeof(Matrix4x4));
            Add("Vector2D".ToLower(), typeof(Vector2D));
            Add("Vector3D".ToLower(), typeof(Vector3D));
            Add("Vector4D".ToLower(), typeof(Vector4D));

            Add("Raw".ToLower(), typeof(Stream));
        }
    }
}
