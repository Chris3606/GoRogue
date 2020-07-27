using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// An arbitrary list of any number of positions on the perimeter of a rectangle.  Commonly used to represent a list of
    /// doors or edges of rooms by
    /// some map generation steps.
    /// </summary>
    [PublicAPI]
    public class RectangleEdgePositionsList : IEnumerable<Point>
    {
        private readonly List<Point> _bottomPositions;

        private readonly List<Point> _leftPositions;

        private readonly HashSet<Point> _positions;

        private readonly List<Point> _rightPositions;
        private readonly List<Point> _topPositions;

        /// <summary>
        /// The rectangle whose edge positions are being stored.
        /// </summary>
        public readonly Rectangle Rectangle;


        /// <summary>
        /// Creates a empty list of perimeter for a given rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle the structure stores perimeter positions for.</param>
        public RectangleEdgePositionsList(Rectangle rectangle)
        {
            Rectangle = rectangle;
            _topPositions = new List<Point>();
            _rightPositions = new List<Point>();
            _bottomPositions = new List<Point>();
            _leftPositions = new List<Point>();
            _positions = new HashSet<Point>();
        }

        /// <summary>
        /// Positions on the top edge of the rectangle.
        /// </summary>
        public IReadOnlyList<Point> TopPositions => _topPositions.AsReadOnly();

        /// <summary>
        /// Positions of doors on the right wall of the rectangle.
        /// </summary>
        public IReadOnlyList<Point> RightPositions => _rightPositions.AsReadOnly();

        /// <summary>
        /// Positions of doors on the bottom wall of the rectangle.
        /// </summary>
        public IReadOnlyList<Point> BottomPositions => _bottomPositions.AsReadOnly();

        /// <summary>
        /// Positions of doors on the bottom wall of the rectangle.
        /// </summary>
        public IReadOnlyList<Point> LeftPositions => _leftPositions.AsReadOnly();

        /// <summary>
        /// Positions being stored, with no duplicate locations.
        /// </summary>
        public IEnumerable<Point> Positions => _positions;

        /// <summary>
        /// Retrieves a read-only list of stored positions on the given side.  Direction specified must be a cardinal.
        /// </summary>
        /// <param name="side">Side to get stored positions for.</param>
        /// <returns>A read-only list of stored positions on the given side.</returns>
        public IReadOnlyList<Point> this[Direction side]
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            => side.Type switch
            {
                Direction.Types.Up => TopPositions,
                Direction.Types.Right => RightPositions,
                Direction.Types.Down => BottomPositions,
                Direction.Types.Left => LeftPositions,
                _ => throw new ArgumentException("Side of a room must be a cardinal direction.", nameof(side))
            };

        /// <summary>
        /// Adds the given position to the appropriate lists of positions.
        /// </summary>
        /// <param name="perimeterPosition">Position to add.</param>
        public void Add(Point perimeterPosition) => AddRange(perimeterPosition);

        /// <summary>
        /// Adds the given positions to the appropriate lists of positions.
        /// </summary>
        /// <param name="perimeterPositions">Positions to add.</param>
        public void AddRange(params Point[] perimeterPositions)
            => AddRange((IEnumerable<Point>)perimeterPositions);

        /// <summary>
        /// Adds the given positions to the appropriate lists of positions.
        /// </summary>
        /// <param name="perimeterPositions">Positions to add.</param>
        public void AddRange(IEnumerable<Point> perimeterPositions)
        {
            foreach (var pos in perimeterPositions)
            {
                bool top = Rectangle.IsOnSide(pos, Direction.Up);
                bool right = Rectangle.IsOnSide(pos, Direction.Right);
                bool down = Rectangle.IsOnSide(pos, Direction.Down);
                bool left = Rectangle.IsOnSide(pos, Direction.Left);

                // Not directly on perimeter of rectangle
                if (!(top || right || down || left))
                    throw new ArgumentException(
                        $"Positions added to a {nameof(RectangleEdgePositionsList)} must be on one of the edges of the rectangle.",
                        nameof(perimeterPositions));

                // Allowed but it won't record it multiple times
                if (_positions.Contains(pos))
                    continue;

                // Add to collection of positions and appropriate sub-lists
                _positions.Add(pos);

                if (top)
                    _topPositions.Add(pos);

                if (right)
                    _rightPositions.Add(pos);

                if (down)
                    _bottomPositions.Add(pos);

                if (left)
                    _leftPositions.Add(pos);
            }
        }

        /// <summary>
        /// Removes the given position from the data structure.
        /// </summary>
        /// <param name="perimeterPosition">Position to remove.</param>
        public void Remove(Point perimeterPosition) => RemoveRange(perimeterPosition);

        /// <summary>
        /// Removes the given positions from the data structure.
        /// </summary>
        /// <param name="perimeterPositions">Positions to remove.</param>
        public void RemoveRange(params Point[] perimeterPositions)
            => RemoveRange((IEnumerable<Point>)perimeterPositions);

        /// <summary>
        /// Removes the given positions from the data structure.
        /// </summary>
        /// <param name="perimeterPositions">Positions to remove.</param>
        public void RemoveRange(IEnumerable<Point> perimeterPositions)
        {
            foreach (var pos in perimeterPositions)
            {
                if (!_positions.Contains(pos))
                    throw new ArgumentException(
                        $"Tried to remove a position from a ${nameof(RectangleEdgePositionsList)} that was not present.");

                // Remove from collection of positions and appropriate sub-lists
                _positions.Remove(pos);

                if (Rectangle.IsOnSide(pos, Direction.Up))
                    _topPositions.Remove(pos);

                if (Rectangle.IsOnSide(pos, Direction.Right))
                    _rightPositions.Remove(pos);

                if (Rectangle.IsOnSide(pos, Direction.Down))
                    _bottomPositions.Remove(pos);

                if (Rectangle.IsOnSide(pos, Direction.Left))
                    _leftPositions.Remove(pos);
            }
        }

        /// <summary>
        /// Returns whether or not the structure contains the given position.
        /// </summary>
        /// <param name="position" />
        /// <returns>Whether or not the structure contains the position specified.</returns>
        public bool Contains(Point position) => _positions.Contains(position);

        /// <summary>
        /// Gets an enumerator of all positions in the data structure.
        /// </summary>
        /// <returns/>
        public IEnumerator<Point> GetEnumerator() => _positions.GetEnumerator();

        /// <summary>
        /// Gets an enumerator of all positions in the data structure.
        /// </summary>
        /// <returns/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
