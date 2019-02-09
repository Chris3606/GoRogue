using System;

namespace GoRogue.MapGeneration.Connectors
{
	public class ClosestConnectionPointSelector : IAreaConnectionPointSelector
	{
		public Distance DistanceCalculation { get; }

		public ClosestConnectionPointSelector(Distance distanceCalculation)
		{
			DistanceCalculation = distanceCalculation;
		}

		public Tuple<Coord, Coord> SelectConnectionPoints(IReadOnlyMapArea area1, IReadOnlyMapArea area2)
		{
			Coord c1 = null;
			Coord c2 = null;
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
