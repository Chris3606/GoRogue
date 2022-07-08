using System.Collections.Generic;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    [UsedImplicitly]
    public class DungeonMazeGenDemoRoutine : MapGenDemoRoutine
    {
        public DungeonMazeGenDemoRoutine()
            : base("Dungeon Maze Map Generation")
        { }

        /// <inheritdoc />
        public override void CreateViews()
        {
            views.Add(("Wall-Floor", new LambdaGridView<char>(Map.Width, Map.Height, BasicDungeonView)));
        }

        /// <inheritdoc />
        protected override IEnumerable<GenerationStep> GenerationSteps()
            => DefaultAlgorithms.DungeonMazeMapSteps();

        /// <inheritdoc />
        protected override void UpdateMap()
        {
            // Apply current wall-floor state
            var wallFloorView = generator.Context.GetFirst<IGridView<bool>>();
            Map.ApplyOverlay(new LambdaTranslationGridView<bool, MapGenTileState>(wallFloorView,
                val => val ? MapGenTileState.Floor : MapGenTileState.Wall));

            // Apply doors, if there are any
            var doorList = generator.Context.GetFirstOrDefault<DoorList>();
            if (doorList is null) // No doors yet
                return;

            foreach (var (_, doorsPerRoom) in doorList.DoorsPerRoom)
            {
                foreach (var door in doorsPerRoom.Doors)
                    Map[door] = MapGenTileState.Door;
            }
        }

        /// <inheritdoc />
        protected override void SetInitialMapValues(ISettableGridView<MapGenTileState> map)
            => map.Fill(MapGenTileState.Wall);
    }
}
