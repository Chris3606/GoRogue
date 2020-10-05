using System;
using System.Collections.Generic;
using GoRogue.Random;
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
        protected ArrayMap<TileState> Map { get; }

        /// <summary>
        /// The map generator being used.
        /// </summary>
        protected Generator generator;
        private IEnumerator<object?>? _stageEnumerator;
        private bool _hasNext;
        private int _stepsCompleted;

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
            Map = new ArrayMap<TileState>(MapWidth, MapHeight);

            // Set up basic generator and state for tracking step progress
            generator = new Generator(MapWidth, MapHeight);
            _hasNext = true;
            _stepsCompleted = 0;
        }

        /// <inheritdoc />
        public void NextTimeUnit()
        {
            if (_stageEnumerator == null)
                throw new Exception("Map generation routine configured in invalid state.");

            if (!_hasNext)
                return;

            _hasNext = _stageEnumerator.MoveNext();
            _stepsCompleted++;
            UpdateMap();

        }

        /// <inheritdoc />
        public void LastTimeUnit()
        {
            if (_stepsCompleted <= 0)
                return;

            _stepsCompleted--;
            GlobalRandom.DefaultRNG.Reset();
            generator = new Generator(MapWidth, MapHeight);
            generator.AddSteps(GenerationSteps());
            _stageEnumerator = generator.GetStageEnumerator();

            for (int i = 0; i < _stepsCompleted; i++)
                _hasNext = _stageEnumerator.MoveNext();

            UpdateMap();
        }

        /// <inheritdoc />
        public void GenerateMap()
        {
            generator.AddSteps(GenerationSteps());
            _stageEnumerator = generator.GetStageEnumerator();
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
