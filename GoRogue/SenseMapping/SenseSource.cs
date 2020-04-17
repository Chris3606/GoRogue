using GoRogue.MapViews;
using System;
using System.Collections.Generic;
using SadRogue.Primitives;

namespace GoRogue.SenseMapping
{
	/// <summary>
	/// Different types of algorithms that model how source values spread from their source's location.
	/// </summary>
	public enum SourceType
	{
		/// <summary>
		/// Performs calculation by pushing values out from the source location. Source values spread
		/// around corners a bit.
		/// </summary>
		Ripple,

        /// <summary>
        /// Similar to <see cref="Ripple"/> but with different spread mechanics. Values spread around edges like
        /// smoke or water, but maintains a tendency to curl towards the start position as it goes around edges.
        /// </summary>
        RippleLoose,

        /// <summary>
        /// Similar to <see cref="Ripple"/>, but values spread around corners only very slightly.
        /// </summary>
        RippleTight,

        /// <summary>
        /// Similar to <see cref="Ripple"/>, but values spread around corners a lot.
        /// </summary>
        RippleVeryLoose,

		/// <summary>
		/// Uses a Shadowcasting algorithm. All partially resistant grid locations are treated as
		/// being fully transparent (it's on-off blocking, where a value greater than or equal to the
		/// source's <see cref="SenseSource.Intensity"/> in the resistance map blocks, and all lower
		/// values don't).
		/// </summary>
		Shadow
	};

	/// <summary>
	/// Represents a source location to be used in a <see cref="SenseMap"/>. 
	/// </summary>
	/// <remarks>
	/// Typically, you create these, and then call <see cref="SenseMap.AddSenseSource(SenseSource)"/>
	/// to add them to a sensory map, and perhaps retain a reference for the sake of moving it
	/// around or toggling it on-off.  Note that changing values such as <see cref="Position"/> and
	/// <see cref="Radius"/> after the source is created is possible, however changes will not be
	/// reflected in any <see cref="SenseMap"/> instances using this source until their next call
	/// to <see cref="SenseMap.Calculate"/>.
	/// </remarks>
	public class SenseSource
	{
		// Local calculation arrays, internal so SenseMap can easily copy them.
		internal double[,] _light;

		internal bool[,] _nearLight;
		internal IMapView<double>? _resMap;
		private static readonly string[] s_typeWriteVals = Enum.GetNames(typeof(SourceType));
		private double _radius;
		private double _decay; // Set when radius is set

		private int _size;
		private int _halfSize;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="type">The spread mechanics to use for source values.</param>
        /// <param name="position">The position on a map that the source is located at.</param>
        /// <param name="radius">
        /// The maximum radius of the source -- this is the maximum distance the source values will
        /// emanate, provided the area is completely unobstructed.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="Distance"/>, such as <see cref="Radius"/>).
        /// </param>
        /// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
#pragma warning disable CS8618 // Unintialized non-nullable variable for light and nearLight is incorrect, as the Radius setter initializes them.
        public SenseSource(SourceType type, Point position, double radius, Distance distanceCalc, double intensity = 1.0)
#pragma warning restore CS8618
        {
			if (radius <= 0)
				throw new ArgumentOutOfRangeException(nameof(radius), "SenseMap radius cannot be 0");

			if (intensity < 0)
				throw new ArgumentOutOfRangeException(nameof(intensity), "SenseSource intensity cannot be less than 0.0.");

			Type = type;
			Position = position;
			Radius = radius; // Arrays are initialized by this setter
			DistanceCalc = distanceCalc;

			_resMap = null;
			Enabled = true;

			IsAngleRestricted = false;
			Intensity = intensity;
		}

		/// <summary>
		/// Constructor.  Creates a source whose spreading is restricted to a certain angle and span.
		/// </summary>
		/// <param name="type">The spread mechanics to use for source values.</param>
		/// <param name="position">The position on a map that the source is located at.</param>
		/// <param name="radius">
		/// The maximum radius of the source -- this is the maximum distance the source values will
		/// emanate, provided the area is completely unobstructed.
		/// </param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to <see cref="Distance"/>, such as <see cref="Radius"/>).
		/// </param>
		/// <param name="angle">The angle in degrees that specifies the outermost center point of the cone formed
		/// by the source's values. 0 degrees points right.</param>
		/// <param name="span">
		/// The angle, in degrees, that specifies the full arc contained in the cone formed by the source's values --
		/// <paramref name="angle"/> / 2 degrees are included on either side of the cone's center line.
		/// </param>
		/// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
		public SenseSource(SourceType type, Point position, double radius, Distance distanceCalc, double angle, double span, double intensity = 1.0)
			: this(type, position, radius, distanceCalc, intensity)
		{
			if (span < 0.0 || span > 360.0)
				throw new ArgumentOutOfRangeException(nameof(span), "Span used to initialize SenseSource must be in range [0, 360]");

			IsAngleRestricted = true;
			Angle = angle;
			Span = span;
		}

		/// <summary>
		/// Constructor.  Creates a source whose spread is restricted to a certain angle and span.
		/// </summary>
		/// <param name="type">The spread mechanics to use for source values.</param>
		/// <param name="positionX">The x-value for the position on a map that the source is located at.</param>
		/// <param name="positionY">The y-value for the position on a map that the source is located at.</param>
		/// <param name="radius">
		/// The maximum radius of the source -- this is the maximum distance the source values will
		/// emanate, provided the area is completely unobstructed.
		/// </param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to <see cref="Distance"/>, such as <see cref="Radius"/>).
		/// </param>
		/// <param name="angle">The angle in degrees that specifies the outermost center point of the cone formed
		/// by the source's values. 0 degrees points right.</param>
		/// <param name="span">
		/// The angle, in degrees, that specifies the full arc contained in the cone formed by the source's values --
		/// <paramref name="angle"/> / 2 degrees are included on either side of the cone's center line.
		/// </param>
		/// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
		public SenseSource(SourceType type, int positionX, int positionY, double radius, Distance distanceCalc, double angle, double span, double intensity = 1.0)
			: this(type, new Point(positionX, positionY), radius, distanceCalc, angle, span, intensity) { }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">The spread mechanics to use for source values.</param>
		/// <param name="positionX">
		/// The X-value of the position on a map that the source is located at.
		/// </param>
		/// <param name="positionY">
		/// The Y-value of the position on a map that the source is located at.
		/// </param>
		/// <param name="radius">
		/// The maximum radius of the source -- this is the maximum distance the source values will
		/// emanate, provided the area is completely unobstructed.
		/// </param>
		/// <param name="distanceCalc">
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to <see cref="Distance"/>, such as <see cref="Radius"/>).
		/// </param>
		/// <param name="intensity">The starting intensity value of the source. Defaults to 1.0.</param>
		public SenseSource(SourceType type, int positionX, int positionY, double radius, Distance distanceCalc, double intensity = 1.0)
			: this(type, new Point(positionX, positionY), radius, distanceCalc, intensity) { }

		/// <summary>
		/// The distance calculation used to determine what shape the radius has (or a type
		/// implicitly convertible to <see cref="Distance"/>, such as <see cref="SadRogue.Primitives.Radius"/>).
		/// </summary>
		public Distance DistanceCalc { get; set; }

		/// <summary>
		/// Whether or not this source is enabled. If a source is disabled when <see cref="SenseMap.Calculate"/>
		/// is called, the source does not calculate values and is effectively assumed to be "off".
		/// </summary>
		public bool Enabled { get; set; }

        // Analyzer gets this wrong because it's returned by ref
        #pragma warning disable IDE0044
		private Point _position;
        #pragma warning restore IDE0044
        /// <summary>
        /// The position on a map that the source is located at.
        /// </summary>
        public ref Point Position => ref _position;

		/// <summary>
		/// Whether or not the spreading of values from this source is restricted to an angle and span.
		/// </summary>
		public bool IsAngleRestricted { get; set; }

		private double _intensity;
		/// <summary>
		/// The starting value of the source to spread.  Defaults to 1.0.
		/// </summary>
		public double Intensity
		{
			get => _intensity;

			set
			{
				if (value < 0.0)
					throw new ArgumentOutOfRangeException(nameof(Intensity), "Intensity for SenseSource cannot be set to less than 0.0.");

				if (_intensity != value)
				{
					_intensity = value;
					_decay = _intensity / (_radius + 1);
				}
			}
		}

		private double _angle;

		/// <summary>
		/// If <see cref="IsAngleRestricted"/> is true, the angle in degrees that represents a line from the source's start to
		/// the outermost center point of the cone formed by the source's calculated values.  0 degrees points right.
		/// Otherwise, this will be 0.0 degrees.
		/// </summary>
		public double Angle
		{
			get => IsAngleRestricted ? _angle : 0.0;
			set
			{
				if (_angle != value)
					_angle = ((value > 360.0 || value < 0) ? Math.IEEERemainder(value, 360.0) : value);
			}
		}

		private double _span;

		/// <summary>
		/// If <see cref="IsAngleRestricted"/> is true, the angle in degrees that represents the full arc of the cone formed by
		/// the source's calculated values.  Otherwise, it will be 360 degrees.
		/// </summary>
		public double Span
		{
			get => IsAngleRestricted ? _span : 360.0;
			set
			{
				if (value < 0.0 || value > 360.0)
					throw new ArgumentOutOfRangeException(nameof(Span), "SenseSource Span must be in range [0, 360]");

				if (_span != value)
					_span = value;
			}
		}

		/// <summary>
		/// The maximum radius of the source -- this is the maximum distance the source values will
		/// emanate, provided the area is completely unobstructed. Changing this will trigger
		/// resizing (re-allocation) of the underlying arrays.
		/// </summary>
		public double Radius
		{
			get => _radius;
			set
			{
				if (value <= 0.0)
					throw new ArgumentOutOfRangeException(nameof(Radius), "Radius for a SenseSource must be greater than 0.");

				if (_radius != value)
				{
					_radius = Math.Max(1, value);
					// Can round down here because the EUCLIDEAN distance shape is always contained within
					// the CHEBYSHEV distance shape
					_size = (int)_radius * 2 + 1;
					// Any times 2 is even, plus one is odd. rad 3, 3*2 = 6, +1 = 7. 7/2=3, so math works
					_halfSize = _size / 2;
					_light = new double[_size, _size];
					_nearLight = new bool[_size, _size]; // ALlocate whether we use shadow or not, just to support.  Could be lazy but its just bools

					_decay = _intensity / (_radius + 1);
				}
			}
		}

		/// <summary>
		/// The spread mechanics to use for source values.
		/// </summary>
		public SourceType Type { get; set; }

		/// <summary>
		/// Returns a string representation of the configuration of this SenseSource.
		/// </summary>
		/// <returns>A string representation of the configuration of this SenseSource.</returns>
		public override string ToString() => $"Enabled: {Enabled}, Type: {s_typeWriteVals[(int)Type]}, Radius Mode: {(Radius)DistanceCalc}, Position: {Position}, Radius: {Radius}";

		// Set from lighting, just so we have a reference.

		// 2 * Radius + 1 -- the width/height dimension of the local arrays.
		internal void calculateLight()
		{
			if (Enabled)
			{
                if (_resMap == null)
                    throw new InvalidOperationException("Attempted to calculate the light of a sense map without a resistance map.  This is almost certainly a GoRogue bug.");

				initArrays();
				switch (Type)
				{
					case SourceType.Ripple:
					case SourceType.RippleLoose:
					case SourceType.RippleTight:
					case SourceType.RippleVeryLoose:
						if (IsAngleRestricted)
						{
							double angle = _angle * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
							double span = _span * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
							doRippleFOV(rippleValue(Type), _resMap, angle, span);
						}
						else
							doRippleFOV(rippleValue(Type), _resMap);
						break;

					case SourceType.Shadow:
						if (IsAngleRestricted)
						{
							double angle = _angle * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;
							double span = _span * SadRogue.Primitives.MathHelpers.DegreePctOfCircle;

							shadowCastLimited(1, 1.0, 0.0, 0, 1, 1, 0, _resMap, angle, span);
							shadowCastLimited(1, 1.0, 0.0, 1, 0, 0, 1, _resMap, angle, span);

							shadowCastLimited(1, 1.0, 0.0, 0, -1, 1, 0, _resMap, angle, span);
							shadowCastLimited(1, 1.0, 0.0, -1, 0, 0, 1, _resMap, angle, span);

							shadowCastLimited(1, 1.0, 0.0, 0, -1, -1, 0, _resMap, angle, span);
							shadowCastLimited(1, 1.0, 0.0, -1, 0, 0, -1, _resMap, angle, span);

							shadowCastLimited(1, 1.0, 0.0, 0, 1, -1, 0, _resMap, angle, span);
							shadowCastLimited(1, 1.0, 0.0, 1, 0, 0, -1, _resMap, angle, span);
						}
						else
						{
							foreach (Direction d in AdjacencyRule.Diagonals.DirectionsOfNeighbors())
							{
								shadowCast(1, 1.0, 0.0, 0, d.DeltaX, d.DeltaY, 0, _resMap);
								shadowCast(1, 1.0, 0.0, d.DeltaX, 0, 0, d.DeltaY, _resMap);
							}
						}
						break;
				}
			}
		}

		private static int rippleValue(SourceType type)
		{
			switch (type)
			{
				case SourceType.Ripple:
					return 2;

				case SourceType.RippleLoose:
					return 3;

				case SourceType.RippleTight:
					return 1;

				case SourceType.RippleVeryLoose:
					return 6;

				default:
					Console.Error.WriteLine("Unrecognized ripple type, defaulting to RIPPLE...");
					return rippleValue(SourceType.Ripple);
			}
		}

		private void doRippleFOV(int ripple, IMapView<double> map)
		{
			LinkedList<Point> dq = new LinkedList<Point>();
			dq.AddLast(new Point(_halfSize, _halfSize)); // Add starting point
			while (!(dq.Count == 0))
			{
				Point p = dq.First.Value;
				dq.RemoveFirst();

				if (_light[p.X, p.Y] <= 0 || _nearLight[p.X, p.Y])
					continue; // Nothing left to spread!

				foreach (Direction dir in AdjacencyRule.EightWay.DirectionsOfNeighbors())
				{
					int x2 = p.X + dir.DeltaX;
					int y2 = p.Y + dir.DeltaY;
					int globalX2 = Position.X - (int)Radius + x2;
					int globalY2 = Position.Y - (int)Radius + y2;

					if (globalX2 < 0 || globalX2 >= map.Width || globalY2 < 0 || globalY2 >= map.Height || // Bounds check
						DistanceCalc.Calculate(_halfSize, _halfSize, x2, y2) > _radius) // +1 covers starting tile at least
						continue;

					double surroundingLight = nearRippleLight(x2, y2, globalX2, globalY2, ripple, map);
					if (_light[x2, y2] < surroundingLight)
					{
						_light[x2, y2] = surroundingLight;
						if (map[globalX2, globalY2] < _intensity) // Not a wall (fully blocking)
							dq.AddLast(new Point(x2, y2)); // Need to redo neighbors, since we just changed this entry's light.
					}
				}
			}
		}

		private void doRippleFOV(int ripple, IMapView<double> map, double angle, double span)
		{
			LinkedList<Point> dq = new LinkedList<Point>();
			dq.AddLast(new Point(_halfSize, _halfSize)); // Add starting point
			while (!(dq.Count == 0))
			{
				Point p = dq.First.Value;
				dq.RemoveFirst();

				if (_light[p.X, p.Y] <= 0 || _nearLight[p.X, p.Y])
					continue; // Nothing left to spread!

				foreach (Direction dir in AdjacencyRule.EightWay.DirectionsOfNeighborsCounterClockwise(Direction.Right))
				{
					int x2 = p.X + dir.DeltaX;
					int y2 = p.Y + dir.DeltaY;
					int globalX2 = Position.X - (int)Radius + x2;
					int globalY2 = Position.Y - (int)Radius + y2;

					if (globalX2 < 0 || globalX2 >= map.Width || globalY2 < 0 || globalY2 >= map.Height || // Bounds check
						DistanceCalc.Calculate(_halfSize, _halfSize, x2, y2) > _radius) // +1 covers starting tile at least
						continue;

					double at2 = Math.Abs(angle - MathHelpers.ScaledAtan2Approx(y2 - _halfSize, x2 - _halfSize));
					if (at2 > span * 0.5 && at2 < 1.0 - span * 0.5)
						continue;

					double surroundingLight = nearRippleLight(x2, y2, globalX2, globalY2, ripple, map);
					if (_light[x2, y2] < surroundingLight)
					{
						_light[x2, y2] = surroundingLight;
						if (map[globalX2, globalY2] < _intensity) // Not a wall (fully blocking)
							dq.AddLast(new Point(x2, y2)); // Need to redo neighbors, since we just changed this entry's light.
					}
				}
			}
		}

		// Initializes arrays.
		private void initArrays() // Prep for lighting calculations
		{
			Array.Clear(_light, 0, _light.Length);
			_light[_halfSize, _halfSize] = _intensity; // source light is center, starts out at our intensity
			if (Type != SourceType.Shadow) // Only clear if we are using it, since this is called at each calculate
				Array.Clear(_nearLight, 0, _nearLight.Length);
		}

		private double nearRippleLight(int x, int y, int globalX, int globalY, int rippleNeighbors, IMapView<double> map)
		{
			if (x == _halfSize && y == _halfSize)
				return _intensity;

			List<Point> neighbors = new List<Point>();
            double testDistance;
			Point c;

			foreach (Direction di in AdjacencyRule.EightWay.DirectionsOfNeighbors())
			{
				int x2 = x + di.DeltaX;
				int y2 = y + di.DeltaY;
				int globalX2 = Position.X - (int)Radius + x2;
				int globalY2 = Position.Y - (int)Radius + y2;

				if (globalX2 >= 0 && globalX2 < map.Width && globalY2 >= 0 && globalY2 < map.Height)
				{
                    var tmpDistance = DistanceCalc.Calculate(_halfSize, _halfSize, x2, y2);
                    int idx = 0;

					for (int i = 0; i < neighbors.Count && i <= rippleNeighbors; i++)
					{
						c = neighbors[i];
						testDistance = DistanceCalc.Calculate(_halfSize, _halfSize, c.X, c.Y);
						if (tmpDistance < testDistance)
							break;

						idx++;
					}
					neighbors.Insert(idx, new Point(x2, y2));
				}
			}
			if (neighbors.Count == 0)
				return 0;

			neighbors = neighbors.GetRange(0, Math.Min(neighbors.Count, rippleNeighbors));

			double curLight = 0;
			int lit = 0, indirects = 0;
			foreach (Point p in neighbors)
			{
				int gpx = Position.X - (int)Radius + p.X;
				int gpy = Position.Y - (int)Radius + p.Y;
				if (_light[p.X, p.Y] > 0)
				{
					lit++;
					if (_nearLight[p.X, p.Y])
						indirects++;

					double dist = DistanceCalc.Calculate(x, y, p.X, p.Y);
					double resistance = map[gpx, gpy];
					if (gpx == Position.X && gpy == Position.Y)
						resistance = 0.0;

					curLight = Math.Max(curLight, _light[p.X, p.Y] - dist * _decay - resistance);
				}
			}

			if (map[globalX, globalY] >= _intensity || indirects >= lit)
				_nearLight[x, y] = true;

			return curLight;
		}

		private void shadowCast(int row, double start, double end, int xx, int xy, int yx, int yy, IMapView<double> map)
		{
			double newStart = 0;
			if (start < end)
				return;

			bool blocked = false;
			for (int distance = row; distance <= _radius && distance < _size + _size && !blocked; distance++)
			{
				int deltaY = -distance;
				for (int deltaX = -distance; deltaX <= 0; deltaX++)
				{
					int currentX = _halfSize + deltaX * xx + deltaY * xy;
					int currentY = _halfSize + deltaX * yx + deltaY * yy;
					int gCurrentX = Position.X - (int)_radius + currentX;
					int gCurrentY = Position.Y - (int)_radius + currentY;
					double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
					double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

					if (!(gCurrentX >= 0 && gCurrentY >= 0 && gCurrentX < map.Width && gCurrentY < map.Height) || start < rightSlope)
						continue;

					if (end > leftSlope)
						break;

					double deltaRadius = DistanceCalc.Calculate(deltaX, deltaY);
					if (deltaRadius <= _radius)
					{
						double bright = _intensity - _decay * deltaRadius;
						_light[currentX, currentY] = bright;
					}

					if (blocked) // Previous cell was blocked
					{
						if (map[gCurrentX, gCurrentY] >= _intensity) // Hit a wall...
							newStart = rightSlope;
						else
						{
							blocked = false;
							start = newStart;
						}
					}
					else
					{
						if (map[gCurrentX, gCurrentY] >= _intensity && distance < _radius) // Wall within FOV
						{
							blocked = true;
							shadowCast(distance + 1, start, leftSlope, xx, xy, yx, yy, map);
							newStart = rightSlope;
						}
					}
				}
			}
		}

		private void shadowCastLimited(int row, double start, double end, int xx, int xy, int yx, int yy, IMapView<double> map, double angle, double span)
		{
			double newStart = 0;
			if (start < end)
				return;

			bool blocked = false;
			for (int distance = row; distance <= _radius && distance < _size + _size && !blocked; distance++)
			{
				int deltaY = -distance;
				for (int deltaX = -distance; deltaX <= 0; deltaX++)
				{
					int currentX = _halfSize + deltaX * xx + deltaY * xy;
					int currentY = _halfSize + deltaX * yx + deltaY * yy;
					int gCurrentX = Position.X - (int)_radius + currentX;
					int gCurrentY = Position.Y - (int)_radius + currentY;
					double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
					double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

					if (!(gCurrentX >= 0 && gCurrentY >= 0 && gCurrentX < map.Width && gCurrentY < map.Height) || start < rightSlope)
						continue;
					else if (end > leftSlope)
						break;

					double deltaRadius = DistanceCalc.Calculate(deltaX, deltaY);
					double at2 = Math.Abs(angle - MathHelpers.ScaledAtan2Approx(currentY - _halfSize, currentX - _halfSize));

					if (deltaRadius <= _radius && (at2 <= span * 0.5 || at2 >= 1.0 - span * 0.5))
					{
						double bright = _intensity - _decay * deltaRadius;
						_light[currentX, currentY] = bright;
					}

					if (blocked) // Previous cell was blocked
					{
						if (map[gCurrentX, gCurrentY] >= _intensity) // Hit a wall...
							newStart = rightSlope;
						else
						{
							blocked = false;
							start = newStart;
						}
					}
					else if (map[gCurrentX, gCurrentY] >= _intensity && distance < _radius) // Wall within FOV
					{
						blocked = true;
						shadowCastLimited(distance + 1, start, leftSlope, xx, xy, yx, yy, map, angle, span);
						newStart = rightSlope;
					}
				}
			}
		}
	}
}
