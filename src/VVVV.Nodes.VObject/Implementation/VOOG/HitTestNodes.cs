using System;
using System.IO;
using System.ComponentModel.Composition;
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
    [PluginInfo(
        Name = "Quad",
        Category = "VOOG",
        Version = "Geometry",
        Tags = "microdee"
    )]
    public class VOOGQuadNode : ConstructVObjectNode<VOOGGeometryWrap>
    {
        public override VOOGGeometryWrap ConstructVObject()
        {
            VOOGGeometry vg = new VOOGQuad();
            return new VOOGGeometryWrap(vg);
        }
    }

    [PluginInfo(
        Name = "Circle",
        Category = "VOOG",
        Version = "Geometry",
        Tags = "microdee"
    )]
    public class VOOGCircleNode : ConstructVObjectNode<VOOGGeometryWrap>
    {
        public override VOOGGeometryWrap ConstructVObject()
        {
            VOOGGeometry vg = new VOOGCircle();
            return new VOOGGeometryWrap(vg);
        }
    }

    [PluginInfo(
        Name = "Segment",
        Category = "VOOG",
        Version = "Geometry",
        Tags = "microdee"
    )]
    public class VOOGSegmentNode : ConstructAndSetVObjectNode<VOOGGeometryWrap>
    {
        [Input("Inner Radius")]
        public ISpread<double> FInnerRadius;
        [Input("Phase")]
        public ISpread<double> FPhase;
        [Input("Cycles")]
        public ISpread<double> FCycles;

        public override VOOGGeometryWrap ConstructVObject()
        {
            VOOGSegment vg = new VOOGSegment();
            vg.InnerRadius = FInnerRadius[this.CurrObj];
            vg.Phase = FPhase[this.CurrObj];
            vg.Cycles = FCycles[this.CurrObj];
            return new VOOGGeometryWrap(vg);
        }
        public override void SetVObject(VOOGGeometryWrap Obj)
        {
            VOOGSegment vg = Obj.Content as VOOGSegment;
            vg.InnerRadius = FInnerRadius[this.CurrObj];
            vg.Phase = FPhase[this.CurrObj];
            vg.Cycles = FCycles[this.CurrObj];
        }
    }

    [PluginInfo(
        Name = "Polygon",
        Category = "VOOG",
        Version = "Geometry",
        Tags = "microdee"
    )]
    public class VOOGPolygonNode : ConstructAndSetVObjectNode<VOOGGeometryWrap>
    {
        [Input("Vertices")]
        public ISpread<ISpread<Vector2D>> FVertices;

        public override void SetSliceCount(int SpreadMax)
        {
            this.SliceCount = Math.Max(Math.Max(FConstruct.SliceCount, FSet.SliceCount), FVertices.SliceCount);
        }

        public override VOOGGeometryWrap ConstructVObject()
        {
            VOOGPolygon vg = new VOOGPolygon();
            foreach(Vector2D v in FVertices[this.CurrObj])
            {
                vg.Vertices.Add(new Vector2D(v));
            }
            return new VOOGGeometryWrap(vg);
        }
        public override void SetVObject(VOOGGeometryWrap Obj)
        {
            VOOGPolygon vg = Obj.Content as VOOGPolygon;
            vg.Vertices.Clear();
            foreach (Vector2D v in FVertices[this.CurrObj])
            {
                vg.Vertices.Add(new Vector2D(v));
            }
        }
    }
}
