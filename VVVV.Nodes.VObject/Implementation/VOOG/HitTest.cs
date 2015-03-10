using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Packs.VObjects;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.IO;

namespace VVVV.Nodes.VObjects
{
    public abstract class VOOGGeometry
    {
        public List<int> Hits = new List<int>();
        public virtual void HitTest(Matrix4x4 transform, List<Vector2D> points)
        {
            Hits.Clear();
        }
        public VOOGGeometry() { }
    }
    public class VOOGGeometryWrap : VObject
    {
        public VOOGGeometryWrap() : base() { }
        public VOOGGeometryWrap(VOOGGeometry o) : base(o) { }
        public VOOGGeometryWrap(Stream s) : base(s) { }

        public override void Serialize()
        {
            base.Serialize();
        }
        public override VObject DeepCopy()
        {
            return null;
        }
        public override void Dispose()
        {
            base.Dispose();
        }
    }
    public class VOOGQuad : VOOGGeometry
    {
        public override void HitTest(Matrix4x4 transform, List<Vector2D> points)
        {
            double ptx, pty;
            for(int i=0; i<points.Count; i++)
            {
                ptx = points[i].x;
                pty = points[i].y;
                Vector2D v = new Vector2D(ptx, pty);

                Matrix4x4 trobject = new Matrix4x4(transform);
                Vector2D trv = ((!trobject) * v).xy;

                if (trv > -0.5 && trv < 0.5)
                {
                    this.Hits.Add(i);
                }
            }
        }
        public VOOGQuad() { }
    }
    public class VOOGCircle : VOOGGeometry
    {
        public override void HitTest(Matrix4x4 transform, List<Vector2D> points)
        {
            Hits.Clear();
            double ptx, pty;
            for (int i = 0; i < points.Count; i++)
            {
                ptx = points[i].x;
                pty = points[i].y;
                Vector2D v = new Vector2D(ptx, pty);
                Matrix4x4 trobject = new Matrix4x4(transform);
                Vector2D trv = ((!trobject) * v).xy;

                double dist = Math.Sqrt(trv.x * trv.x + trv.y * trv.y);

                if (dist < 0.5)
                {
                    this.Hits.Add(i);
                }
            }
        }
        public VOOGCircle() { }
    }
    public class VOOGSegment : VOOGGeometry
    {
        public double InnerRadius { get; set; }
        public double Phase { get; set; }
        public double Cycles { get; set; }
        public override void HitTest(Matrix4x4 transform, List<Vector2D> points)
        {
            Hits.Clear();
            double ptx, pty;
            for (int i = 0; i < points.Count; i++)
            {
                ptx = points[i].x;
                pty = points[i].y;
                Vector2D v = new Vector2D(ptx, pty);


                Matrix4x4 trobject = new Matrix4x4(transform);
                Vector2D trv = ((!trobject) * v).xy;

                double dist = Math.Sqrt(trv.x * trv.x + trv.y * trv.y);
                bool passed = dist < 0.5;
                double inner = this.InnerRadius * 0.5;
                passed = passed && (dist > inner);

                double phase = this.Phase % 1.0d;

                double angle = Math.Atan2(trv.y, trv.x);
                angle /= (Math.PI * 2.0);
                angle -= this.Phase;
                angle = angle % 1.0d;
                angle = angle < 0 ? (angle + 1.0) : angle;

                passed = passed && (angle < this.Cycles);


                if (passed)
                {
                    this.Hits.Add(i);
                }
            }
        }
        public VOOGSegment() { }
    }
    public class VOOGPolygon : VOOGGeometry
    {
        public List<Vector2D> Vertices = new List<Vector2D>();
        private bool PointInPoly(int nvert, double[] vertx, double[] verty, double testx, double testy)
        {
            bool c = false;
            int j = nvert - 1;
            for (int i = 0; i < nvert; i++)
            {
                if (((verty[i] > testy) != (verty[j] > testy)) &&
                    (testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))
                    c = !c;

                j = i;
            }

            return c;
        }

        private bool inpoly(                    /* is target point inside a 2D polygon? */
            double[] polyx, double[] polyy,     /*   polygon points, [0]=x, [1]=y       */
            int npoints,                        /*   number of points in polygon        */
            double xt, double yt)               /*   x (horizontal) of target point  yt), y (vertical) of target point */
        {
            double xnew, ynew;
            double xold, yold;
            double x1, y1;
            double x2, y2;
            int i;
            bool inside = false;

            if (npoints < 3)
            {
                return (false);
            }
            xold = polyx[npoints - 1];
            yold = polyy[npoints - 1];
            for (i = 0; i < npoints; i++)
            {
                xnew = polyx[i];
                ynew = polyy[i];
                if (xnew > xold)
                {
                    x1 = xold;
                    x2 = xnew;
                    y1 = yold;
                    y2 = ynew;
                }
                else
                {
                    x1 = xnew;
                    x2 = xold;
                    y1 = ynew;
                    y2 = yold;
                }
                if ((xnew < xt) == (xt <= xold)          /* edge "open" at one end */
                 && ((double)yt - (double)y1) * (double)(x2 - x1)
                  < ((double)y2 - (double)y1) * (double)(xt - x1))
                {
                    inside = !inside;
                }
                xold = xnew;
                yold = ynew;
            }
            return (inside);
        }

        public override void HitTest(Matrix4x4 transform, List<Vector2D> points)
        {
            Hits.Clear();
            double ptx, pty;
            for (int i = 0; i < points.Count; i++)
            {
                ptx = points[i].x;
                pty = points[i].y;
                Vector2D v = new Vector2D(ptx, pty);


                int cnt = 0;
                Matrix4x4 trobject = new Matrix4x4(transform);

                double[] dx = new double[this.Vertices.Count];
                double[] dy = new double[this.Vertices.Count];
                for (int k = 0; k < this.Vertices.Count; k++)
                {
                    double x, y;
                    x = this.Vertices[k].x;
                    y = this.Vertices[k].y;

                    Vector2D v2 = new Vector2D(x, y);
                    Vector2D trv = (trobject * v2).xy;

                    dx[k] = trv.x;
                    dy[k] = trv.y;
                    cnt++;
                }

                if (this.inpoly(dx, dy, this.Vertices.Count, ptx, pty))
                {
                    this.Hits.Add(i);
                }
            }
        }
        public VOOGPolygon() { }
    }
}
