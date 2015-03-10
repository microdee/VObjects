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
    #region PluginInfo
    [PluginInfo(
        Name = "Host",
        Category = "VOOG",
        Version = "Helper",
        Help = "Hittesting and routing",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class VOOGHostHelperNode : IPluginEvaluate
    {
        [Input("Dictionary")]
        public Pin<VObjectDictionaryWrap> FInput;
        [Input("VOOG World")]
        public ISpread<string> FVPath;
        [Input("VPath Separator", DefaultString="¦")]
        public ISpread<string> FVPathSep;
        [Input("Touch Points")]
        public ISpread<Vector2D> FTouchPoints;
        [Input("Touch ID's")]
        public ISpread<int> FTouchID;
        [Input("Touch Start", IsBang = true)]
        public ISpread<bool> FTouchStart;
        [Input("Touch End", IsBang = true)]
        public ISpread<bool> FTouchEnd;
        [Input("Interact", DefaultBoolean = true)]
        public ISpread<bool> FInteract;

        [Output("GUIObjects")]
        public ISpread<VObjectCollectionWrap> FGUIObjects;
        [Output("GUIElements")]
        public ISpread<VObjectCollectionWrap> FGUIElements;

        private List<VObjectCollection> GUIObjects = new List<VObjectCollection>();

        private Dictionary<int, List<VObjectCollection>> ObjectsUnderPoint = new Dictionary<int, List<VObjectCollection>>();

        public void Evaluate(int SpreadMax)
        {
            VObjectDictionary vd = FInput[0].Content as VObjectDictionary;

            #region clearpoints
            ObjectsUnderPoint.Clear();

            int ii = 0;
            foreach (Vector2D v in FTouchPoints)
            {
                if (!ObjectsUnderPoint.ContainsKey(FTouchID[ii]))
                    ObjectsUnderPoint.Add(FTouchID[ii], new List<VObjectCollection>());
                ii++;
            }
            #endregion clearpoints

            GUIObjects.Clear();

            FGUIElements.SliceCount = 0;
            FGUIObjects.SliceCount = 0;

            #region hittest
            foreach (object o in vd.VPath(FVPath[0], FVPathSep[0]))
            {
                var guiobjectw = o as VObjectCollectionWrap;
                var guiobject = guiobjectw.Content as VObjectCollection;
                var guiew = guiobject["GUIElement"];
                if(guiew != null)
                {
                    var guie = guiew.Content as VObjectCollection;
                    if(guie.Children.ContainsKey("BaseData"))
                    {
                        var basedata = guie["BaseData"].Content as PrimitiveObject;
                        bool enabled = (bool)basedata["Enabled"][0];
                        if(enabled)
                        {
                            FGUIElements.Add(guiew as VObjectCollectionWrap);
                            FGUIObjects.Add(guiobjectw);

                            if(!guie.Children.ContainsKey("Geometry"))
                            {
                                VOOGQuad quadgeom = new VOOGQuad();
                                VOOGGeometryWrap qgw = new VOOGGeometryWrap(quadgeom);
                                guie.Add("Geometry", qgw);
                            }
                            VOOGGeometry geom = guie["Geometry"].Content as VOOGGeometry;
                            geom.Hits.Clear();
                            basedata["TouchID"].Objects.Clear();
                            basedata["Tap"][0] = false;

                            List<Vector2D> pointlist = new List<Vector2D>();
                            foreach (Vector2D v in FTouchPoints)
                                pointlist.Add(new Vector2D(v));

                            geom.HitTest((Matrix4x4)basedata["Transform"][0], pointlist);
                            foreach(int id in geom.Hits)
                            {
                                ObjectsUnderPoint[FTouchID[id]].Add(guiobject);
                            }
                        }
                    }
                }
            }
            #endregion hittest

            #region depthtest
            foreach (KeyValuePair<int, List<VObjectCollection>> kvp in ObjectsUnderPoint)
            {
                VObjectCollection currobj;
                if(kvp.Value.Count == 0)
                {
                    continue;
                }
                if(kvp.Value.Count > 1)
                {
                    List<Tuple<VObjectCollection, double>> UnsortedDepth = new List<Tuple<VObjectCollection, double>>();
                    foreach (VObjectCollection obj in kvp.Value)
                    {
                        ObjectTypePair TrIn = (ObjectTypePair)obj.VPath("\"GUIElement\".\"BaseData\".\"Transform\"", ".")[0];
                        Matrix4x4 mtrx = (Matrix4x4)TrIn[0];
                        UnsortedDepth.Add(new Tuple<VObjectCollection, double>(obj, mtrx.m43));
                    }
                    List<Tuple<VObjectCollection, double>> SortedDepth = UnsortedDepth.OrderBy(t => t.Item2).ToList();
                    currobj = SortedDepth[0].Item1;
                }
                else currobj = kvp.Value[0];

                PrimitiveObjectWrap basedataw = (PrimitiveObjectWrap)currobj.VPath("\"GUIElement\".\"BaseData\"", ".")[0];
                PrimitiveObject basedata = basedataw.Content as PrimitiveObject;
                VOOGGeometryWrap geomw = (VOOGGeometryWrap)currobj.VPath("\"GUIElement\".\"Geometry\"", ".")[0];
                VOOGGeometry geom = geomw.Content as VOOGGeometry;

                //FFocus[0] = vd.Objects[currobj.Name];

                basedata["Tap"][0] = true;
                basedata["TouchID"].Objects.Add(kvp.Key);
            }
            #endregion depthtest
        }

    }
}
