using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.VMath;

namespace VVVV.Packs.VObjects
{
    public class VectorND : ICloneable, IEnumerable<double>
    {
        public List<double> Axis = new List<double>();

        public double x
        {
            get { return Axis[0]; }
            set { Axis[0] = value; }
        }
        public double y
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
        public double z
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
        public double w
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

        public double this[int i]
        {
            get { return Axis[i]; }
            set { Axis[i] = value; }
        }
        public VectorND this[IEnumerable<int> ii]
        {
            get
            {
                List<double> t = new List<double>();
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
                List<double> t = new List<double>();
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

        public IEnumerator<double> GetEnumerator()
        {
            return ((IEnumerable<double>)Axis).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<double>)Axis).GetEnumerator();
        }

        public VectorND()
        {
            Axis.Add(0.0);
        }
        public VectorND(int d)
        {
            for (int i = 0; i < d; i++)
                Axis.Add(0);
        }
        public VectorND(IEnumerable<double> a)
        {
            foreach (double d in a)
                Axis.Add(d);
        }
        public VectorND(params double[] s)
        {
            for (int i = 0; i < s.Length; i++)
                Axis.Add(s[i]);
        }
        public VectorND(Vector2D v)
        {
            Axis.Add(v.x);
            Axis.Add(v.y);
        }
        public VectorND(Vector3D v)
        {
            Axis.Add(v.x);
            Axis.Add(v.y);
            Axis.Add(v.z);
        }
        public VectorND(Vector4D v)
        {
            Axis.Add(v.x);
            Axis.Add(v.y);
            Axis.Add(v.z);
            Axis.Add(v.w);
        }
    }
}
