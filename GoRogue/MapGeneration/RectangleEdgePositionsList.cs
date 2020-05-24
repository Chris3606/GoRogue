using System;
using System.Collections.Generic;
using SadRogue.Primitives;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// An arbitrary list of any number of positions on the perimeter of a rectangle.  Commonly used to represent a list of doors or edges of rooms by
    /// some map generation steps.
    /// </summary>
    public class RectangleEdgePositionsList
    {
        private readonly List<Point> _topPositions;

        /// <summary>
        /// Positions on the top edge of the rectangle.
        /// </summary>
        public IReadOnlyList<Point> TopPositions => _topPositions.AsReadOnly();

        private readonly List<Point> _rightPositions;

        /// <summary>
        /// Positions of doors on the right wall of the rectangle.
        /// </summary>
        public IReadOnlyList<Point> RightPositions => _rightPositions.AsReadOnly();

        private readonly List<Point> _bottomPositions;

        /// <summary>
        /// Positions of doors on the bottom wall of the rectangle.
        /// </summary>
        public IReadOnlyList<Point> BottomPositions => _bottomPositions.AsReadOnly();

        private readonly List<Point> _leftPositions;

        /// <summary>
        /// Positions of doors on the bottom wall of the rectangle.
        /// </summary>
        public IReadOnlyList<Point> LeftPositions => _leftPositions.AsReadOnly();

        /// <summary>
        /// The rectangle whose edge positions are being stored.
        /// </summary>
        public readonly Rectangle Rectangle;

        private readonly HashSet<Point> _positions;
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
            => side.Type switch
            {
                Direction.Types.Up => TopPositions,
                Direction.Types.Right => RightPositions,
                Direction.Types.Down => BottomPositions,
                Direction.Types.Left => LeftPositions,
                _ => throw new ArgumentException("Side of a room must be a cardinal direction.", nameof(side))
            };


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
        /// Adds the given position to the appropriate lists of positions.
        /// </summary>
        /// <param name="perimeterPosition">Position to add.</param>
        public void AddPosition(Point perimeterPosition) => AddPositions(perimeterPosition);

        /// <summary>
        /// Adds the given positions to the appropriate lists of positions.
        /// </summary>
        /// <param name="perimeterPositions">Positions to add.</param>
        public void AddPositions(params Point[] perimeterPositions) => AddPositions((IEnumerable<Point>)perimeterPositions);

        /// <summary>
        /// Adds the given positions to the appropriate lists of positions.
        /// </summary>
        /// <param name="perimeterPositions">Positions to add.</param>
        public void AddPositions(IEnumerable<Point> perimeterPositions)
        {
            foreach (var pos in perimeterPositions)
            {
                // Not directly on perimeter of rectangle
                if (!Rectangle.IsOnTopEdge(pos) && !Rectangle.IsOnRightEdge(pos) && !Rectangle.IsOnBottomEdge(pos) && !Rectangle.IsOnLeftEdge(pos))
                    throw new ArgumentException($"Positions added to a {nameof(RectangleEdgePositionsList)} must be on one of the edges of the rectangle.", nameof(perimeterPositions));

                // Allowed but it won't record it multiple times
                if (_positions.Contains(pos))
                    continue;

                // Add to collection of positions and appropriate sublists
                _positions.Add(pos);

                if (Rectangle.IsOnTopEdge(pos))
                    _topPositions.Add(pos);

                if (Rectangle.IsOnRightEdge(pos))
                    _rightPositions.Add(pos);

                if (Rectangle.IsOnBottomEdge(pos))
                    _bottomPositions.Add(pos);

                if (Rectangle.IsOnLeftEdge(pos))
                    _leftPositions.Add(pos);
            }
        }

        /// <summary>
        /// Removes the given position from the data structure.
        /// </summary>
        /// <param name="perimeterPosition">Position to remove.</param>
        public void RemovePosition(Point perimeterPosition) => RemovePositions(perimeterPosition);

        /// <summary>
        /// Removes the given positions from the data structure.
        /// </summary>
        /// <param name="perimeterPositions">Positions to remove.</param>
        public void RemovePositions(params Point[] perimeterPositions) => RemovePositions((IEnumerable<Point>)perimeterPositions);

        /// <summary>
        /// Removes the given positions from the data structure.
        /// </summary>
        /// <param name="perimeterPositions">Positions to remove.</param>
        public void RemovePositions(IEnumerable<Point> perimeterPositions)
        {
            foreach (var pos in perimeterPositions)
            {
                if (!_positions.Contains(pos))
                    throw new ArgumentException($"Tried to remove a position from a ${nameof(RectangleEdgePositionsList)} that was not present.");

                // Remove from collection of positions and appropriate sublists
                _positions.Remove(pos);

                if (Rectangle.IsOnTopEdge(pos))
                    _topPositions.Remove(pos);

                if (Rectangle.IsOnRightEdge(pos))
                    _rightPositions.Remove(pos);

                if (Rectangle.IsOnBottomEdge(pos))
                    _bottomPositions.Remove(pos);

                if (Rectangle.IsOnLeftEdge(pos))
                    _leftPositions.Remove(pos);
            }
        }

        /// <summary>
        /// Returns whether or not the structure contains the given position.
        /// </summary>
        /// <param name="position"/>
        /// <returns>Whether or not the structure contains the position specified.</returns>
        public bool Contains(Point position) => _positions.Contains(position);
    }
}
