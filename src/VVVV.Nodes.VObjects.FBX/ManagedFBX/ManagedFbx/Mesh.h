#pragma once

#include "Polygon.h"

namespace ManagedFbx
{
	ref class SceneNode;
	public ref class Mesh
	{
	public:
		Mesh(string^ name, SceneNode^ container);

		property_rw(array<Polygon>^, Polygons);
		property_rw(array<Vector3>^, Vertices);
		property_rw(array<Vector3>^, Normals);
		property_r(array<Vector2>^, TextureCoords);
		property_r(array<Colour>^, VertexColours);
		property_r(array<int>^, MaterialIDs);
		property_r(bool, Triangulated);
		property int UVLayer;

		Mesh ^Triangulate();

		int GetUVIndex(int polygon, int index);
		int GetMaterialId(int polygon);
		Vector3 GetVertexNormal(int polygon, int index);

	internal:
		Mesh(FbxMesh *nativeMesh);
		FbxMesh *m_nativeMesh;
	};
}