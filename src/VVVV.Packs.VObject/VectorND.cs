using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VVVV.Utils.VMath;

namespace VVVV.Packs.VObjects
{
    public class VectorND : ICloneable, IEnumerable<float>
    {
        public List<float> Axis = new List<float>();

        public float x
        {
            get { return Axis[0]; }
            set { Axis[0] = value; }
        }
        public float y
        {
            get
            {
                if (Axis.Count >= 2)
                    return Axis[1];
                else throw new Exception("Vector is not 2D.");
            }
            set
            {
                if (Axis.Count >= 2)
                    Axis[1] = value;
                else throw new Exception("Vector is not 2D.");
            }
        }
        public float z
        {
            get
            {
                if (Axis.Count >= 3)
                    return Axis[2];
                else throw new Exception("Vector is not 3D.");
            }
            set
            {
                if (Axis.Count >= 3)
                    Axis[2] = value;
                else throw new Exception("Vector is not 3D.");
            }
        }
        public float w
        {
            get
            {
                if (Axis.Count >= 4)
                    return Axis[3];
                else throw new Exception("Vector is not 4D.");
            }
            set
            {
                if (Axis.Count >= 4)
                    Axis[3] = value;
                else throw new Exception("Vector is not 4D.");
            }
        }

        public float this[int i]
        {
            get { return Axis[i]; }
            set { Axis[i] = value; }
        }
        public VectorND this[IEnumerable<int> ii]
        {
            get
            {
                List<float> t = new List<float>();
                foreach (int i in ii)
                {
                    t.Add(Axis[i]);
                }
                return new VectorND(t);
            }
            set
            {
                int j = 0;
                foreach(int i in ii)
                {
                    Axis[i] = value[j];
                    j++;
                }
            }
        }

        protected List<int> GetIndicesFromPattern(string p)
        {
            List<int> res = new List<int>();
            string axisnames = "xyzw";
            string channelnames = "rgba";
            string numbers = "0123456789";
            foreach(char c in p)
            {
                if (axisnames.Contains(c))
                {
                    res.Add(axisnames.IndexOf(c));
                    continue;
                }
                if(channelnames.Contains(c))
                {
                    res.Add(channelnames.IndexOf(c));
                    continue;
                }
                if (numbers.Contains(c))
                {
                    res.Add(numbers.IndexOf(c));
                    continue;
                }
            }
            return res;
        }

        public VectorND this[string p]
        {
            get
            {
                List<float> t = new List<float>();
                foreach (int i in GetIndicesFromPattern(p))
                {
                    t.Add(Axis[i]);
                }
                return new VectorND(t);
            }
            set
            {
                int j = 0;
                foreach (int i in GetIndicesFromPattern(p))
                {
                    Axis[i] = value[j];
                    j++;
                }
            }
        }
        
        public object Clone()
        {
            return new VectorND(Axis);
        }

        public IEnumerator<float> GetEnumerator()
        {
            return ((IEnumerable<float>)Axis).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<float>)Axis).GetEnumerator();
        }

        public VectorND()
        {
            Axis.Add(0f);
        }
        public VectorND(int d)
        {
            for (int i = 0; i < d; i++)
                Axis.Add(0f);
        }
        public VectorND(IEnumerable<float> a)
        {
            foreach (float d in a)
                Axis.Add(d);
        }
        public VectorND(params float[] s)
        {
            for (int i = 0; i < s.Length; i++)
                Axis.Add(s[i]);
        }
        public VectorND(Vector2D v)
        {
            Axis.Add((float)v.x);
            Axis.Add((float)v.y);
        }
        public VectorND(Vector3D v)
        {
            Axis.Add((float)v.x);
            Axis.Add((float)v.y);
            Axis.Add((float)v.z);
        }
        public VectorND(Vector4D v)
        {
            Axis.Add((float)v.x);
            Axis.Add((float)v.y);
            Axis.Add((float)v.z);
            Axis.Add((float)v.w);
        }
    }
}
