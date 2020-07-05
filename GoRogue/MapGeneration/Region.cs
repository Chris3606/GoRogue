using System;
using System.Collections.Generic;
using System.Linq;
using SadRogue.Primitives;

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
            Connections = new List<Point>();
            WestBoundary = Lines.Get(NorthWestCorner, SouthWestCorner);
            SouthBoundary = Lines.Get(SouthWestCorner, SouthEastCorner);
            EastBoundary = Lines.Get(SouthEastCorner, NorthEastCorner);
            NorthBoundary = Lines.Get(NorthEastCorner, NorthWestCorner);

            Rise = se.Y - ne.Y;
            Run = se.X - sw.X;
            OuterPoints = WestBoundary.Concat(EastBoundary).Concat(SouthBoundary).Concat(NorthBoundary);
            OuterPoints = OuterPoints.Distinct().ToList();
            InnerPoints = InnerFromOuterPoints(OuterPoints).Distinct().ToList();
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
        public static bool operator ==(Region left, Region right)
        {
            bool equals = left.Name == right.Name;
            if (left.NorthWestCorner != right.NorthWestCorner) equals = false;
            if (left.SouthWestCorner != right.SouthWestCorner) equals = false;
            if (left.NorthEastCorner != right.NorthEastCorner) equals = false;
            if (left.SouthEastCorner != right.SouthEastCorner) equals = false;
            if (left.Center != right.Center) equals = false;
            return equals;
        }
        public static bool operator !=(Region left, Region right)
        {
            return !(left == right);
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
            foreach (Point c in InnerPoints)
            {
                if (other.InnerPoints.Contains(c))
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
            return InnerPoints.Contains(here);
        }
        /// <summary>
        /// Is this Point one of the corners of the Region?
        /// </summary>
        /// <param name="here">the point to evaluate</param>
        /// <returns>whether or not the point is within the region</returns>
        private bool IsCorner(Point here)
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
            IEnumerable<Region> regionsReversed = SubRegions;
            regionsReversed.Reverse();
            foreach (Region region in regionsReversed)
            {
                foreach (Point point in region.Points.Distinct())
                {
                    foreach (Region distinguishedRegion in regionsDistinguished)
                    {
                        if (distinguishedRegion.Contains(point))
                        {
                            while (region.OuterPoints.Contains(point))
                                ((List<Point>) region.OuterPoints).Remove(point);

                            while (region.InnerPoints.Contains(point))
                                ((List<Point>) region.InnerPoints).Remove(point);
                        }
                    }
                }

                regionsDistinguished.Add(region);
            }
        }
        /// <summary>
        /// Removes a common point from the OuterPoints of both regions
        /// and adds it to the Connections of both Regions
        /// </summary>
        /// <param name="a">the first region to evaluate</param>
        /// <param name="b">the second region to evaluate</param>
        public static void AddConnectionBetween(Region a, Region b)
        {
            List<Point> possible = new List<Point>();
            List<Point> Points = a.OuterPoints.Where(here => b.OuterPoints.Contains(here) && !a.IsCorner(here) && !b.IsCorner(here)).ToList();
            foreach (Point Point in Points)
            {
                possible.Add(Point);
            }
            if (possible.Count() <= 2) return;
            possible.Remove(possible.First());
            possible.Remove(possible.Last());

            Point connection = possible.RandomItem();

            ((List<Point>)a.OuterPoints).Remove(connection);
            ((List<Point>)a.Connections).Add(connection);
            ((List<Point>)b.OuterPoints).Remove(connection);
            ((List<Point>)b.Connections).Add(connection);
        }

        /// <summary>
        /// Removes (from this region) the outer points shared in common
        /// with the imposing region
        /// </summary>
        /// <param name="imposing">the region to check for common outer points</param>
        public void RemoveOverlappingOuterpoints(Region imposing)
        {
            foreach (Point c in imposing.OuterPoints)
            {
                while (OuterPoints.Contains(c))
                    ((List<Point>)OuterPoints).Remove(c);
                while (InnerPoints.Contains(c))
                    ((List<Point>) InnerPoints).Remove(c);
            }
        }

        /// <summary>
        /// Removes (from this region) the inner points shared in common
        /// with the imposing region
        /// </summary>
        /// <param name="imposing">the region to check for common outer points</param>
        public void RemoveOverlappingInnerpoints(Region imposing)
        {
            foreach (Point c in imposing.InnerPoints)
            {
                while (OuterPoints.Contains(c))
                    ((List<Point>) OuterPoints).Remove(c);
                while (InnerPoints.Contains(c))
                    ((List<Point>) InnerPoints).Remove(c);
            }
        }
        /// <summary>
        /// Removes (from this region) the outer points shared in common
        /// with the imposing region
        /// </summary>
        /// <param name="imposing">the region to check for common outer points</param>
        public void RemoveOverlappingPoints(Region imposing)
        {
            foreach (Point c in imposing.OuterPoints.Concat(imposing.InnerPoints))
            {
                while (OuterPoints.Contains(c))
                    ((List<Point>) OuterPoints).Remove(c);
                while (InnerPoints.Contains(c))
                    ((List<Point>) InnerPoints).Remove(c);
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
            Region Region;
            int quarterTurns = 0;
            double radians = SadRogue.Primitives.MathHelpers.ToRadian(degrees);

            List<Point> corners = new List<Point>();
            if (origin == default(Point))
                origin = new Point((Left + Right) / 2, (Top + Bottom) / 2);

            while (degrees < 0)
                degrees += 360;

            while (degrees > 360)
                degrees -= 360;

            float d = degrees;
            while (d > 45)
            {
                d -= 90;
                quarterTurns++;
            }

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

        private Region QuarterRotation(bool doToSelf, Point origin)
        {
            //transpose
            int radius = Math.Abs(origin.X - Right);
            int nextDiff = Math.Abs(origin.X - Left);
            radius = radius < nextDiff ? nextDiff : radius;

            nextDiff = Math.Abs(origin.Y - Bottom);
            radius = radius < nextDiff ? nextDiff : radius;

            nextDiff = Math.Abs(origin.Y - Top);
            radius = radius < nextDiff ? nextDiff : radius;

            Point nw = SouthWestCorner - Center;
            nw = new Point(radius - nw.Y, nw.X) + Center;

            Point ne = NorthWestCorner;
            ne = new Point(radius - ne.Y, ne.X) + Center;

            Point se = NorthEastCorner;
            se = new Point(radius - se.Y, se.X) + Center;

            Point sw = SouthEastCorner;
            sw = new Point(radius - sw.Y, sw.X) + Center;

            if (doToSelf)
            {
                Generate(se, ne, nw, sw);
                return this;
            }
            else
                return new Region(Name, se, ne, nw, sw);

        }
        #endregion
    }
}
