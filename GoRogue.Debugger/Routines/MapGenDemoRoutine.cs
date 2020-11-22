using System;
using System.Collections.Generic;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    public abstract class MapGenDemoRoutine : IRoutine
    {
        /// <inheritdoc />
        public string Name { get; }

        private const int MapWidth = 80;
        private const int MapHeight = 25;

        private readonly ArrayView<TileState> _underlyingMap;
        /// <summary>
        /// _grid used for displaying.
        /// </summary>
        protected DiffAwareGridView<TileState> Map { get; }

        /// <summary>
        /// The map generator being used.
        /// </summary>
        protected readonly Generator generator;
        private IEnumerator<object?>? _stageEnumerator;
        private bool _hasNext;

        /// <summary>
        /// Internal list of views.
        /// </summary>
        protected readonly List<(string name, IGridView<char> view)> views;

        /// <inheritdoc />
        public IReadOnlyList<(string name, IGridView<char> view)> Views => views;

        protected MapGenDemoRoutine(string name)
        {
            Name = name;
            views = new List<(string name, IGridView<char> view)>();

            // Set up map
            _underlyingMap = new ArrayView<TileState>(MapWidth, MapHeight);
            Map = new DiffAwareGridView<TileState>(_underlyingMap);

            // Set up basic generator and state for tracking step progress
            generator = new Generator(MapWidth, MapHeight);
            _hasNext = true;
        }

        /// <inheritdoc />
        public void NextTimeUnit()
        {
            if (_stageEnumerator == null)
                throw new Exception("_grid generation routine configured in invalid state.");

            if (!_hasNext)
                return;

            // If there was an existing diff to apply, we're done
            if (Map.ApplyNextDiffOrFinalize())
                return;

            // Otherwise, we'll advance the map generator and update the map state
            _hasNext = _stageEnumerator.MoveNext();
            UpdateMap();
        }

        /// <inheritdoc />
        public void LastTimeUnit()
        {
            if (Map.CurrentDiffIndex < 0 || Map.Diffs.Count == 0)
                return;

            Map.RevertToPreviousDiff();
        }

        /// <inheritdoc />
        public void GenerateMap()
        {
            // Set up map generation steps
            generator.AddSteps(GenerationSteps());
            _stageEnumerator = generator.GetStageEnumerator();

            // Set initial values
            SetInitialMapValues(_underlyingMap);
        }

        /// <inheritdoc />
        public void InterpretKeyPress(int key) { }

        /// <inheritdoc />
        public abstract void CreateViews();

        /// <summary>
        /// Returns the generation steps this routine uses and displays.
        /// </summary>
        /// <returns>The generation steps this routine uses and displays.</returns>
        protected abstract IEnumerable<GenerationStep> GenerationSteps();

        /// <summary>
        /// Sets the initial values for the map to the map view specified.  The underlying BaseGrid for the diff-aware
        /// map view used is passed to the function, and values should be set directly to that, to avoid creating diffs
        /// for the initial state.
        /// </summary>
        /// <param name="map">The map that the function should set values to.</param>
        protected abstract void SetInitialMapValues(ISettableGridView<TileState> map);

        /// <summary>
        /// Updates the map with new tiles based on current map generation context.
        /// </summary>
        protected abstract void UpdateMap();

        /// <summary>
        /// A simple function for creating a map view that displays tiles as wall/floor.
        /// </summary>
        /// <param name="pos">Position to return the character for.</param>
        /// <returns>The character for the position specified, depending on if it is a wall or a floor.</returns>
        protected char WallFloorView(Point pos)
            => Map[pos] switch
            {
                TileState.Wall => '#',
                TileState.Floor => '.',
                _ => throw new Exception("Wall-floor view encountered unsupported tile settings.")
            };
    }
}
