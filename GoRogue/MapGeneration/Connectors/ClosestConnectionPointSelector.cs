using System;

namespace GoRogue.MapGeneration.Connectors
{
	/// <summary>
	/// Point selector that selects the two points from the MapAreas that are closest to each other
	/// according to the given distance calculation.
	/// </summary>
	public class ClosestConnectionPointSelector : IAreaConnectionPointSelector
	{
		/// <summary>
		/// Distance calculation to use to determine closeness.
		/// </summary>
		public Distance DistanceCalculation { get; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="distanceCalculation">Distance calculation to use to determine closeness.</param>
		public ClosestConnectionPointSelector(Distance distanceCalculation)
		{
			DistanceCalculation = distanceCalculation;
		}

		/// <summary>
		/// Determines the two points in the given map areas that are closest to each other, and returns
		/// them as a tuple of two Coords.  The first Coord is the position in area1 to use, the second
		/// Coord is the position in area2 to use.
		/// </summary>
		/// <param name="area1">First MapArea to connect.</param>
		/// <param name="area2">Second MapArea to connect.</param>
		/// <returns>
		/// A tuple containing the coordinates from each MapArea to connect -- the first item in the
		/// tuple is the Coord in area1, the second is the Coord in area2.
		/// </returns>
		public Tuple<Coord, Coord> SelectConnectionPoints(IReadOnlyMapArea area1, IReadOnlyMapArea area2)
		{
			Coord c1 = Coord.NONE;
			Coord c2 = Coord.NONE;
			double minDist = double.MaxValue;

			foreach (var point1 in area1.Positions)
				foreach (var point2 in area2.Positions)
				{
					double distance = DistanceCalculation.Calculate(point1, point2);
					if (distance < minDist)
					{
						c1 = point1;
						c2 = point2;
						minDist = distance;
					}
				}

			return new Tuple<Coord, Coord>(c1, c2);
		}
	}
}
