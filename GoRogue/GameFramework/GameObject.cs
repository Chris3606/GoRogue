using System;
using System.Collections.Generic;

namespace GoRogue.GameFramework
{
	public class GameObject : IHasID, IHasLayer
	{
		private List<Component> _components;

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
					Moved?.Invoke(this, new ItemMovedEventArgs<GameObject>(this, oldPos, _position));
				}
			}
		}

		public event EventHandler<ItemMovedEventArgs<GameObject>> Moved;

		public virtual bool IsWalkable { get; set; }
		public virtual bool IsTransparent { get; set; }

		public bool IsStatic { get; }

		public uint ID { get; }
		public int Layer { get; }

		public Map CurrentMap { get; internal set; }

		private static IDGenerator _idGenerator = new IDGenerator();

		public GameObject(Coord position, int layer, bool isStatic = false, bool isWalkable = true, bool isTransparent = true)
		{
			Position = position;
			IsWalkable = isWalkable;
			IsTransparent = isTransparent;
			IsStatic = isStatic;

			CurrentMap = null;

			ID = _idGenerator.UseID();

			_components = new List<Component>();
		}

		public static void SetStartingID(uint id) => _idGenerator = new IDGenerator(id);

		#region Components
		public void AddComponent(Component component)
		{
			component.GameObject = this;
			_components.Add(component);
		}

		public ComponentType GetComponent<ComponentType>() where ComponentType : Component
		{
			foreach (var component in _components)
				if (component is ComponentType returnableComponent)
					return returnableComponent;

			return null;
		}

		public IEnumerable<Component> GetComponents<ComponentType>() where ComponentType : Component
		{
			foreach (var component in _components)
				if (component is ComponentType returnableComponent)
					yield return returnableComponent;
		}
		#endregion
	}
}
