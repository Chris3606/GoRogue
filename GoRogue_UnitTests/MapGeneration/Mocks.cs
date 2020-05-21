using System;

using GoRogue.MapGeneration;

namespace GoRogue_UnitTests.MapGeneration
{
    #region Map ContextComponents
    interface IMapContextComponent { }

    class MapContextComponent1 : IMapContextComponent { }
    class MapContextComponent2 : IMapContextComponent { }
    #endregion

    #region Generation Steps
    class TestGenerationStep : GenerationStep
    {
        private readonly Action _onPerform;

        // Necessary to avoid constructor ambiguity
        public TestGenerationStep(Action onPerform, string? name = null)
            : base(name)
        {
            _onPerform = onPerform;
        }

        public TestGenerationStep(Action onPerform, string? name = null, params Type[] requiredComponents)
            : base(name, requiredComponents)
         {
            _onPerform = onPerform;
         }

        public TestGenerationStep(Action onPerform, string? name = null, params (Type type, string? tag)[] requiredComponents)
            : base(name, requiredComponents)
        {
            _onPerform = onPerform;
        }

        protected override void OnPerform(GenerationContext context)
        {
            _onPerform();
        }
    }
    #endregion
}
