using System.Collections.Generic;
using GoRogue.MapGeneration;
using JetBrains.Annotations;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    [UsedImplicitly]
    public class CellularAutomataGenerationDemoRoutine : MapGenDemoRoutine
    {
        public CellularAutomataGenerationDemoRoutine()
            : base("Cellular Automata Map Generation")
        { }

        /// <inheritdoc />
        public override void CreateViews()
        {
            views.Add(("Wall-Floor", new LambdaGridView<char>(Map.Width, Map.Height, BasicDungeonView)));
        }

        /// <inheritdoc />
        protected override IEnumerable<GenerationStep> GenerationSteps()
            => DefaultAlgorithms.CellularAutomataGenerationSteps();

        /// <inheritdoc />
        protected override void UpdateMap()
        {
            var wallFloorView = generator.Context.GetFirst<IGridView<bool>>();
            Map.ApplyOverlay(new LambdaTranslationGridView<bool, MapGenTileState>(wallFloorView,
                val => val ? MapGenTileState.Floor : MapGenTileState.Wall));
        }

        /// <inheritdoc />
        protected override void SetInitialMapValues(ISettableGridView<MapGenTileState> map)
            => map.Fill(MapGenTileState.Wall);
    }
}
