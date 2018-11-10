using System;
using System.Collections.Generic;
using System.Text;

namespace GoRogue
{
	public interface IReadOnlyLayeredSpatialMap<T> : IReadOnlySpatialMap<T> where T : IHasID, IHasLayer
	{
		IEnumerable<IReadOnlySpatialMap<T>> Layers { get; }
		int NumberOfLayers { get; }

		IReadOnlyLayeredSpatialMap<T> AsReadOnly();
		IEnumerable<T> GetItems(Coord position, int startingLayer);
		IEnumerable<T> GetItems(int x, int y, int startingLayer);
	}
}
