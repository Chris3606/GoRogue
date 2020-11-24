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
    public partial class Region : IMatchable<Region>
    {
        #region Generation

        /// <summary>
        /// A region of the map with four corners of arbitrary shape and size
        /// </summary>
        /// <param name="name">the name of this region</param>
        /// <param name="northWest">the North-West corner of this region</param>
        /// <param name="northEast">the North-East corner of this region</param>
        /// <param name="southEast">the South-East corner of this region</param>
        /// <param name="southWest">the South-West corner of this region</param>
        public Region(string name, Point northWest, Point northEast, Point southEast, Point southWest)
        {
            Name = name;
            SouthEastCorner = southEast;
            NorthEastCorner = northEast;
            NorthWestCorner = northWest;
            SouthWestCorner = southWest;

            _westBoundary = new Area(Lines.Get(NorthWestCorner, SouthWestCorner));
            _southBoundary = new Area(Lines.Get(SouthWestCorner, SouthEastCorner));
            _eastBoundary = new Area(Lines.Get(SouthEastCorner, NorthEastCorner));
            _northBoundary = new Area(Lines.Get(NorthEastCorner, NorthWestCorner));
            _outerPoints = new Area(_westBoundary.Concat(_eastBoundary).Concat(_southBoundary).Concat(_northBoundary));
            _innerPoints = InnerFromOuterPoints(_outerPoints);
        }
        #endregion

        #region Overrides

        /// <summary>
        /// Returns whether or not this region is completely overlapping with the other region
        /// </summary>
        /// <param name="other">the region against which to compare</param>
        /// <returns></returns>
        public bool Matches(Region? other)
        {
            if (other is null) return false;
            if (Name != other.Name) return false;
            if (NorthWestCorner != other.NorthWestCorner) return false;
            if (SouthWestCorner != other.SouthWestCorner) return false;
            if (NorthEastCorner != other.NorthEastCorner) return false;
            if (SouthEastCorner != other.SouthEastCorner) return false;
            if (Center != other.Center) return false;

            return true;
        }

        /// <summary>
        /// Returns a string detailing the region's corner locations.
        /// </summary>
        /// <returns/>
        public override string ToString()
            => $"{Name}: NW{NorthWestCorner.ToString()}=> NE{NorthEastCorner.ToString()}=> SE{SouthEastCorner.ToString()}=> SW{SouthWestCorner.ToString()}";
        #endregion

        #region Management
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
        public bool Contains(Point here) =>
            _outerPoints.Contains(here) || _innerPoints.Contains(here) || _connections.Contains(here);

        /// <summary>
        /// Is this Point one of the corners of the Region?
        /// </summary>
        /// <param name="here">the point to evaluate</param>
        /// <returns>whether or not the point is within the region</returns>
        public bool IsCorner(Point here) =>
            here == NorthEastCorner || here == NorthWestCorner || here == SouthEastCorner || here == SouthWestCorner;

        #endregion

        #region Transform
        /// <summary>
        /// Shifts the entire region by performing Point addition
        /// </summary>
        /// <param name="distance">the distance and direction to shift the region</param>
        /// <returns>This region shifted by the distance</returns>
        public Region Shift(Point distance)
        {
            Region region = new Region(Name, NorthWestCorner + distance, NorthEastCorner + distance, SouthEastCorner + distance, SouthWestCorner + distance);
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
        public bool HasSubRegion(string name) => SubRegions.Any(r => r.Name == name);

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

            List<Point> possible = GetPossibleConnections(a, b).ToList();

            if (possible.Count <= 2)
                throw new ArgumentException("The two proposed regions have no overlapping points.");

            possible.RemoveAt(0);
            possible.RemoveAt(possible.Count - 1);
            var connection = possible.RandomItem(rng);
            a.AddConnection(connection);
            b.AddConnection(connection);
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{Point}"/> of all the OuterPoints shared by the left and right regions
        /// </summary>
        /// <param name="left">the first region to analyze</param>
        /// <param name="right">the second region to analyze</param>
        /// <returns></returns>
        public static IEnumerable<Point> GetPossibleConnections(Region left, Region right) =>
            left._outerPoints.Where(here => right._outerPoints.Contains(here) && !left.IsCorner(here) && !right.IsCorner(here));


        /// <summary>
        /// Adds a new connection to this region
        /// </summary>
        /// <param name="connection">The location of the connection</param>
        public void AddConnection(Point connection)
        {
            if(!Contains(connection))
                throw new ArgumentException("Connection must be within the region");

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
            foreach (Point c in imposing._outerPoints)
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
            foreach (Point c in imposing._innerPoints)
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
        /// Rotates a region around it's center.
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <returns>A region equal to the original region rotated by the given degree</returns>
        public virtual Region Rotate(double degrees) => Rotate(degrees, Center);

        /// <summary>
        /// Rotates this region by an arbitrary number of degrees
        /// </summary>
        /// <param name="degrees">The amount of degrees to rotate this region</param>
        /// <param name="origin">The Point around which to rotate</param>
        /// <returns>This region, rotated.</returns>
        /// <remarks>
        /// This is destructive to the region's <see cref="Connections"/>, so you should try to refrain from generating
        /// those until after you've performed your rotations.
        /// </remarks>
        public virtual Region Rotate(double degrees, Point origin)
        {
            degrees = MathHelpers.WrapAround(degrees, 360);

            //figure out the new corners post-rotation
            List<Point> corners = new List<Point>();
            Point southwest = SouthWestCorner.Rotate(degrees, origin);
            corners.Add(southwest);
            Point southeast = SouthEastCorner.Rotate(degrees, origin);
            corners.Add(southeast);
            Point northwest = NorthWestCorner.Rotate(degrees, origin);
            corners.Add(northwest);
            Point northeast = NorthEastCorner.Rotate(degrees, origin);
            corners.Add(northeast);

            //order the new corner by Y-value
            corners = corners.OrderBy(corner => Direction.YIncreasesUpward ? -corner.Y : corner.Y).ToList();

            //split that list in half and then sort by X-value
            Point[] topTwo = { corners[0], corners[1] };
            topTwo = topTwo.OrderBy(c => c.X).ToArray();
            northwest = topTwo[0];
            northeast = topTwo[1];

            Point[] bottomTwo = { corners [2], corners [3] };
            bottomTwo = bottomTwo.OrderBy(c => c.X).ToArray();
            southwest = bottomTwo[0];
            southeast = bottomTwo[1];

            return new Region(Name, northwest, northeast, southeast, southwest);
        }

        #endregion

        #region SubRegions
        /// <summary>
        /// Adds a sub-region to this region.
        /// </summary>
        /// <param name="region">The region you wish to add as a sub-region</param>
        public void AddSubRegion(Region region)
        {
            _subRegions.Add(region);
        }
        /// <summary>
        /// Removes a sub-region whose name matches with the string given.
        /// </summary>
        /// <param name="name">Key of sub-region to remove.</param>
        public void RemoveSubRegion(string name)
        {
            if (HasSubRegion(name))
                _subRegions.Remove(GetSubRegion(name));
        }

        /// <summary>
        /// Get a sub-region by its name.
        /// </summary>
        /// <param name="name">The name of the region to find</param>
        public Region GetSubRegion(string name)
        {
            if (HasSubRegion(name))
                return SubRegions.First(r => r.Name == name);
            else
                throw new KeyNotFoundException("No sub-region named " + name + " was found");
        }
        #endregion
    }
}
