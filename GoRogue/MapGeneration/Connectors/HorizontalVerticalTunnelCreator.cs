using GoRogue.Random;
using System;

namespace GoRogue.MapGeneration.Connectors
{
    /// <summary>
    /// Implements a tunnel creation algorithm that performs all vertical movement before horizontal movement, or vice versa.
    /// </summary>
    public class HorizontalVerticalTunnelCreator : ITunnelCreator
    {
        private IRandom rng;

        public HorizontalVerticalTunnelCreator(IRandom rng = null)
        {
            if (rng == null)
                this.rng = SingletonRandom.DefaultRNG;
            else
                this.rng = rng;
        }

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
