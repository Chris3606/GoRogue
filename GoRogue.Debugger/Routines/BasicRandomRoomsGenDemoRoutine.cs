using System.Collections.Generic;
using GoRogue.MapGeneration;
using GoRogue.MapGeneration.ContextComponents;
using JetBrains.Annotations;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    [UsedImplicitly]
    public class BasicRandomRoomsGenDemoRoutine : MapGenDemoRoutine
    {
        public BasicRandomRoomsGenDemoRoutine()
            : base("Basic Random Rooms Map Generation")
        { }

        /// <inheritdoc />
        public override void CreateViews()
        {
            views.Add(("Wall-Floor", new LambdaGridView<char>(Map.Width, Map.Height, BasicDungeonView)));
        }

        /// <inheritdoc />
        protected override IEnumerable<GenerationStep> GenerationSteps()
            => DefaultAlgorithms.BasicRandomRoomsMapSteps();

        /// <inheritdoc />
        protected override void SetInitialMapValues(ISettableGridView<TileState> map) => map.Fill(TileState.Wall);

        /// <inheritdoc />
        protected override void UpdateMap()
        {
            // Apply current wall-floor state
            var wallFloorView = generator.Context.GetFirst<IGridView<bool>>();
            Map.ApplyOverlay(new LambdaTranslationGridView<bool, TileState>(wallFloorView,
                val => val ? TileState.Floor : TileState.Wall));

            // Apply doors, if there are any
            var doorList = generator.Context.GetFirstOrDefault<DoorList>();
            if (doorList is null) // No doors yet
                return;

            foreach (var (_, doorsPerRoom) in doorList.DoorsPerRoom)
            {
                foreach (var door in doorsPerRoom.Doors)
                    Map[door] = TileState.Door;
            }
        }
    }
}
