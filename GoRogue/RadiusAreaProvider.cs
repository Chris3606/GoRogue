using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Class capable of getting all unique positions inside of a given radius and (optional) bounds.
	/// </summary>
	/// <remarks>
	/// In the case that MANHATTAN/CHEBYSHEV distance, or DIAMOND/SQUARE/OCTAHEDRON/CUBE shapes are
	/// used, Coords are guaranteed to be returned in order of distance from the center, from least
	/// to greatest. This guarantee does NOT hold if EUCLIDEAN distance, or CIRCLE/SPHERE radius
	/// shapes are specified. /// If no bounds are specified, the IEnumerable returned by positions
	/// will contain each coordinate within the radius. Otherwise, it will contain each coordinate in
	/// the radius that is also within the bounds of the rectangle. /// If the same radius length is
	/// being used multiple times (even from different center points), it is recommended to use only
	/// one RadiusAreaProvider, as the class allocates measurable memory, and using only one instance
	/// if the radius is used multiple times prevents reallocation. /// When the Radius value is
	/// changed, reallocation must be performed, however the overhead should be insignificant on
	/// everything but extremely large radiuses.
	/// </remarks>
	public class RadiusAreaProvider : IReadOnlyRadiusAreaProvider
	{
		private Coord _center;

		private int _radius;

		private bool[,] inQueue;

		private Coord topLeft;

		/// <summary>
		/// Constructor. Specifies center, radius length, distance calculation that defines the
		/// concept of radius, and bounds.
		/// </summary>
		/// <param name="center">The center point of the radius.</param>
		/// <param name="radius">The length of the radius.</param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius)..
		/// </param>
		/// <param name="bounds">The bounds to constrain the returned Coords to.</param>
		public RadiusAreaProvider(Coord center, int radius, Distance distanceCalc, Rectangle bounds)
		{
			_center = center;
			_radius = radius;
			this.DistanceCalc = distanceCalc;
			this.Bounds = bounds;
			topLeft = _center - _radius;

			int size = _radius * 2 + 1;
			inQueue = new bool[size, size];
		}

		/// <summary>
		/// Constructor. Specifies center, radius length, distance calculation that defines the
		/// concept of radius, and bounds.
		/// </summary>
		/// <param name="centerX">The x-value of the center point of the radius.</param>
		/// <param name="centerY">The y-value of the center point of the radius.</param>
		/// <param name="radius">The length of the radius.</param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius)..
		/// </param>
		/// <param name="bounds">The bounds to constrain the returned Coords to.</param>
		public RadiusAreaProvider(int centerX, int centerY, int radius, Distance distanceCalc, Rectangle bounds)
			: this(new Coord(centerX, centerY), radius, distanceCalc, bounds) { }

		/// <summary>
		/// Constructor. Specifies center, radius length, and distance calculation that defines the
		/// concept of radius, with no bounds.
		/// </summary>
		/// <param name="center">The center point of the radius.</param>
		/// <param name="radius">The length of the radius.</param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius).
		/// </param>
		public RadiusAreaProvider(Coord center, int radius, Distance distanceCalc)
			: this(center, radius, distanceCalc, Rectangle.EMPTY) { }

		/// <summary>
		/// Constructor. Specifies center, radius length, and distance calculation that defines the
		/// concept of radius, with no bounds.
		/// </summary>
		/// <param name="centerX">The x-value of the center point of the radius.</param>
		/// <param name="centerY">The y-value of the center point of the radius.</param>
		/// <param name="radius">The length of the radius.</param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius).
		/// </param>
		public RadiusAreaProvider(int centerX, int centerY, int radius, Distance distanceCalc)
			: this(new Coord(centerX, centerY), radius, distanceCalc, Rectangle.EMPTY) { }

		/// <summary>
		/// The bounds to constrain the returned Coords to. Set to Rectangle.EMPTY to indicate that
		/// there are no bounds.
		/// </summary>
		public Rectangle Bounds { get; set; }

		/// <summary>
		/// The center point of the radius.
		/// </summary>
		public Coord Center
		{
			get => _center;
			set
			{
				_center = value;
				topLeft = _center - _radius;
			}
		}

		public int CenterX => _center.X;
		public int CenterY => _center.Y;

		/// <summary>
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to Distance, eg. Radius).
		/// </summary>
		public Distance DistanceCalc { get; set; }

		/// <summary>
		/// The length of the radius, eg. the number of tiles from the center point (as defined by the distance
		/// calculation/radius shape given) to which the radius extends.
		/// </summary>
		///<remarks>
		/// When this value is changed, reallocation of an underlying array is performed, however overhead should
		/// be relatively small in most cases.
		/// </remarks>
		public int Radius
		{
			get => _radius;
			set
			{
				if (value != _radius)
				{
					_radius = value;
					topLeft = _center - _radius;
					int size = _radius * 2 + 1;
					inQueue = new bool[size, size];
				}
			}
		}

		/// <summary>
		/// Returns a read-only representation of this RadiusAreaProvider.
		/// </summary>
		/// <returns></returns>
		public IReadOnlyRadiusAreaProvider AsReadOnly() => this;

		/// <summary>
		/// Calculates the new radius, and returns an IEnumerable of all unique Coords within that
		/// radius and bounds specified (as applicable). See class description for details on the ordering.
		/// </summary>
		/// <returns>Enumerable of all unique Coords within the radius and bounds specified.</returns>
		public IEnumerable<Coord> CalculatePositions()
		{
			for (int x = 0; x < inQueue.GetLength(0); x++)
				for (int y = 0; y < inQueue.GetLength(1); y++)
					inQueue[x, y] = false;

			var q = new Queue<Coord>();
			q.Enqueue(_center);
			inQueue[inQueue.GetLength(0) / 2, inQueue.GetLength(0) / 2] = true;

			Coord cur;

			Coord localNeighbor;

			while (q.Count != 0)
			{
				cur = q.Dequeue();
				yield return cur;

				foreach (var neighbor in ((AdjacencyRule)DistanceCalc).Neighbors(cur))
				{
					localNeighbor = neighbor - topLeft;

					if (DistanceCalc.Calculate(_center, neighbor) > _radius || inQueue[localNeighbor.X, localNeighbor.Y] ||
						Bounds != Rectangle.EMPTY && !Bounds.Contains(neighbor))
						continue;

					q.Enqueue(neighbor);
					inQueue[localNeighbor.X, localNeighbor.Y] = true;
				}
			}
		}

		/// <summary>
		/// Returns a string representation of the parameters of the RadiusAreaProvider.
		/// </summary>
		/// <returns>A string representation of the RadiusAreaProvider.</returns>
		public override string ToString()
		{
			string bounds = (Bounds.IsEmpty) ? "None" : Bounds.ToString();
			return $"Center: {_center}, Radius: {Radius}, Distance Measurement: {DistanceCalc}, Bounds: {bounds}";
		}
	}
}