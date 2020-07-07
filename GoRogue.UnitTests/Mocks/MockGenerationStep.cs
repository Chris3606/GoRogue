﻿using System;
using GoRogue.MapGeneration;

namespace GoRogue.UnitTests.Mocks
{
    #region Map ContextComponents

    internal interface IMapContextComponent
    { }

    internal class MapContextComponent1 : IMapContextComponent
    { }

    internal class MapContextComponent2 : IMapContextComponent
    { }

    #endregion

    #region Generation Steps

    internal class MockGenerationStep : GenerationStep
    {
        private readonly Action? _onPerform;

        // Necessary to avoid constructor ambiguity
        public MockGenerationStep(Action? onPerform, string? name = null)
            : base(name)
            => _onPerform = onPerform;

        public MockGenerationStep(Action onPerform, string? name = null, params Type[] requiredComponents)
            : base(name, requiredComponents)
            => _onPerform = onPerform;

        public MockGenerationStep(Action onPerform, string? name = null,
                                  params (Type type, string? tag)[] requiredComponents)
            : base(name, requiredComponents)
            => _onPerform = onPerform;

        protected override void OnPerform(GenerationContext context) => _onPerform?.Invoke();
    }

    #endregion
}