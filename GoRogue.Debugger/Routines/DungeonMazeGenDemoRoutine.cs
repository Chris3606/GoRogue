using System.Collections.Generic;
using GoRogue.MapGeneration;
using GoRogue.MapViews;
using JetBrains.Annotations;

namespace GoRogue.Debugger.Routines
{
    [UsedImplicitly]
    public class DungeonMazeGenDemoRoutine : MapGenDemoRoutine
    {
        public DungeonMazeGenDemoRoutine()
            : base("Dungeon Maze Map Generation")
        {}

        /// <inheritdoc />
        public override void CreateViews()
        {
            views.Add(("Wall-Floor", new LambdaMapView<char>(Map.Width, Map.Height, WallFloorView)));
        }

        /// <inheritdoc />
        protected override IEnumerable<GenerationStep> GenerationSteps()
            => DefaultAlgorithms.DungeonMazeMapSteps();

        /// <inheritdoc />
        protected override void UpdateMap()
        {
            var wallFloorView = generator.Context.GetFirstOrDefault<IMapView<bool>>();
            if (wallFloorView == null)
                Map.SetToDefault();
            else
                Map.ApplyOverlay(new LambdaTranslationMap<bool,TileState>(wallFloorView,
                    val => val ? TileState.Floor : TileState.Wall));
        }
    }
}
