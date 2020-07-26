using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A region of the map with four sides of arbitrary shape and size
    /// </summary>
    [PublicAPI]
    public partial class Region
    {
        #region generation
        /// <summary>
        /// A region of the map with four corners of arbitrary shape and size
        /// </summary>
        /// <param name="name">the name of this region</param>
        /// <param name="se">the South-East corner of this region</param>
        /// <param name="ne">the North-East corner of this region</param>
        /// <param name="nw">the North-West corner of this region</param>
        /// <param name="sw">the South-West corner of this region</param>
        public Region(string name, Point se, Point ne, Point nw, Point sw)
        {
            Name = name;
            Generate(se, ne, nw, sw);
        }

        /// <summary>
        /// A private function for generating the values of the region based on corners.
        /// </summary>
        /// <param name="se">South-East corner</param>
        /// <param name="ne">North-East corner</param>
        /// <param name="nw">North-West corner</param>
        /// <param name="sw">South-West corner</param>
        /// <remarks>This will destroy any Connections that the region currently has.</remarks>
        private void Generate(Point se, Point ne, Point nw, Point sw)
        {
            SouthEastCorner = se;
            NorthEastCorner = ne;
            NorthWestCorner = nw;
            SouthWestCorner = sw;

            _westBoundary = new Area(Lines.Get(NorthWestCorner, SouthWestCorner));
            _southBoundary = new Area(Lines.Get(SouthWestCorner, SouthEastCorner));
            _eastBoundary = new Area(Lines.Get(SouthEastCorner, NorthEastCorner));
            _northBoundary = new Area(Lines.Get(NorthEastCorner, NorthWestCorner));

            Rise = se.Y - ne.Y;
            Run = se.X - sw.X;
            _outerPoints = new Area(_westBoundary.Concat(_eastBoundary).Concat(_southBoundary).Concat(_northBoundary).Distinct());
            _innerPoints = new Area(InnerFromOuterPoints(OuterPoints).Distinct());
        }
        #endregion

        #region overrides
        /// <summary>
        /// Returns a string detailing the region's corner locations.
        /// </summary>
        /// <returns/>
        public override string ToString()
        {
            return Name + ": NW" + NorthWestCorner +"=> NE" + NorthEastCorner + "=> SE" + SouthEastCorner + "=> SW" + SouthWestCorner;
        }

        /// <summary>
        /// True if the given object is a region that has the same corner/centers; false otherwise.
        /// </summary>
        /// <param name="obj"/>
        /// <returns/>
        public override bool Equals(object obj) => obj is Region region && this == region;

        /// <summary>
        /// To facilitate in equality operations
        /// </summary>
        /// <returns>The Hash Code of the Region</returns>
        /// <remarks>
        /// Due to the nature of Connections being personal to the map generation algorithm,
        /// connections are not considered when evaluating a region's Hash Code.
        /// </remarks>
        public override int GetHashCode()
        {
            return SouthEastCorner.GetHashCode() + NorthEastCorner.GetHashCode() + SouthWestCorner.GetHashCode() +
                       NorthWestCorner.GetHashCode() + Center.GetHashCode();
        }

        #endregion

        #region management
        /// <summary>
        /// Gets all Points that overlap with another region
        /// </summary>
        /// <param name="other">The region to evaluate against</param>
        /// <returns>All overlapping points</returns>
        public IEnumerable<Point> Overlap(Region other)
        {
            foreach (Point c in Points)
            {
                if (other.Contains(c))
                    yield return c;
            }
        }

        /// <summary>
        /// Whether or not this region contains a point
        /// </summary>
        /// <param name="here">The Point to evaluate</param>
        /// <returns>If this region contains the given point</returns>
        public bool Contains(Point here)
        {
            return new Area(Points).Contains(here);
        }
        /// <summary>
        /// Is this Point one of the corners of the Region?
        /// </summary>
        /// <param name="here">the point to evaluate</param>
        /// <returns>whether or not the point is within the region</returns>
        public bool IsCorner(Point here)
        {
            return here == NorthEastCorner || here == NorthWestCorner || here == SouthEastCorner || here == SouthWestCorner;
        }
        #endregion

        #region transform
        /// <summary>
        /// Shifts the entire region by performing Point addition
        /// </summary>
        /// <param name="distance">the distance and direction to shift the region</param>
        /// <returns>This region shifted by the distance</returns>
        public Region Shift(Point distance)
        {
            Region region = new Region(Name, SouthEastCorner + distance, NorthEastCorner + distance, NorthWestCorner + distance, SouthWestCorner + distance);
            foreach (var subRegion in SubRegions)
            {
                ((List<Region>) region.SubRegions).Add(subRegion);
            }
            return region;
        }
        /// <summary>
        /// Cuts Points out of a parent region if that point exists in a subregion,
        /// and cuts Points out of subregions if that point is already in a subregion
        /// </summary>
        /// <remarks>
        /// This favors regions that were added later, like a stack.
        /// </remarks>
        public void DistinguishSubRegions()
        {
            List<Region> regionsDistinguished = new List<Region>();
            List<Region> regionsReversed = SubRegions.ToList();
            regionsReversed.Reverse();
            foreach (Region region in regionsReversed)
            {
                List<Point> removeThese = new List<Point>();
                foreach (Point point in region.Points.Distinct())
                {
                    foreach (Region distinguishedRegion in regionsDistinguished)
                    {
                        if (distinguishedRegion.Contains(point))
                        {
                            if (region.Contains(point))
                                removeThese.Add(point);
                        }
                    }
                }

                foreach (var point in removeThese)
                {
                    region.Remove(point);
                }
                regionsDistinguished.Add(region);
            }
        }

        /// <summary>
        /// Removes a point from the region.
        /// </summary>
        /// <param name="point">The point to remove.</param>
        /// <remarks>
        /// This removes a point from the inner, outer points, and connections. Does not remove a point
        /// from a boundary, since those are generated from the corners.
        /// </remarks>
        public void Remove(in Point point)
        {
            while (_outerPoints.Contains(point))
                _outerPoints.Remove(point);
            while (_innerPoints.Contains(point))
                _innerPoints.Remove(point);
            while (_connections.Contains(point))
                _connections.Remove(point);
        }

        /// <summary>
        /// Does this region have a sub-region with a certain name?
        /// </summary>
        /// <param name="name">name of the region to analyze</param>
        /// <returns>whether the specified region exists</returns>
        public bool HasRegion(string name) => SubRegions.Any(r => r.Name == name);

        /// <summary>
        /// Removes a common point from the OuterPoints of both regions
        /// and adds it to the Connections of both Regions
        /// </summary>
        /// <param name="a">the first region to evaluate</param>
        /// <param name="b">the second region to evaluate</param>
        /// <param name="rng">The RNG to use.  Defaults to <see cref="GlobalRandom.DefaultRNG"/>.</param>
        public static void AddConnectionBetween(Region a, Region b, IGenerator? rng = null)
        {
            rng ??= GlobalRandom.DefaultRNG;

            List<Point> possible =  a.OuterPoints.Where(here => b.OuterPoints.Contains(here) && !a.IsCorner(here) && !b.IsCorner(here)).ToList();

            if (possible.Count <= 2)
                throw new ArgumentException("The two proposed regions have no overlapping points.");

            possible.RemoveAt(0);
            possible.RemoveAt(possible.Count - 1);
            var connection = possible.RandomItem(rng);
            a.AddConnection(connection);
            b.AddConnection(connection);
        }

        /// <summary>
        /// Adds a new connection to this region
        /// </summary>
        /// <param name="connection">The location of the connection</param>
        public void AddConnection(Point connection)
        {
            Remove(connection);
            _connections.Add(connection);
        }

        /// <summary>
        /// Removes (from this region) the outer points shared in common
        /// with the imposing region
        /// </summary>
        /// <param name="imposing">the region to check for common outer points</param>
        public void RemoveOverlappingOuterPoints(Region imposing)
        {
            foreach (Point c in imposing.OuterPoints)
            {
                while (_outerPoints.Contains(c))
                    _outerPoints.Remove(c);
                while (_innerPoints.Contains(c))
                    _innerPoints.Remove(c);
            }
        }

        /// <summary>
        /// Removes (from this region) the inner points shared in common
        /// with the imposing region
        /// </summary>
        /// <param name="imposing">the region to check for common outer points</param>
        public void RemoveOverlappingInnerPoints(Region imposing)
        {
            foreach (Point c in imposing.InnerPoints)
            {
                while (_outerPoints.Contains(c))
                    _outerPoints.Remove(c);
                while (_innerPoints.Contains(c))
                    _innerPoints.Remove(c);
            }
        }
        /// <summary>
        /// Removes (from this region) the outer points shared in common
        /// with the imposing region
        /// </summary>
        /// <param name="imposing">the region to check for common outer points</param>
        public void RemoveOverlappingPoints(Region imposing)
        {
            foreach (Point c in imposing.Points)
            {
                while (_outerPoints.Contains(c))
                    _outerPoints.Remove(c);
                while (_innerPoints.Contains(c))
                    _innerPoints.Remove(c);
            }
        }

        /// <summary>
        /// Rotates a region around it's center
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <param name="doToSelf">Whether to perform this transformation on itself, or return a new region.</param>
        /// <returns>A region equal to the original region rotated by the given degree</returns>
        public virtual Region Rotate(double degrees, bool doToSelf)
        {
            Point origin = new Point((Left + Right) / 2, (Top + Bottom) / 2);
            return Rotate(degrees, doToSelf, origin);
        }

        /// <summary>
        /// Rotates this region by an arbitrary number of degrees
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <param name="doToSelf">Whether to perform this rotation on this region, or return a new region</param>
        /// <param name="origin">The Point around which to rotate</param>
        /// <returns>This region, rotated</returns>
        /// <remarks>
        /// This is destructive to the region's Connections, so you should try to refrain from generating those
        /// until after you've performed your rotations.
        /// </remarks>
        public virtual Region Rotate(double degrees, bool doToSelf, Point origin)
        {
            degrees = MathHelpers.WrapAround(degrees, 360);
            double radians = SadRogue.Primitives.MathHelpers.ToRadian(degrees);
            List<Point> corners = new List<Point>();


            Point sw = RotatePoint(SouthWestCorner - origin, radians) + origin;
            corners.Add(sw);

            Point se = RotatePoint(SouthEastCorner - origin, radians) + origin;
            corners.Add(se);

            Point nw = RotatePoint(NorthWestCorner - origin, radians) + origin;
            corners.Add(nw);

            Point ne = RotatePoint(NorthEastCorner - origin, radians) + origin;
            corners.Add(ne);

            corners = corners.OrderBy(corner => Direction.YIncreasesUpward ? -corner.Y : corner.Y).ToList();
            Point[] topTwo = new Point[2] { corners[0], corners[1] };
            Point[] bottomTwo = new Point[2] {corners [2], corners [3] };

            sw = bottomTwo.OrderBy(c => c.X).ToArray()[0];
            se = bottomTwo.OrderBy(c => -c.X).ToArray()[1];

            nw = topTwo.OrderBy(c => c.X).ToArray()[0];
            ne = corners.OrderBy(c => -c.X).ToArray()[1];

            if (doToSelf)
            {
                Generate(se, ne, nw, sw);

                foreach (Region subRegion in SubRegions)
                {
                    subRegion.Rotate(degrees, true, origin);
                }
                return this;
            }
            else
                return new Region(Name, se, ne, nw, sw);
        }

        /// <summary>
        /// Rotates a single point around the origin (0, 0).
        /// </summary>
        /// <param name="point">The Point to rotate</param>
        /// <param name="radians">The amount of Radians to rotate this point</param>
        /// <returns>The equivalnt point after a rotation</returns>
        /// <remarks>
        /// This is intended only as a helper class for rotation, and not for general use.
        /// Intended usage is like so:
        /// `Point sw = RotatePoint(SouthWestCorner - origin, radians) + origin;`
        /// </remarks>
        private Point RotatePoint(Point point, in double radians)
        {
            int x = (int)Math.Round(point.X * Math.Cos(radians) - point.Y * Math.Sin(radians));
            int y = (int)Math.Round(point.X * Math.Sin(radians) + point.Y * Math.Cos(radians));
            return new Point(x, y);
        }

        #endregion

        #region subregions
        /// <summary>
        /// Adds a sub-region to this region.
        /// </summary>
        /// <param name="region">The region you wish to add as a sub-region</param>
        public void Add(Region region)
        {
            _subRegions.Add(region);
        }
        /// <summary>
        /// Removes a sub-region whose name matches with the string given.
        /// </summary>
        /// <param name="name">Key of sub-region to remove.</param>
        public void Remove(string name)
        {
            if (HasRegion(name))
                _subRegions.Remove(GetRegion(name));
        }

        /// <summary>
        /// Get a sub-region by its name.
        /// </summary>
        /// <param name="name">The name of the region to find</param>
        public Region GetRegion(string name)
        {
            if (HasRegion(name))
                return SubRegions.First(r => r.Name == name);
            else
                throw new KeyNotFoundException("No sub-region named " + name + " was found");
        }
        #endregion
    }
}
