using System;
using System.Collections.Generic;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Base class for implementing custom map generation steps.
    /// </summary>
    public abstract class GenerationStep
    {
        private readonly Type[] _requiredComponents;

        /// <summary>
        /// Components that are required and enforced to be on the <see cref="GenerationContext"/> when it is passed to <see cref="OnPerform(GenerationContext)"/>.
        /// </summary>
        public IEnumerable<Type> RequiredComponents => _requiredComponents;

        /// <summary>
        /// Creates a generation step that requires the given component(s) on the <see cref="GenerationContext"/> to function.
        /// </summary>
        /// <param name="requiredComponents">Components that <see cref="OnPerform(GenerationContext)"/> will use from the context.</param>
        public GenerationStep(params Type[] requiredComponents)
        {
            _requiredComponents = requiredComponents;
        }

        /// <summary>
        /// Performs the generation step on the given map context.  Throws exception if a required component is missing.
        ///
        /// This function is not virtual -- to implement actual geneation logic, implement <see cref="OnPerform(GenerationContext)"/>.
        /// </summary>
        /// <param name="context">Context to perform the generation step on.</param>
        public void PerformStep(GenerationContext context)
        {
            foreach (var componentType in _requiredComponents)
            {
                if (!context.HasComponent(componentType))
                    throw new InvalidOperationException($"Map generation step {GetType().Name} requires component of type {componentType.Name} in the context it is given, but no component of that type was found.");
            }

            OnPerform(context);
        }

        /// <summary>
        /// Implement to perform the actual work of the generation step.
        /// </summary>
        /// <param name="context">Context to perform the generation step on.</param>
        protected abstract void OnPerform(GenerationContext context);
    }
}
