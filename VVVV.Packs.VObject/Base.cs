using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace VVVV.Packs.VObjects
{
    /*
     * Object management hackery: 
     * 1 construct object (MyObject)
     * 2 wrap in a "MyObjectWrapper : VObject" :
     *      MyObject Instance = new MyObject();
     *      VObject Generic = new MyObjectWrapper(Instance.GetType(), Instance);
     * NOTE: you probably want to override constructor with :base()
     * 
     */

    // base class
    public class VObject : IDisposable
    {
        public Object Content;
        public Type ObjectType;
        public Stream Serialized = new MemoryStream();
        public VObject() { }
        public VObject(Object content)
        {
            this.Content = content;
            this.ObjectType = content.GetType();
        }
        public VObject(Stream Input)
        {
            this.DeSerialize(Input);
        }

        public bool Disposed = false;
        public virtual void Dispose()
        {
            this.Serialized.Dispose();
            Disposed = true;
            GC.SuppressFinalize(this);
        }

        public virtual void Serialize()
        {
            this.Serialized.Position = 0;
            this.Serialized.SetLength(0);
            // wrapper type
            this.Serialized.WriteUint((uint)this.GetType().ToString().UnicodeLength());
            this.Serialized.WriteUnicode(this.GetType().ToString());
            // object type
            this.Serialized.WriteUint((uint)this.ObjectType.ToString().UnicodeLength());
            this.Serialized.WriteUnicode(this.ObjectType.ToString());
        }
        public virtual void DeSerialize(Stream Input)
        {
            Stream dest = this.Serialized;

            this.Serialized.Position = 0;
            Input.Position = 0;

            this.Serialized.SetLength(0);
            Input.CopyTo(this.Serialized);
            this.Serialized.Position = 0;

            // wrap type
            uint l = this.Serialized.ReadUint();
            Type WrapType = Type.GetType(this.Serialized.ReadUnicode((int)l));
            // object type
            l = this.Serialized.ReadUint();
            this.ObjectType = Type.GetType(this.Serialized.ReadUnicode((int)l));
        }
        public virtual VObject DeepCopy()
        {
            return null;
        }
    }
}
