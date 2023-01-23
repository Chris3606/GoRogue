using System.Collections.Generic;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger.Routines
{
    public class ScratchpadRoutine : IRoutine
    {

        public string Name => "Scratchpad Routine";

        private readonly List<(string name, IGridView<char> view)> _views = new List<(string name, IGridView<char> view)>();

        public IReadOnlyList<(string name, IGridView<char> view)> Views => _views.AsReadOnly();

        public void NextTimeUnit()
        { }

        public void LastTimeUnit()
        { }

        public void GenerateMap()
        {
        }

        public void CreateViews()
        {
            _views.Add(("Test View", new ArrayView<char>(10, 10)));
        }

        public void InterpretKeyPress(int key) { }
    }
}
