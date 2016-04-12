#pragma once

namespace ManagedFbx
{
	/// <summary>
	/// Represents a polygon.
	/// </summary>
	public value struct Polygon
	{
	public:
		Polygon(array<int>^ indices);

		/// <summary>
		/// Gets the array of indices which make up this polygon.
		/// </summary>
		property_rw(array<int>^, Indices);

	private:
		array<int>^ m_indices;
	};
}