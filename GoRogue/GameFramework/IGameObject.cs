using System;

namespace GoRogue.GameFramework
{
	public interface IGameObject : IHasID, IHasLayer
	{
		Map CurrentMap { get; }
		bool IsStatic { get; }
		bool IsTransparent { get; set; }
		bool IsWalkable { get; set; }
		Coord Position { get; set; }

		event EventHandler<ItemMovedEventArgs<IGameObject>> Moved;

		bool MoveIn(Direction direction);

		/// <summary>
		/// Internal use only, do not call manually!  Must, at minimum, update the CurrentMap field of the
		/// IGameObject to reflect the change.
		/// </summary>
		/// <param name="newMap">New map to which the IGameObject has been added.</param>
		void OnMapChanged(Map newMap);
	}
}