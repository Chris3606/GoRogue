using GoRogue.Random;
using System;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Implements a tunnel creation algorithm that creates a tunnel that performs all needed vertical movement before horizontal movement, or vice versa (depending
    /// on rng).
    /// </summary>
    public class HorizontalVerticalTunnelCreator : ITunnelCreator
    {
        private IRandom rng;

        /// <summary>
        /// Constructor.  Takes rng to use -- if null is specified, the default RNG is used.
        /// </summary>
        /// <param name="rng">Rng to use -- if null is specified, the default RNG is used.</param>
        public HorizontalVerticalTunnelCreator(IRandom rng = null)
        {
            if (rng == null)
                this.rng = SingletonRandom.DefaultRNG;
            else
                this.rng = rng;
        }

        /// <summary>
        /// Implmenets the algorithm, creating the tunnel as specified in the class description.
        /// </summary>
        /// <param name="map">The map to create the tunnel on.</param>
        /// <param name="start">Start coordinate of the tunnel.</param>
        /// <param name="end">End coordinate of the tunnel.</param>
        public void CreateTunnel(ISettableMapOf<bool> map, Coord start, Coord end)
        {
            if (rng == null) rng = SingletonRandom.DefaultRNG;

            if (rng.Next(2) == 0) // Favors vertical tunnels
            {
                createHTunnel(map, start.X, end.X, start.Y);
                createVTunnel(map, start.Y, end.Y, end.X);
            }
            else
            {
                createVTunnel(map, start.Y, end.Y, start.X);
                createHTunnel(map, start.X, end.X, end.Y);
            }
        }

        static private void createHTunnel(ISettableMapOf<bool> map, int xStart, int xEnd, int yPos)
        {
            for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); ++x)
                map[x, yPos] = true;
        }

        static private void createVTunnel(ISettableMapOf<bool> map, int yStart, int yEnd, int xPos)
        {
            for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); ++y)
                map[xPos, y] = true;
        }
    }
}