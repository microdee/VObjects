using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Reflection.Emit;

namespace VVVV.Packs.VObjects
{
    public static class DynamicConstruct
    {
        public static VObject ActivatorCreateInstance(Stream s)
        {
            s.Position = 0;
            int DerivedTypeL = (int)s.ReadUint();
            Type DerivedType = Type.GetType(s.ReadUnicode(DerivedTypeL));
            s.Position = 0;

            Object[] ConstrArgs = new Object[] { s };
            return Activator.CreateInstance(DerivedType, ConstrArgs) as VObject;
        }

        private static object ObjectGenerator(Type type)
        {
            var target = type.GetConstructor(Type.EmptyTypes);
            var dynamic = new DynamicMethod(string.Empty,
                          type,
                          new Type[0],
                          target.DeclaringType);
            var il = dynamic.GetILGenerator();
            il.DeclareLocal(target.DeclaringType);
            il.Emit(OpCodes.Newobj, target);
            il.Emit(OpCodes.Stloc_0);
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ret);

            var method = (Func<object>)dynamic.CreateDelegate(typeof(Func<object>));
            return method();
        }

        public static VObject DynamicGenerator(Stream s)
        {
            s.Position = 0;
            int DerivedTypeL = (int)s.ReadUint();
            Type DerivedType = Type.GetType(s.ReadUnicode(DerivedTypeL));
            s.Position = 0;

            VObject result = ObjectGenerator(DerivedType) as VObject;
            result.DeSerialize(s);
            return result;
        }
        public static VObject AssemblyCreateInstance(Stream s)
        {
            s.Position = 0;
            int DerivedTypeL = (int)s.ReadUint();
            string DerivedTypeName = s.ReadUnicode(DerivedTypeL);
            Type DerivedType = Type.GetType(DerivedTypeName);
            s.Position = 0;

            VObject result = DerivedType.Assembly.CreateInstance(DerivedTypeName) as VObject;
            result.DeSerialize(s);
            return result;
        }
    }
}
