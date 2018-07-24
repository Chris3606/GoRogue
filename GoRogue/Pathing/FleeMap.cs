using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.MapViews;

namespace GoRogue.Pathing
{
    public class FleeMap : IMapView<double?>, IDisposable
	{
		private readonly GoalMap _baseMap;

		private ArrayMap<double?> _goalMap;

		public double Magnitude { get; set; }

		public int Height => _goalMap.Height;

		public int Width => _goalMap.Width;

		public double? this[Coord pos] => _goalMap[pos];

		public double? this[int x, int y] => _goalMap[x, y];

		public FleeMap(GoalMap baseMap, double magnitude)
		{
			_baseMap = baseMap ?? throw new ArgumentNullException(nameof(baseMap));
			this.Magnitude = magnitude;
			_goalMap = new ArrayMap<double?>(baseMap.Width, baseMap.Height);
			_baseMap.Updated += this.Update;
		}

		public FleeMap(GoalMap baseMap) : this(baseMap, 1.2)
		{
		}

		private void Update()
		{
			var openSet = new HashSet<Coord>();
			foreach (var point in _baseMap.Walkable) {
				var newPoint = _baseMap[point] * -Magnitude;
				_goalMap[point] = newPoint;
				openSet.Add(point);
			}

			var edgeSet = new HashSet<Coord>();
			var closedSet = new HashSet<Coord>();
			var walkable = new HashSet<Coord>(_baseMap.Walkable);
			while (openSet.Count > 0) //multiple runs are needed to deal with islands
			{
				var min = (double)Width * Height;
				var minPoint = Coord.Get(-1, -1);
				foreach (var point in openSet)
				{
					var value = _goalMap[point];
					if (value < min)
					{
						min = value.Value;
						minPoint = point;
					}
				}
				closedSet.Add(minPoint);
				foreach (var openPoint in ((AdjacencyRule)_baseMap.DistanceMeasurement).Neighbors(minPoint))
				{
					if ((!closedSet.Contains(openPoint)) && walkable.Contains(openPoint))
						edgeSet.Add(openPoint);
				}
					while (edgeSet.Count > 0)
				{
					foreach (var coord in edgeSet.ToArray())
					{
						var current = _goalMap[coord].Value;
						foreach (var openPoint in ((AdjacencyRule)_baseMap.DistanceMeasurement).Neighbors(coord))
						{
							if (closedSet.Contains(openPoint) || !walkable.Contains(openPoint))
								continue;
							var neighborValue = _goalMap[openPoint].Value;
							var newValue = current + _baseMap.DistanceMeasurement.Calculate(coord, openPoint);
							if (newValue < neighborValue)
							{
								_goalMap[openPoint] = newValue;
								edgeSet.Add(openPoint);
							}
						}
						edgeSet.Remove(coord);
						closedSet.Add(coord);
					}
				}
				openSet.RemoveWhere(c => closedSet.Contains(c));
			}
		}

		#region IDisposable Support
		private bool _disposed = false;

		~FleeMap() {
		   Dispose();
		}

		public void Dispose()
		{
			_baseMap.Updated -= this.Update;
			_disposed = true;
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
