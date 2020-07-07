using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using GoRogue.Random;
using SadRogue.Primitives;
using Troschuetz.Random;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A region of the map with four corners of arbitrary shape and size
    /// </summary>
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
        /// A private constructor for region that is only used internally
        /// </summary>
        private Region() { }

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
            _connections = new List<Point>();
            _westBoundary = Lines.Get(NorthWestCorner, SouthWestCorner).ToList();
            _southBoundary = Lines.Get(SouthWestCorner, SouthEastCorner).ToList();
            _eastBoundary = Lines.Get(SouthEastCorner, NorthEastCorner).ToList();
            _northBoundary = Lines.Get(NorthEastCorner, NorthWestCorner).ToList();

            Rise = se.Y - ne.Y;
            Run = se.X - sw.X;
            _outerPoints = _westBoundary.Concat(_eastBoundary).Concat(_southBoundary).Concat(_northBoundary).Distinct().ToList();
            _innerPoints = InnerFromOuterPoints(OuterPoints).Distinct().ToList();
        }
        #endregion

        #region overrides
        public override string ToString()
        {
            return Name + ": NW" + NorthWestCorner +"=> NE" + NorthEastCorner + "=> SE" + SouthEastCorner + "=> SW" + SouthWestCorner;
        }
        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(Region))
                return false;
            else
                return this == (Region)obj;
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
            Area you = new Area(other.Points);

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
        /// Does this region have a subregion with a certain name?
        /// </summary>
        /// <param name="name">name of the region to analyze</param>
        /// <returns>whether the specified region exists</returns>
        public bool HasRegion(string name) => SubRegions.Where(r => r.Name == name).Count() > 0;

        /// <summary>
        /// Removes a common point from the OuterPoints of both regions
        /// and adds it to the Connections of both Regions
        /// </summary>
        /// <param name="a">the first region to evaluate</param>
        /// <param name="b">the second region to evaluate</param>
        public static void AddConnectionBetween(Region a, Region b, IGenerator rng = null)
        {
            rng = rng ?? GlobalRandom.DefaultRNG;
            List<Point> possible =  a.OuterPoints.Where(here => b.OuterPoints.Contains(here) && !a.IsCorner(here) && !b.IsCorner(here)).ToList();

            if (possible.Count <= 2)
                throw new ArgumentException("The two proposed regions have no overlapping points.");
            possible.RemoveAt(0);
            possible.RemoveAt(possible.Count() - 1);
            int connectionIndex = rng.Next(0, possible.Count);
            Point connection = possible[connectionIndex];
            a.AddConnection(connection);
            b.AddConnection(connection);
        }

        /// <summary>
        /// Ads a new connection to this region
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
        public virtual Region Rotate(float degrees, bool doToSelf, Point origin = default)
        {
            int quarterTurns = 0;
            double radians = SadRogue.Primitives.MathHelpers.ToRadian(degrees);

            List<Point> corners = new List<Point>();
            if (origin == default(Point))
                origin = new Point((Left + Right) / 2, (Top + Bottom) / 2);

            degrees = MathHelpers.WrapAround((int) degrees, 360);

            Point sw = SouthWestCorner - origin;
            int x = (int) Math.Round(sw.X * Math.Cos(radians) - sw.Y * Math.Sin(radians));
            int y = (int) Math.Round(sw.X * Math.Sin(radians) + sw.Y * Math.Cos(radians));
            corners.Add(new Point(x, y) + origin);

            Point se = SouthEastCorner - origin;
            x = (int) Math.Round(se.X * Math.Cos(radians) - se.Y * Math.Sin(radians));
            y = (int) Math.Round(se.X * Math.Sin(radians) + se.Y * Math.Cos(radians));
            corners.Add(new Point(x, y) + origin);

            Point nw = NorthWestCorner - origin;
            x = (int) Math.Round(nw.X * Math.Cos(radians) - nw.Y * Math.Sin(radians));
            y = (int) Math.Round(nw.X * Math.Sin(radians) + nw.Y * Math.Cos(radians));
            corners.Add(new Point(x, y) + origin);

            Point ne = NorthEastCorner - origin;
            x = (int) Math.Round(ne.X * Math.Cos(radians) - ne.Y * Math.Sin(radians));
            y = (int) Math.Round(ne.X * Math.Sin(radians) + ne.Y * Math.Cos(radians));
            corners.Add(new Point(x, y) + origin);

            sw = corners.OrderBy(c => -c.Y).ThenBy(c => c.X).First();
            corners.Remove(sw);

            se = corners.OrderBy(c => -c.Y).First();
            corners.Remove(se);

            nw = corners.OrderBy(c => c.X).First();
            corners.Remove(nw);

            ne = corners.OrderBy(c => -c.X).First();

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
        #endregion

        #region subregions
        /// <summary>
        /// Adds a subregion to this region
        /// </summary>
        /// <param name="region">The region you wish to add as a subregion</param>
        public void Add(Region region)
        {
            _subRegions.Add(region);
        }
        /// <summary>
        /// removes a subregion whose name matches with the key
        /// </summary>
        /// <param name="name"></param>
        public void Remove(string key)
        {
            if (HasRegion(key))
                _subRegions.Remove(GetRegion(key));
        }

        /// <summary>
        /// Get a SubRegion by its name
        /// </summary>
        /// <param name="name">the name of the region to find</param>
        public Region GetRegion(string name)
        {
            if (HasRegion(name))
                return SubRegions.First(r => r.Name == name);
            else
                throw new KeyNotFoundException("No SubRegion named " + name + " was found");
        }
        #endregion
    }
}
