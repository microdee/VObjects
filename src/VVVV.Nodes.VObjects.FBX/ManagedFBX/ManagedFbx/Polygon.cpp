#include "stdafx.h"
#include "Polygon.h"

using namespace ManagedFbx;

Polygon::Polygon(array<int>^ indices) : m_indices(indices)
{
}

array<int> ^Polygon::Indices::get()
{
	return m_indices;
}

void Polygon::Indices::set(array<int> ^value)
{
	m_indices = value;
}