using System.Collections.Generic;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using SadRogue.Primitives;

namespace GoRogue.Debugger.Routines
{
    public class DungeonMazeMap : IRoutine
    {
        public IEnumerable<Region> Regions => new List<Region>();
        public string Name { get; }
        public Map? BaseMap { get; private set; }
        public Map? TransformedMap { get; private set; }
        private readonly Generator _generator;
        private readonly IEnumerator<object?> _stageEnumerator;
        private bool _hasNext = true;

        public DungeonMazeMap()
        {
            Name = "Dungeon Maze Map Generation";
            _generator = new Generator(80, 25);
            _generator.AddSteps(DefaultAlgorithms.DungeonMazeMapSteps());
            _stageEnumerator = _generator.GetStageEnumerator();
        }

        public Map ElapseTimeUnit()
        {
            if (_hasNext)
                _hasNext = _stageEnumerator.MoveNext();

            var mapView = _generator.Context.GetFirst<IMapView<bool>>();
            TransformedMap?.ApplyTerrainOverlay(mapView, (pos, floor) =>
                floor ? new GameObject(pos, 0, null) :
                    new GameObject(pos, 0, null, false, false));

            return TransformedMap!; // Known to not be null since GenerateMap was called first
        }

        public Map GenerateMap()
        {
            BaseMap = new Map(_generator.Context.Width, _generator.Context.Height, 1, Distance.Chebyshev,
                              entityLayersSupportingMultipleItems:uint.MaxValue);

            TransformedMap = new Map(_generator.Context.Width, _generator.Context.Height, 1, Distance.Chebyshev,
                entityLayersSupportingMultipleItems:uint.MaxValue);

            // Set entire map to walls
            var allWalls = new LambdaMapView<IGameObject>(BaseMap.Width, BaseMap.Height, pos => new GameObject(pos, 0, null, false, false));
            BaseMap.ApplyTerrainOverlay(allWalls);
            TransformedMap.ApplyTerrainOverlay(allWalls);


            return BaseMap;
        }
    }
}
