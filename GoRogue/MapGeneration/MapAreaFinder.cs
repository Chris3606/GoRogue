using GoRogue.MapViews;
using System;
using System.Collections.Generic;

namespace GoRogue.MapGeneration
{
	/// <summary>
	/// Class designed to calculate and produce a list of MapAreas representing each unique connected
	/// area of the map.
	/// </summary>
	/// <remarks>
	/// The class takes in an IMapView, where a value of true for a given position indicates it
	/// should be part of a map area, and false indicates it should not be part of any map area. In a
	/// classic roguelike dungeon example, this might be a walkability map where floors return a
	/// value of true and walls a value of false.
	/// </remarks>
	public class MapAreaFinder
	{
		/// <summary>
		/// The method used for determining connectivity of the grid.
		/// </summary>
		public AdjacencyRule AdjacencyMethod;

		/// <summary>
		/// IMapView indicating which cells should be considered part of a map area and which should not.
		/// </summary>
		public IMapView<bool> Map;

		private bool[,] visited;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="map">
		/// IMapView indicating which cells should be considered part of a map area and which should not.
		/// </param>
		/// <param name="adjacencyMethod">The method used for determining connectivity of the grid.</param>
		public MapAreaFinder(IMapView<bool> map, AdjacencyRule adjacencyMethod)
		{
			Map = map;
			visited = null;
			AdjacencyMethod = adjacencyMethod;
		}

		/// <summary>
		/// Convenience function that creates an MapAreaFinder and returns the result of that
		/// MapAreaFinder's MapAreas function. Intended to be used for cases in which the area finder
		/// will never be re-used.
		/// </summary>
		/// <param name="map">
		/// IMapView indicating which cells should be considered part of a map area and which should not.
		/// </param>
		/// <param name="adjacencyMethod">The method used for determining connectivity of the grid.</param>
		/// <returns>An IEnumerable of each (unique) map area.</returns>
		static public IEnumerable<MapArea> MapAreasFor(IMapView<bool> map, AdjacencyRule adjacencyMethod)
		{
			var areaFinder = new MapAreaFinder(map, adjacencyMethod);
			return areaFinder.MapAreas();
		}

		/// <summary>
		/// Calculates the list of map areas, returning each unique map area.
		/// </summary>
		/// <returns>An IEnumerable of each (unique) map area.</returns>
		public IEnumerable<MapArea> MapAreas()
		{
			if (visited == null || visited.GetLength(1) != Map.Height || visited.GetLength(0) != Map.Width)
				visited = new bool[Map.Width, Map.Height];
			else
				Array.Clear(visited, 0, visited.Length);

			for (int x = 0; x < Map.Width; x++)
				for (int y = 0; y < Map.Height; y++)
				{
					var area = visit(new Coord(x, y));

					if (area != null && area.Count != 0)
						yield return area;
				}
		}

		private MapArea visit(Coord position)
		{
			// Don't bother allocating a MapArea, because the starting point isn't valid.
			if (!Map[position])
				return null;

			var stack = new Stack<Coord>();
			var area = new MapArea();
			stack.Push(position);

			while (stack.Count != 0)
			{
				position = stack.Pop();
				if (visited[position.X, position.Y] || !Map[position]) // Already visited, or not part of any mapArea
					continue;

				area.Add(position);
				visited[position.X, position.Y] = true;

				foreach (var c in AdjacencyMethod.Neighbors(position))
				{
					if (c.X < 0 || c.Y < 0 || c.X >= Map.Width || c.Y >= Map.Height) // Out of bounds, thus not actually a neighbor
						continue;

					if (Map[c] && !visited[c.X, c.Y])
						stack.Push(c);
				}
			}

			return area;
		}
	}
}