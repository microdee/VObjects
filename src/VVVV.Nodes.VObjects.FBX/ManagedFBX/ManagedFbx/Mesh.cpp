#include "stdafx.h"
#include "Mesh.h"
#include "Manager.h"

using namespace ManagedFbx;
using namespace System::Runtime::InteropServices;

Mesh::Mesh(FbxMesh *nativeMesh)
{
	m_nativeMesh = nativeMesh;
}
Mesh::Mesh(string^ name, SceneNode^ container) : Mesh(FbxMesh::Create(container->m_nativeNode, (const char*)Marshal::StringToHGlobalAnsi(name).ToPointer()))
{
	// Create an empty default layer
	m_nativeMesh->CreateLayer();
}

Mesh ^Mesh::Triangulate()
{
	auto mesh = gcnew Mesh((FbxMesh*)Manager::GetGeomConverter()->Triangulate(m_nativeMesh->GetNode()->GetNodeAttribute(), true));
	mesh->UVLayer = UVLayer;
	return mesh;
}

bool Mesh::Triangulated::get()
{
	return m_nativeMesh->IsTriangleMesh();
}

array<ManagedFbx::Polygon> ^Mesh::Polygons::get()
{
	int count = m_nativeMesh->GetPolygonCount();
	auto list = gcnew array<Polygon>(count);

	for(int i = 0; i < count; i++)
	{
		auto poly = Polygon();

		int indexCount = m_nativeMesh->GetPolygonSize(i);
		poly.Indices = gcnew array<int>(indexCount);

		for(int j = 0; j < indexCount; j++)
			poly.Indices[j] = m_nativeMesh->GetPolygonVertex(i, j);

		list[i] = poly;
	}

	return list;
}
void Mesh::Polygons::set(array<ManagedFbx::Polygon>^ value)
{
	for (int i = m_nativeMesh->GetPolygonCount() - 1; i >= 0; i--)
	{
		m_nativeMesh->RemovePolygon(i);
	}
	m_nativeMesh->ReservePolygonCount(value->Length);
	for each (Polygon p in value)
	{
		m_nativeMesh->BeginPolygon(0);
		for each (int idx in p.Indices)
		{
			m_nativeMesh->AddPolygon(idx);
		}
		m_nativeMesh->EndPolygon();
	}
}

array<Vector3> ^Mesh::Vertices::get()
{
	int count = m_nativeMesh->GetControlPointsCount();
	auto list = gcnew array<Vector3>(count);

	for(int i = 0; i < count; i++)
	{
		auto point =  m_nativeMesh->GetControlPointAt(i);
		list[i] = Vector3(point);
	}

	return list;
}
void Mesh::Vertices::set(array<Vector3>^ value)
{
	m_nativeMesh->InitControlPoints(value->Length);
	auto pts = m_nativeMesh->GetControlPoints();
	for (int i = 0; i < value->Length; i++)
	{
		pts[i] = FbxVector4(
			value[i].X,
			value[i].Y,
			value[i].Z,
			1.0);
	}
}

array<Vector3> ^Mesh::Normals::get()
{
	auto normals = m_nativeMesh->GetLayer(0)->GetNormals();
	int count = normals->GetDirectArray().GetCount();
	auto list = gcnew array<Vector3>(count);

	for(int i = 0; i < count; i++)
		list[i] = Vector3(normals->GetDirectArray().GetAt(i));

	return list;
}

void Mesh::Normals::set(array<Vector3>^ value)
{
	auto normals = m_nativeMesh->GetLayer(0)->GetNormals();
	if (normals == NULL)
	{
		normals = m_nativeMesh->CreateElementNormal();
	}
	normals->GetDirectArray().Clear();
	normals->GetDirectArray().Resize(value->Length);
	for (int i = 0; i < value->Length; i++)
	{
		normals->GetDirectArray().Add(FbxVector4(
			value[i].X,
			value[i].Y,
			value[i].Z,
			1.0));
	}
}

array<Vector2> ^Mesh::TextureCoords::get()
{
	auto layer = m_nativeMesh->GetLayer(UVLayer);

	if(!layer)
		return gcnew array<Vector2>(0);

	auto coords = layer->GetUVs();
	int count = coords == nullptr ? 0 : coords->GetDirectArray().GetCount();
	auto list = gcnew array<Vector2>(count);

	for(int i = 0; i < count; i++)
		list[i] = Vector2(coords->GetDirectArray().GetAt(i));

	return list;
}

int Mesh::GetMaterialId(int polygon)
{
	FbxLayerElementArrayTemplate<int> *materials = nullptr;
	m_nativeMesh->GetMaterialIndices(&materials);
	return materials->GetAt(polygon);
}

array<int> ^Mesh::MaterialIDs::get()
{
	auto materials = m_nativeMesh->GetLayer(0)->GetMaterials();
	int count = materials == nullptr ? 0 : materials->GetIndexArray().GetCount();
	auto list = gcnew array<int>(count);

	for(int i = 0; i < count; i++)
		list[i] = materials->GetIndexArray().GetAt(i);

	return list;
}

int Mesh::GetUVIndex(int polygon, int index)
{
	return m_nativeMesh->GetTextureUVIndex(polygon, index);
}

Vector3 Mesh::GetVertexNormal(int polygon, int index)
{
	FbxVector4 normal;
	m_nativeMesh->GetPolygonVertexNormal(polygon, index, normal);
	return Vector3(normal);
}

array<Colour> ^Mesh::VertexColours::get()
{
	auto colours = m_nativeMesh->GetLayer(0)->GetVertexColors();
	int count = colours == nullptr ? 0 : colours->GetDirectArray().GetCount();
	auto list = gcnew array<Colour>(count);

	for(int i = 0; i < count; i++)
		list[i] = Colour(colours->GetDirectArray().GetAt(i));

	return list;
}