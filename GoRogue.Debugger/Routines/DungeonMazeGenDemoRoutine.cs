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
            views.Add(("Wall-Floor", new LambdaGridView<char>(Map.Width, Map.Height, WallFloorView)));
        }

        /// <inheritdoc />
        protected override IEnumerable<GenerationStep> GenerationSteps()
            => DefaultAlgorithms.DungeonMazeMapSteps();

        /// <inheritdoc />
        protected override void UpdateMap()
        {
            var wallFloorView = generator.Context.GetFirst<IGridView<bool>>();
            Map.ApplyOverlay(new LambdaTranslationGridView<bool,TileState>(wallFloorView,
                val => val ? TileState.Floor : TileState.Wall));
        }

        /// <inheritdoc />
        protected override void SetInitialMapValues(ISettableGridView<TileState> map)
            => map.Fill(TileState.Wall);
    }
}
