using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.Pathing
{
    /// <summary>
    /// Encapsulates a path as returned by pathfinding algorithms like AStar.
    /// </summary>
    /// <remarks>
    /// Provides various functions to iterate through/access steps of the path, as well as
    /// constant-time reversing functionality.
    /// </remarks>
    [PublicAPI]
    public class Path : IReadOnlyList<Point> // TODO: Need efficient enumerables; also maybe pull start out of path?
    {
        private readonly IReadOnlyList<Point> _steps;
        private bool _inOriginalOrder;

        /// <summary>
        /// Creates a copy of the path, optionally reversing the path as it does so.
        /// </summary>
        /// <remarks>Reversing is an O(1) operation, since it does not modify the list.</remarks>
        /// <param name="pathToCopy">The path to copy.</param>
        /// <param name="reverse">Whether or not to reverse the path. Defaults to <see langword="false" />.</param>
        public Path(Path pathToCopy, bool reverse = false)
        {
            _steps = pathToCopy._steps;
            _inOriginalOrder = reverse ? !pathToCopy._inOriginalOrder : pathToCopy._inOriginalOrder;
        }

        // Create based on internal list
        internal Path(IReadOnlyList<Point> steps)
        {
            _steps = steps;
            _inOriginalOrder = true;
        }

        /// <summary>
        /// Gets the nth step along the path, where 0 is the step AFTER the starting point.
        /// </summary>
        /// <param name="index">The (array-like index) of the step to get.</param>
        /// <returns>The coordinate constituting the step specified.</returns>
        public Point this[int index] => _inOriginalOrder ? _steps[^(2 - index)] : _steps[index + 1];

        /// <summary>
        /// The length of the path, NOT including the starting point.
        /// </summary>
        public int Count => _steps.Count - 1;

        /// <summary>
        /// Ending point of the path.
        /// </summary>
        public Point End => _inOriginalOrder ? _steps[0] : _steps[^1];

        /// <summary>
        /// The length of the path, NOT including the starting point.
        /// </summary>
        [Obsolete("Use Count property instead.")]
        public int Length => _steps.Count - 1;

        /// <summary>
        /// The length of the path, INCLUDING the starting point.
        /// </summary>
        [Obsolete("Use CountWithStart property instead.")]
        public int LengthWithStart => _steps.Count;

        /// <summary>
        /// Starting point of the path.
        /// </summary>
        public Point Start => _inOriginalOrder ? _steps[^1] : _steps[0];


        /// <summary>
        /// The coordinates that constitute the path (in order), NOT including the starting point.
        /// These are the coordinates something might walk along to follow a path.
        /// </summary>
        [Obsolete("Path now implements IReadOnlyList; use its indexers/enumerable implementation directly instead.")]
        public IEnumerable<Point> Steps
        {
            get
            {
                if (_inOriginalOrder)
                    for (var i = _steps.Count - 2; i >= 0; i--)
                        yield return _steps[i];
                else
                    for (var i = 1; i < _steps.Count; i++)
                        yield return _steps[i];
            }
        }

        /// <summary>
        /// The coordinates that constitute the path (in order), INCLUDING the starting point.
        /// </summary>
        public IEnumerable<Point> StepsWithStart
        {
            get
            {
                if (_inOriginalOrder)
                    for (var i = _steps.Count - 1; i >= 0; i--)
                        yield return _steps[i];
                else
                    foreach (var step in _steps)
                        yield return step;
            }
        }

        /// <summary>
        /// Gets the nth step along the path, where 0 is the step AFTER the starting point.
        /// </summary>
        /// <param name="stepNum">The (array-like index) of the step to get.</param>
        /// <returns>The coordinate constituting the step specified.</returns>
        [Obsolete("Use the indexers provided by Path instead.")]
        public Point GetStep(int stepNum)
        {
            if (_inOriginalOrder)
                return _steps[_steps.Count - 2 - stepNum];

            return _steps[stepNum + 1];
        }

        /// <summary>
        /// Gets the nth step along the path, where 0 IS the starting point.
        /// </summary>
        /// <param name="stepNum">The (array-like index) of the step to get.</param>
        /// <returns>The coordinate constituting the step specified.</returns>
        public Point GetStepWithStart(int stepNum) =>
            // TODO: Revisit array-from-end syntax here
            _inOriginalOrder ? _steps[_steps.Count - 1 - stepNum] : _steps[stepNum];

        /// <summary>
        /// Reverses the path, in constant time.
        /// </summary>
        public void Reverse() => _inOriginalOrder = !_inOriginalOrder;

        /// <summary>
        /// Returns a string representation of all the steps in the path, including the start point,
        /// eg. [(1, 2), (3, 4), (5, 6)].
        /// </summary>
        /// <returns>A string representation of all steps in the path, including the start.</returns>
        public override string ToString() => StepsWithStart.ExtendToString();
    }
}
