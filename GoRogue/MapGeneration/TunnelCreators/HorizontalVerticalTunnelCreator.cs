using System;
using System.Collections.Generic;
using GoRogue.Random;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Troschuetz.Random;

namespace GoRogue.MapGeneration.TunnelCreators
{
    /// <summary>
    /// Implements a tunnel creation algorithm that creates a tunnel that performs all needed
    /// vertical movement before horizontal movement, or vice versa (depending on rng).
    /// </summary>
    [PublicAPI]
    public class HorizontalVerticalTunnelCreator : ITunnelCreator
    {
        private readonly IGenerator _rng;

        /// <summary>
        /// Creates a new tunnel creator.
        /// </summary>
        /// <param name="rng">RNG to use for movement selection.</param>
        public HorizontalVerticalTunnelCreator(IGenerator? rng = null) => _rng = rng ?? GlobalRandom.DefaultRNG;

        /// <inheritdoc />
        public Area CreateTunnel(ISettableGridView<bool> map, Point tunnelStart, Point tunnelEnd)
        {
            var tunnel = new Area();

            if (_rng.NextBoolean())
            {
                tunnel.Add(CreateHTunnel(map, tunnelStart.X, tunnelEnd.X, tunnelStart.Y));
                tunnel.Add(CreateVTunnel(map, tunnelStart.Y, tunnelEnd.Y, tunnelEnd.X));
            }
            else
            {
                tunnel.Add(CreateVTunnel(map, tunnelStart.Y, tunnelEnd.Y, tunnelStart.X));
                tunnel.Add(CreateHTunnel(map, tunnelStart.X, tunnelEnd.X, tunnelEnd.Y));
            }

            return tunnel;
        }

        /// <inheritdoc />
        public Area CreateTunnel(ISettableGridView<bool> map, int startX, int startY, int endX, int endY)
            => CreateTunnel(map, new Point(startX, startY), new Point(endX, endY));

        private static IEnumerable<Point> CreateHTunnel(ISettableGridView<bool> map, int xStart, int xEnd, int yPos)
        {
            for (var x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); ++x)
            {
                map[x, yPos] = true;
                yield return new Point(x, yPos);
            }
        }

        private static IEnumerable<Point> CreateVTunnel(ISettableGridView<bool> map, int yStart, int yEnd, int xPos)
        {
            for (var y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); ++y)
            {
                map[xPos, y] = true;
                yield return new Point(xPos, y);
            }
        }
    }
}
