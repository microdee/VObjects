using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using VVVV.Packs.VObjects;

namespace VVVV.Nodes.VObjects
{
    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "bool",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectboolEditorNode : EditPrimitiveObjectNode<bool> { }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "int",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectintEditorNode : EditPrimitiveObjectNode<int> { }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "double",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectdoubleEditorNode : EditPrimitiveObjectNode<double> { }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "float",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectfloatEditorNode : EditPrimitiveObjectNode<float> { }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "string",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectstringEditorNode : EditPrimitiveObjectNode<string> { }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "Color",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectRGBAColorEditorNode : EditPrimitiveObjectNode<RGBAColor>
    {
        public override RGBAColor Copy(RGBAColor source)
        {
            return new RGBAColor(source.R, source.G, source.B, source.A);
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "Transform",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectMatrix4x4EditorNode : EditPrimitiveObjectNode<Matrix4x4>
    {
        public override Matrix4x4 Copy(Matrix4x4 source)
        {
            return new Matrix4x4(source);
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "Vector2D",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectVector2DEditorNode : EditPrimitiveObjectNode<Vector2D>
    {
        public override Vector2D Copy(Vector2D source)
        {
            return new Vector2D(source);
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "Vector3D",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectVector3DEditorNode : EditPrimitiveObjectNode<Vector3D>
    {
        public override Vector3D Copy(Vector3D source)
        {
            return new Vector3D(source);
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "Vector4D",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectVector4DEditorNode : EditPrimitiveObjectNode<Vector4D>
    {
        public override Vector4D Copy(Vector4D source)
        {
            return new Vector4D(source);
        }
    }

    #region PluginInfo
    [PluginInfo(
        Name = "Edit",
        Category = "PrimitiveObject",
        Version = "Raw",
        Help = "Manage Primitive Objects dynamically",
        Tags = "microdee",
        AutoEvaluate = true
    )]
    #endregion PluginInfo
    public class PrimitiveObjectStreamEditorNode : EditPrimitiveObjectNode<Stream>
    {
        public override Stream Copy(Stream source)
        {
            Stream ns = new MemoryStream();
            source.CopyTo(ns);
            ns.Position = 0;
            return ns;
        }
    }
}
