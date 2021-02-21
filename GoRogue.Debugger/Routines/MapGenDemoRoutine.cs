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
        /// Map used for displaying.
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
            // Enumerator must be configured before next time unit is requested
            if (_stageEnumerator == null)
                throw new Exception("Map generation routine configured in invalid state.");


            // If there is a next diff to apply, simply apply it
            if (Map.CurrentDiffIndex < Map.Diffs.Count - 1)
            {
                Map.ApplyNextDiff();
                return;
            }

            // Otherwise, iterate until a change that displays something new to the user is made, or the algorithm
            // is done.
            int startingIndex = Map.CurrentDiffIndex;
            while (startingIndex == Map.CurrentDiffIndex && _hasNext)
            {

                // Advance the map generator, and apply the new state to the map
                _hasNext = _stageEnumerator.MoveNext();
                UpdateMap();

                // Finalize state we just created
                Map.FinalizeCurrentDiff();
            }
        }

        /// <inheritdoc />
        public void LastTimeUnit()
        {
            // No previous diffs to go to
            if (Map.CurrentDiffIndex < 0 || Map.Diffs.Count == 0)
                return;

            // Otherwise, just revert state
            Map.RevertToPreviousDiff();
        }

        /// <inheritdoc />
        public void GenerateMap()
        {
            // Set up map generation steps and get regeneration-safe stage enumerator
            _stageEnumerator = generator.ConfigAndGetStageEnumeratorSafe(gen =>
            {
                gen.AddSteps(GenerationSteps());
            });

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
        /// A simple function for creating a map view that displays tiles as wall/floor/door.
        /// </summary>
        /// <param name="pos">Position to return the character for.</param>
        /// <returns>The character for the position specified, depending on if it is a wall, floor, or door.</returns>
        protected char BasicDungeonView(Point pos)
            => Map[pos] switch
            {
                TileState.Wall => '#',
                TileState.Floor => '.',
                TileState.Door => '+',
                _ => throw new Exception("BasicDungeonView view encountered unsupported tile settings.")
            };
    }
}
