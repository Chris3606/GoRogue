using System;

namespace GoRogue.GameFramework
{
	public class GameObject<BaseSubclass> : IHasID, IHasLayer where BaseSubclass : GameObject<BaseSubclass>
	{
		private Coord _position;
		public virtual Coord Position
		{
			get => _position;
			set
			{
				if (_position == value || IsStatic)
					return;

				if (CurrentMap == null || IsWalkable || CurrentMap.WalkabilityView[value])
				{
					var oldPos = _position;
					_position = value;
					Moved?.Invoke(this, new ItemMovedEventArgs<BaseSubclass>((BaseSubclass)this, oldPos, _position));
				}
			}
		}

		public event EventHandler<ItemMovedEventArgs<BaseSubclass>> Moved;

		public virtual bool IsWalkable { get; set; }
		public virtual bool IsTransparent { get; set; }

		public bool IsStatic { get; }

		public uint ID { get; }
		public int Layer { get; }

		public Map<BaseSubclass> CurrentMap { get; internal set; }

		private static IDGenerator _idGenerator = new IDGenerator();

		public GameObject(Coord position, int layer, bool isStatic = false, bool isWalkable = true, bool isTransparent = true)
		{
			_position = position;
			Layer = layer;
			IsWalkable = isWalkable;
			IsTransparent = isTransparent;
			IsStatic = isStatic;

			CurrentMap = null;

			ID = _idGenerator.UseID();
		}

		public bool MoveIn(Direction direction)
		{
			Coord oldPos = _position;
			Position += direction;

			return _position != oldPos;
		}

		public static void SetStartingID(uint id) => _idGenerator = new IDGenerator(id);
	}
}
