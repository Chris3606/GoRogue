using System.Collections.Generic;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// _grid generator that applies a series of <see cref="GenerationStep" /> instances to a
    /// <see cref="GenerationContext" /> to generate a map.
    /// </summary>
    [PublicAPI]
    public class Generator
    {
        private readonly List<GenerationStep> _generationSteps;

        /// <summary>
        /// Context for the map this <see cref="Generator" /> is generating.
        /// </summary>
        public readonly GenerationContext Context;

        /// <summary>
        /// Creates a generator that will be used to generate a map of the given width/height.
        /// </summary>
        /// <param name="width">Width of the generated map.</param>
        /// <param name="height">Height of the generated map.</param>
        public Generator(int width, int height)
        {
            Context = new GenerationContext(width, height);
            _generationSteps = new List<GenerationStep>();
        }

        /// <summary>
        /// Steps used to generate the map.
        /// </summary>
        public IReadOnlyList<GenerationStep> GenerationSteps => _generationSteps.AsReadOnly();

        /// <summary>
        /// Adds a component to the context this generator is applying generation steps to.
        /// </summary>
        /// <param name="component">Component to add to the map context.</param>
        /// <param name="tag">An optional tag to give the component.  Defaults to no tag.</param>
        /// <returns>This generator (for chaining).</returns>
        public Generator AddComponent(object component, string? tag = null)
        {
            Context.Add(component, tag);
            return this;
        }

        /// <summary>
        /// Adds a generation step.  Steps are executed in the order they are added.
        /// </summary>
        /// <param name="step">The generation step to add.</param>
        /// <returns>This generator (for chaining).</returns>
        public Generator AddStep(GenerationStep step)
        {
            _generationSteps.Add(step);
            return this;
        }

        /// <summary>
        /// Adds the given generation steps.  Steps are executed in the order they are added.
        /// </summary>
        /// <param name="steps">The generation steps to add.</param>
        /// <returns>This generator (for chaining).</returns>
        public Generator AddSteps(params GenerationStep[] steps) => AddSteps((IEnumerable<GenerationStep>)steps);

        /// <summary>
        /// Adds the given generation steps.  Steps are executed in the order they are added.
        /// </summary>
        /// <param name="steps">The generation steps to add.</param>
        /// <returns>This generator (for chaining).</returns>
        public Generator AddSteps(IEnumerable<GenerationStep> steps)
        {
            _generationSteps.AddRange(steps);
            return this;
        }

        /// <summary>
        /// Applies the generation steps added, in the order in which they were added. to the <see cref="Context" /> to
        /// generate the map.
        /// </summary>
        /// <returns>This generator (for chaining).</returns>
        public Generator Generate()
        {
            foreach (var step in _generationSteps)
                step.PerformStep(Context);

            return this;
        }

        /// <summary>
        /// Returns an enumerator that, when evaluated to completion, performs each stage sequentially (as defined
        /// by the generation steps in their implementation); one stage per MoveNext call.
        /// Typically you will want to use <see cref="Generate"/> instead.
        /// </summary>
        /// <remarks>
        /// For traditional cases, you will want to call the <see cref="Generate"/> function which simply completes
        /// all steps.  However, if you want to visually examine each stage of the generation algorithm, you can call
        /// this function, then call the resulting enumerator's MoveNext function each time you want to complete a
        /// stage.  This can be useful for demonstration purposes and debugging.
        /// </remarks>
        /// <returns>
        /// An enumerator that will complete a stage of the generation step each time its MoveNext function
        /// is called.
        /// </returns>
        public IEnumerator<object?> GetStageEnumerator()
        {
            foreach (var step in _generationSteps)
            {
                var stepEnumerator = step.GetStageEnumerator(Context);
                bool hasNext;
                do
                {
                    hasNext = stepEnumerator.MoveNext();
                    if (hasNext) // If we're past the end, we don't want to introduce an additional stopping point
                        yield return null;
                } while (hasNext);
            }
        }
    }
}
