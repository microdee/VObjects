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
        public VObject(Type type, Object content)
        {
            this.Content = content;
            this.ObjectType = type;
        }
        public VObject(Type type, Stream Input)
        {
            this.ObjectType = type;
            this.DeSerialize(Input);
        }

        protected bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;
            if (disposing)
                this.Serialized.Dispose();
            disposed = true;
        }

        public virtual void Serialize()
        {
            this.Serialized.Position = 0;
            this.Serialized.SetLength(0);
            this.Serialized.WriteUint((uint)this.GetType().ToString().Length);
            this.Serialized.WriteUnicode(this.GetType().ToString());
            this.Serialized.WriteUint((uint)this.ObjectType.ToString().Length);
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

            uint l = this.Serialized.ReadUint();
            this.Serialized.ReadUnicode((int)l);
            l = this.Serialized.ReadUint();
            this.Serialized.ReadUnicode((int)l);
        }
        public virtual VObject DeepCopy();
    }
}
