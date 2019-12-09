using GoRogue.MapViews;
using GoRogue.Random;
using System;
using Troschuetz.Random;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration.Connectors
{
	/// <summary>
	/// Implements a tunnel creation algorithm that creates a tunnel that performs all needed
	/// vertical movement before horizontal movement, or vice versa (depending on rng).
	/// </summary>
	public class HorizontalVerticalTunnelCreator : ITunnelCreator
	{
		private IGenerator rng;

		/// <summary>
		/// Constructor. Takes rng to use -- if null is specified, the <see cref="SingletonRandom.DefaultRNG"/> is used.
		/// </summary>
		/// <param name="rng">Rng to use -- if null is specified, <see cref="SingletonRandom.DefaultRNG"/> is used.</param>
		public HorizontalVerticalTunnelCreator(IGenerator rng = null)
		{
			if (rng == null)
				this.rng = SingletonRandom.DefaultRNG;
			else
				this.rng = rng;
		}

		/// <summary>
		/// Implemnets the algorithm, creating the tunnel as specified in the class description.
		/// </summary>
		/// <param name="map">The map to create the tunnel on.</param>
		/// <param name="start">Start Pointinate of the tunnel.</param>
		/// <param name="end">End Pointinate of the tunnel.</param>
		public void CreateTunnel(ISettableMapView<bool> map, Point start, Point end)
		{
			if (rng == null) rng = SingletonRandom.DefaultRNG;

			if (rng.NextBoolean())
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

		/// <summary>
		/// Implements the algorithm, creating the tunnel as specified in the class description.
		/// </summary>
		/// <param name="map">The map to create the tunnel on.</param>
		/// <param name="startX">X-value of the start position of the tunnel.</param>
		/// <param name="startY">Y-value of the start position of the tunnel.</param>
		/// <param name="endX">X-value of the end position of the tunnel.</param>
		/// <param name="endY">Y-value of the end position of the tunnel.</param>
		public void CreateTunnel(ISettableMapView<bool> map, int startX, int startY, int endX, int endY) => CreateTunnel(map, new Point(startX, startY), new Point(endX, endY));

		static private void createHTunnel(ISettableMapView<bool> map, int xStart, int xEnd, int yPos)
		{
			for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); ++x)
				map[x, yPos] = true;
		}

		static private void createVTunnel(ISettableMapView<bool> map, int yStart, int yEnd, int xPos)
		{
			for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); ++y)
				map[xPos, y] = true;
		}
	}
}
