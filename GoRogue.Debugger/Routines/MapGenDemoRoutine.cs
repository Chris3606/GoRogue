using System;
using System.Collections.Generic;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.Debugger.Routines
{
    public abstract class MapGenDemoRoutine : IRoutine
    {
        /// <inheritdoc />
        public string Name { get; }

        private const int MapWidth = 80;
        private const int MapHeight = 25;

        /// <summary>
        /// Map used for displaying.
        /// </summary>
        protected TimeStepAwareMapView<TileState> Map { get; }

        /// <summary>
        /// The map generator being used.
        /// </summary>
        protected readonly Generator generator;
        private IEnumerator<object?>? _stageEnumerator;
        private bool _hasNext;

        /// <summary>
        /// Internal list of views.
        /// </summary>
        protected readonly List<(string name, IMapView<char> view)> views;

        /// <inheritdoc />
        public IReadOnlyList<(string name, IMapView<char> view)> Views => views;

        protected MapGenDemoRoutine(string name)
        {
            Name = name;
            views = new List<(string name, IMapView<char> view)>();

            // Set up map
            Map = new TimeStepAwareMapView<TileState>(MapWidth, MapHeight);

            // Set up basic generator and state for tracking step progress
            generator = new Generator(MapWidth, MapHeight);
            _hasNext = true;
        }

        /// <inheritdoc />
        public void NextTimeUnit()
        {
            if (_stageEnumerator == null)
                throw new Exception("Map generation routine configured in invalid state.");

            if (!_hasNext)
                return;

            if (Map.NextTimeStep())
            {
                _hasNext = _stageEnumerator.MoveNext();
                UpdateMap();
            }
        }

        /// <inheritdoc />
        public void LastTimeUnit()
        {
            if (Map.CurrentStep < 0)
                return;

            Map.LastTimeStep();
        }

        /// <inheritdoc />
        public void GenerateMap()
        {
            generator.AddSteps(GenerationSteps());
            _stageEnumerator = generator.GetStageEnumerator();

            SetInitialMapValues();
        }

        /// <inheritdoc />
        public void InterpretKeyPress(ConsoleKey key) { }

        /// <inheritdoc />
        public abstract void CreateViews();

        /// <summary>
        /// Returns the generation steps this routine uses and displays.
        /// </summary>
        /// <returns>The generation steps this routine uses and displays.</returns>
        protected abstract IEnumerable<GenerationStep> GenerationSteps();

        /// <summary>
        /// Sets the initial values for the map.
        /// </summary>
        protected abstract void SetInitialMapValues();

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
