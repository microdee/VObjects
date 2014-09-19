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
    public class ObjectTypePair
    {
        public List<object> Objects = new List<object>();
        public Type Type;
        public ObjectTypePair() { }
        public Stream Serialized = new MemoryStream();

        public void Serialize()
        {
            this.Serialized.SetLength(0);
            this.Serialized.WriteUint((uint)this.Objects.Count);
            this.Serialized.WriteUint(this.Type.ToString().UnicodeLength());
            this.Serialized.WriteUnicode(this.Type.ToString());

            for (int i = 0; i < this.Objects.Count; i++)
            {
                uint l = 0;
                if (this.Type == typeof(bool))
                {
                    l = 1;
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteBool((bool)Objects[i]);
                }
                if (this.Type == typeof(int))
                {
                    l = 4;
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteInt((int)Objects[i]);
                }
                if (this.Type == typeof(float))
                {
                    l = 4;
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteFloat((float)Objects[i]);
                }
                if (this.Type == typeof(double))
                {
                    l = 8;
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteDouble((double)Objects[i]);
                }
                if (this.Type == typeof(string))
                {
                    l = ((string)Objects[i]).UnicodeLength();
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteUnicode((string)Objects[i]);
                }

                if (this.Type == typeof(RGBAColor))
                {
                    l = 32;
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteDouble(((RGBAColor)Objects[i]).R);
                    this.Serialized.WriteDouble(((RGBAColor)Objects[i]).G);
                    this.Serialized.WriteDouble(((RGBAColor)Objects[i]).B);
                    this.Serialized.WriteDouble(((RGBAColor)Objects[i]).A);
                }
                if (this.Type == typeof(Vector2D))
                {
                    l = 16;
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteDouble(((Vector2D)Objects[i]).x);
                    this.Serialized.WriteDouble(((Vector2D)Objects[i]).y);
                }
                if (this.Type == typeof(Vector3D))
                {
                    l = 24;
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteDouble(((Vector3D)Objects[i]).x);
                    this.Serialized.WriteDouble(((Vector3D)Objects[i]).y);
                    this.Serialized.WriteDouble(((Vector3D)Objects[i]).z);
                }
                if (this.Type == typeof(Vector4D))
                {
                    l = 32;
                    this.Serialized.WriteUint(l);
                    this.Serialized.WriteDouble(((Vector4D)Objects[i]).x);
                    this.Serialized.WriteDouble(((Vector4D)Objects[i]).y);
                    this.Serialized.WriteDouble(((Vector4D)Objects[i]).z);
                    this.Serialized.WriteDouble(((Vector4D)Objects[i]).w);
                }
                if (this.Type == typeof(Matrix4x4))
                {
                    l = 128;
                    this.Serialized.WriteUint(l);
                    for (int j = 0; j < 16; j++)
                    {
                        this.Serialized.WriteDouble(((Matrix4x4)Objects[i]).Values[j]);
                    }
                }
                if (this.Type == typeof(Stream))
                {
                    l = (uint)((Stream)Objects[i]).Length;
                    this.Serialized.WriteUint(l);
                    ((Stream)Objects[i]).Position = 0;
                    ((Stream)Objects[i]).CopyTo(this.Serialized);
                }
            }
        }

        public void DeSerialize(Stream Input)
        {
            if (!Input.Equals(this.Serialized)) Input.CopyTo(this.Serialized);
            Input.Position = 0;
            Objects.Clear();
            uint objcount = Input.ReadUint();
            uint typeL = Input.ReadUint();
            this.Type = Type.GetType(Input.ReadUnicode((int)typeL));

            for (int i = 0; i < objcount; i++)
            {
                uint l = Input.ReadUint();
                if (this.Type == typeof(bool)) this.Objects.Add((object)Input.ReadBool());
                if (this.Type == typeof(int)) this.Objects.Add((object)Input.ReadInt());
                if (this.Type == typeof(float)) this.Objects.Add((object)Input.ReadFloat());
                if (this.Type == typeof(double)) this.Objects.Add((object)Input.ReadDouble());
                if (this.Type == typeof(string)) this.Objects.Add((object)Input.ReadUnicode((int)l));

                if (this.Type == typeof(RGBAColor))
                {
                    double[] val = new double[4];
                    for(int j=0; j<val.Length; j++) val[j] = Input.ReadDouble();
                    RGBAColor res = new RGBAColor(val);
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Vector2D))
                {
                    Vector2D res = new Vector2D();
                    res.x = Input.ReadDouble();
                    res.y = Input.ReadDouble();
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Vector3D))
                {
                    Vector3D res = new Vector3D();
                    res.x = Input.ReadDouble();
                    res.y = Input.ReadDouble();
                    res.z = Input.ReadDouble();
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Vector4D))
                {
                    Vector4D res = new Vector4D();
                    res.x = Input.ReadDouble();
                    res.y = Input.ReadDouble();
                    res.z = Input.ReadDouble();
                    res.w = Input.ReadDouble();
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Matrix4x4))
                {
                    Matrix4x4 res = new Matrix4x4();
                    for (int j = 0; j < 16; j++) res.Values[j] = Input.ReadDouble();
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Stream))
                {
                    Stream res = new MemoryStream();
                    Input.CopyTo(res, (int)l);
                    res.Position = 0;
                    this.Objects.Add((object)res);
                }
            }
        }
        public void DeSerialize()
        {
            this.DeSerialize(this.Serialized);
        }

        public ObjectTypePair DeepCopy()
        {
            ObjectTypePair Out = new ObjectTypePair();
            Out.Type = this.Type;

            for (int i = 0; i < this.Objects.Count; i++)
            {
                if (this.Type == typeof(bool)) Out.Objects.Add((object)((bool)this.Objects[i]));
                if (this.Type == typeof(int)) Out.Objects.Add((object)((int)this.Objects[i]));
                if (this.Type == typeof(float)) Out.Objects.Add((object)((float)this.Objects[i]));
                if (this.Type == typeof(double)) Out.Objects.Add((object)((double)this.Objects[i]));
                if (this.Type == typeof(string)) Out.Objects.Add((object)((string)this.Objects[i]));

                if (this.Type == typeof(RGBAColor))
                {
                    RGBAColor inp = (RGBAColor)this.Objects[i];
                    RGBAColor res = new RGBAColor(inp.R, inp.G, inp.B, inp.A);
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Vector2D))
                {
                    Vector2D inp = (Vector2D)this.Objects[i];
                    Vector2D res = new Vector2D(inp);
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Vector3D))
                {
                    Vector3D inp = (Vector3D)this.Objects[i];
                    Vector3D res = new Vector3D(inp);
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Vector4D))
                {
                    Vector4D inp = (Vector4D)this.Objects[i];
                    Vector4D res = new Vector4D(inp);
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Matrix4x4))
                {
                    Matrix4x4 inp = (Matrix4x4)this.Objects[i];
                    Matrix4x4 res = new Matrix4x4(inp);
                    this.Objects.Add((object)res);
                }
                if (this.Type == typeof(Stream))
                {
                    Stream inp = (Stream)this.Objects[i];
                    inp.Position = 0;
                    Stream res = new MemoryStream();
                    inp.CopyTo(res);
                    res.Position = 0;
                    this.Objects.Add((object)res);
                }
            }
            return Out;
        }
        public void Dispose()
        {
            if(this.Type == typeof(Stream))
            {
                foreach(object o in this.Objects)
                {
                    Stream tmp = o as Stream;
                    tmp.Dispose();
                }
            }
            this.Serialized.Dispose();
        }
        public object this[int i]
        {
            get { return this.Objects[i]; }
            set
            {
                if(value.GetType() == this.Type)
                    this.Objects[i] = value;
            }
        }
    }
}
