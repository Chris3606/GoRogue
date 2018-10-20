using System;

namespace GoRogue.MapGeneration.Connectors
{
	/// <summary>
	/// Selects the center points of the bounding boxes of each map area. On concave map areas,
	/// because the center point of the bounding box is not actually guaranteed to be among the
	/// MapArea's walkable tiles, connecting these two points is not guaranteed to actually connect
	/// the entirety of the two areas.
	/// </summary>
	public class CenterBoundsConnectionPointSelector : IAreaConnectionPointSelector
	{
		/// <summary>
		/// Selects and returns a the center point of the bounding rectangle for each map area's
		/// positions list.
		/// </summary>
		/// <param name="area1">First map area to connect.</param>
		/// <param name="area2">First map area to connect.</param>
		/// <returns></returns>
		public Tuple<Coord, Coord> SelectConnectionPoints(IReadOnlyMapArea area1, IReadOnlyMapArea area2) => new Tuple<Coord, Coord>(area1.Bounds.Center, area2.Bounds.Center);
	}
}