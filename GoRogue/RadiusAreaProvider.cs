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
    /// shapes are specified.
    ///
    /// If no bounds are specified, the IEnumerable returned by positions will contain each
    /// coordinate within the radius. Otherwise, it will contain each coordinate in the radius that
    /// is also within the bounds of the rectangle.
    ///
    /// If the same radius length is being used multiple times (even from different center points),
    /// it is recommended to use only one RadiusAreaProvider, as the class allocates measurable
    /// memory, and using only one instance if the radius is used multiple times prevents reallocation.
    ///
    /// When the Radius value is changed, reallocation must be performed, however the overhead should
    /// be insignificant on everything but extremely large radiuses.
    /// </remarks>
    public class RadiusAreaProvider
    {
        /// <summary>
        /// The bounds to constrain the returned Coords to. Set to Rectangle.EMPTY to indicate that
        /// there are no bounds.
        /// </summary>
        public Rectangle Bounds;

        private Coord _center;

        private Distance _distanceCalc;

        private int _radius;

        private bool[,] inQueue;

        private Direction.NeighborsGetter neighbors;

        private Coord topLeft;

        /// <summary>
        /// Constructor. Specifies center, radius, radius shape, and bounds.
        /// </summary>
        /// <param name="center">
        /// The center point of the radius.
        /// </param>
        /// <param name="radius">
        /// The length of the radius.
        /// </param>
        /// <param name="shape">
        /// The shape of the radius.
        /// </param>
        /// <param name="bounds">
        /// The bounds to constrain the returned Coords to.
        /// </param>
        public RadiusAreaProvider(Coord center, int radius, Radius shape, Rectangle bounds)
            : this(center, radius, (Distance)shape, bounds) { }

        /// <summary>
        /// Constructor. Specifies center, radius, and shape, with no bounds.
        /// </summary>
        /// <param name="center">
        /// The center point of the radius.
        /// </param>
        /// <param name="radius">
        /// The length of the radius.
        /// </param>
        /// <param name="shape">
        /// The shape of the radius.
        /// </param>
        public RadiusAreaProvider(Coord center, int radius, Radius shape)
            : this(center, radius, (Distance)shape, Rectangle.EMPTY) { }

        /// <summary>
        /// Constructor. Specifies center, radius length, distance calculation that defines the
        /// concept of radius, and bounds.
        /// </summary>
        /// <param name="center">
        /// The center point of the radius.
        /// </param>
        /// <param name="radius">
        /// The length of the radius.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation that defines the concept of radius.
        /// </param>
        /// <param name="bounds">
        /// The bounds to constrain the returned Coords to.
        /// </param>
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
        /// Constructor. Specifies center, radius length, and distance calculation that defines the
        /// concept of radius, with no bounds.
        /// </summary>
        /// <param name="center">
        /// The center point of the radius.
        /// </param>
        /// <param name="radius">
        /// The length of the radius.
        /// </param>
        /// <param name="distanceCalc">
        /// The distance calculation that defines the concept of radius.
        /// </param>
        public RadiusAreaProvider(Coord center, int radius, Distance distanceCalc)
            : this(center, radius, distanceCalc, Rectangle.EMPTY) { }

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

        /// <summary>
        /// The distance calculation that defines the concept of radius.
        /// </summary>
        public Distance DistanceCalc
        {
            get => _distanceCalc;
            set
            {
                _distanceCalc = value;
                neighbors = Direction.GetNeighbors(value);
            }
        }

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
        /// The shape of the radius.
        /// </summary>
        public Radius RadiusShape { get => (Radius)_distanceCalc; set => DistanceCalc = (Distance)value; }

        /// <summary>
        /// Returns an IEnumerable of all unique Coords within the radius and bounds specified (as
        /// applicable). See class description for details on the ordering.
        /// </summary>
        /// <returns>
        /// Enumerable of all unique Coords within the radius and bounds specified.
        /// </returns>
        public IEnumerable<Coord> Positions()
        {
            for (int x = 0; x < inQueue.GetLength(0); x++)
                for (int y = 0; y < inQueue.GetLength(1); y++)
                    inQueue[x, y] = false;

            var q = new Queue<Coord>();
            q.Enqueue(_center);
            inQueue[inQueue.GetLength(0) / 2, inQueue.GetLength(0) / 2] = true;

            Coord cur;
            Coord neighbor;

            Coord localNeighbor;

            while (q.Count != 0)
            {
                cur = q.Dequeue();
                yield return cur;

                foreach (var dir in neighbors())
                {
                    neighbor = cur + dir;
                    localNeighbor = neighbor - topLeft;

                    if (_distanceCalc.DistanceBetween(_center, neighbor) > _radius || inQueue[localNeighbor.X, localNeighbor.Y] ||
                        Bounds != Rectangle.EMPTY && !Bounds.Contains(neighbor))
                        continue;

                    q.Enqueue(neighbor);
                    inQueue[localNeighbor.X, localNeighbor.Y] = true;
                }
            }
        }
    }
}