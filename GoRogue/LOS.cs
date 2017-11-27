using System;

namespace GoRogue
{
    /// <summary>
    /// Class responsible for caculating LOS/FOV.  Effectively a simplified, slightly faster interface compared to SenseMap, that supports only a single source and only
    /// shadowcasting.  This is more conducive to the typical use case for line of sight (LOS).  It can calculate the LOS with a finite or infinite max radius, and can use a
    /// variety of radius types, as specified in Radius class (all the same ones that SenseMap supports).  It also supports both 360 degree LOS and a "field of view" (cone)
    /// LOS.  One may access this class like a 2D array of doubles (LOS values), wherein the values will range from 0.0 to 1.0, where 1.0 means the corresponding map
    /// grid coordinate is at maximum visibility, and 0.0 means the cooresponding coordinate is outside of LOS entirely (not visible).
    /// </summary>
    public class LOS : IMapOf<double>
    {
        private IMapOf<double> resMap;
        private double[,] light; // Last light cached

        /// <summary>
        /// Width of LOS map.
        /// </summary>
        public int Width { get => resMap.Width; }
        /// <summary>
        /// Height of LOS map.
        /// </summary>
        public int Height { get => resMap.Height; }

        /// <summary>
        /// Array-style indexer that takes a Coord as the index, and retrieves the LOS value at the given location.
        /// </summary>
        /// <param name="position">The position to retrieve the LOS value for.</param>
        /// <returns>The LOS value at the given location.</returns>
        public double this[Coord position]
        {
            get { return light[position.X, position.Y]; }
        }

        /// <summary>
        /// Array-style indexer that takes an x and y value as the index, and retrieves the LOS value at the given location.
        /// </summary>
        /// <param name="x">The x-coordinate of the position to retrieve the LOS value for.</param>
        /// <param name="y">The y-coordinate of the position to retrieve the LOS value for.</param>
        /// <returns>The LOS value at (x, y).</returns>
        public double this[int x, int y]
        {
            get { return light[x, y]; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resMap">The resistance map to use to calculate LOS.  Values of 1.0 are considered blocking to LOS,
        /// while other (lower) values are considered to be not blocking.</param>
        public LOS(IMapOf<double> resMap)
        {
            this.resMap = resMap;
            light = null;
        }

        // Since the values aren't compile-time constants, we have to do it this way (with overloads, vs. default values).
        /// <summary>
        /// Calculates LOS, given an origin point of (startX, startY), with a given radius.  If no radius is specified, simply calculates with a radius of
        /// maximum integer value, which is effectively infinite.  Radius is computed as a circle around the source (type Radius.CIRCLE).
        /// </summary>
        /// <param name="startX">Coordinate x-value of the origin.</param>
        /// <param name="startY">Coordinate y-value of the origin.</param>
        /// <param name="radius">The maximum radius -- basically the maximum distance of LOS if completely unobstructed.  If no radius is specified, it is
        /// effectively infinite.</param>
        public void Calculate(int startX, int startY, int radius = int.MaxValue)
        {
            Calculate(startX, startY, radius, Radius.CIRCLE);
        }

        /// <summary>
        /// Calculates LOS, given an origin point of (startX, startY), with the given radius and radius calculation strategy.
        /// </summary>
        /// <param name="startX">Coordinate x-value of the origin.</param>
        /// <param name="startY">Coordinate y-value of the origin.</param>
        /// <param name="radius">The maximum radius -- basically the maximum distance of LOS if completely unobstructed.</param>
        /// <param name="radiusTechnique">The type of the radius (square, circle, diamond, etc.)</param>
        public void Calculate(int startX, int startY, int radius, Radius radiusTechnique)
        {
            var distanceTechnique = (Distance)radiusTechnique;
            double rad = Math.Max(1, radius);
            double decay = 1.0 / (rad + 1);

            initializeLightMap();
            light[startX, startY] = 1; // Full power to starting space

            foreach (Direction d in Direction.DiagonalsTopBottom())
            {
                shadowCast(1, 1.0, 0.0, 0, d.DeltaX, d.DeltaY, 0, (int)rad, startX, startY, decay, light, resMap, distanceTechnique);
                shadowCast(1, 1.0, 0.0, d.DeltaX, 0, 0, d.DeltaY, (int)rad, startX, startY, decay, light, resMap, distanceTechnique);
            }
        }

        /// <summary>
        /// Calculates LOS, given an origin point of (startX, startY), with the given radius and radius calculation strategy, and assuming LOS is restricted to the area
        /// specified by the given angle and span, in degrees. Provided that span is greater than 0, a conical section of the regular LOS radius will be actually in LOS.
        /// </summary>
        /// <param name="startX">Coordinate x-value of the origin.</param>
        /// <param name="startY">Coordinate y-value of the origin.</param>
        /// <param name="radius">The maximum radius -- basically the maximum distance of LOS if completely unobstructed.</param>
        /// <param name="radiusTechnique">The type of the radius (square, circle, diamond, etc.)</param>
        /// <param name="angle">The angle in degrees that specifies the outermost center point of the LOS cone.  0 degrees points right.</param>
        /// <param name="span">The angle, in degrees, that specifies the full arc contained in the LOS cone -- angle/2 degrees are included on either side of the span line.</param>
        public void Calculate(int startX, int startY, int radius, Radius radiusTechnique, double angle, double span)
        {
            var distanceTechnique = (Distance)radiusTechnique;
            double rad = Math.Max(1, radius);
            double decay = 1.0 / (rad + 1);

            double angle2 = MathHelpers.ToRadian((angle > 360.0 || angle < 0.0) ? Math.IEEERemainder(angle + 720.0, 360.0) : angle);
            double span2 = MathHelpers.ToRadian(span);

            initializeLightMap();
            light[startX, startY] = 1; // Starting space full light

            // TODO: There may be a glitch here with too large radius, may have to set to longest possible straight-line Manhattan dist for map intsead.  No falloff issue -- shadow is on/off.
            int ctr = 0;
            bool started = false;
            foreach (Direction d in Direction.DiagonalsCounterClockwise(Direction.UP_RIGHT))
            {
                ctr %= 4;
                ctr++;
                if (angle <= Math.PI / 2.0 * ctr + span / 2.0)
                    started = true;

                if (started)
                {
                    if (ctr < 4 && angle < Math.PI / 2.0 * (ctr - 1) - span / 2.0)
                        break;

                    light = shadowCastLimited(1, 1.0, 0.0, 0, d.DeltaX, d.DeltaY, 0, (int)rad, startX, startY, decay, light, resMap, distanceTechnique, angle2, span2);
                    light = shadowCastLimited(1, 1.0, 0.0, d.DeltaX, 0, 0, d.DeltaY, (int)rad, startX, startY, decay, light, resMap, distanceTechnique, angle2, span2);
                }
            }
        }

        private void initializeLightMap()
        {
            if (light == null || light.GetLength(0) != resMap.Width || light.GetLength(1) != resMap.Height)
                light = new double[resMap.Width, resMap.Height];
            else
                Array.Clear(light, 0, light.Length);
        }

        // Returns value because its recursive
        private static double[,] shadowCast(int row, double start, double end, int xx, int xy, int yx, int yy,
                                     int radius, int startX, int startY, double decay, double[,] lightMap,
                                     IMapOf<double> map, Distance distanceStrategy)
        {
            double newStart = 0;
            if (start < end)
                return lightMap;

            bool blocked = false;
            for (int distance = row; distance <= radius && distance < map.Width + map.Height && !blocked; distance++)
            {
                int deltaY = -distance;
                for (int deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    int currentX = startX + deltaX * xx + deltaY * xy;
                    int currentY = startY + deltaX * yx + deltaY * yy;
                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!(currentX >= 0 && currentY >= 0 && currentX < map.Width && currentY < map.Height) || start < rightSlope)
                        continue;

                    if (end > leftSlope)
                        break;

                    double deltaRadius = distanceStrategy.DistanceOf(deltaX, deltaY);
                    // If within lightable area, light if needed
                    if (deltaRadius <= radius)
                    {
                        double bright = 1 - decay * deltaRadius;
                        lightMap[currentX, currentY] = bright;
                    }

                    if (blocked) // Previous cell was blocked
                    {
                        if (map[currentX, currentY] >= 1) // Hit a wall...
                            newStart = rightSlope;
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else
                    {
                        if (map[currentX, currentY] >= 1 && distance < radius) // Wall within sight line
                        {
                            blocked = true;
                            lightMap = shadowCast(distance + 1, start, leftSlope, xx, xy, yx, yy, radius, startX, startY, decay, lightMap, map, distanceStrategy);
                            newStart = rightSlope;
                        }
                    }
                }
            }
            return lightMap;
        }

        private static double[,] shadowCastLimited(int row, double start, double end, int xx, int xy, int yx, int yy, int radius, int startX, int startY, double decay,
                                                   double[,] lightMap, IMapOf<double> map, Distance distanceStrategy, double angle, double span)
        {
            double newStart = 0;
            if (start < end)
                return lightMap;

            bool blocked = false;
            for (int distance = row; distance <= radius && distance < map.Width + map.Height && !blocked; distance++)
            {
                int deltaY = -distance;
                for (int deltaX = -distance; deltaX <= 0; deltaX++)
                {
                    int currentX = startX + deltaX * xx + deltaY * xy;
                    int currentY = startY + deltaX * yx + deltaY * yy;
                    double leftSlope = (deltaX - 0.5f) / (deltaY + 0.5f);
                    double rightSlope = (deltaX + 0.5f) / (deltaY - 0.5f);

                    if (!(currentX >= 0 && currentY >= 0 && currentX < map.Width && currentY < map.Height) || start < rightSlope)
                        continue;

                    if (end > leftSlope)
                        break;

                    double newAngle = Math.Atan2(currentY - startY, currentX - startX) + Math.PI * 2;
                    if (Math.Abs(Math.IEEERemainder(angle - newAngle + Math.PI * 8, Math.PI * 2)) > span / 2.0)
                        continue;

                    double deltaRadius = distanceStrategy.DistanceOf(deltaX, deltaY);
                    if (deltaRadius <= radius) // Check if within lightable area, light if needed
                    {
                        double bright = 1 - decay * deltaRadius;
                        lightMap[currentX, currentY] = bright;
                    }

                    if (blocked) // Previous cell was blocking
                    {
                        if (map[currentX, currentY] >= 1) // We hit a wall
                            newStart = rightSlope;
                        else
                        {
                            blocked = false;
                            start = newStart;
                        }
                    }
                    else
                    {
                        if (map[currentX, currentY] >= 1 && distance < radius) // Wall within line of sight
                        {
                            blocked = true;
                            lightMap = shadowCastLimited(distance + 1, start, leftSlope, xx, xy, yx, yy, radius, startX, startY, decay, lightMap, map, distanceStrategy, angle, span);
                            newStart = rightSlope;
                        }
                    }
                }
            }
            return lightMap;
        }
    }
}