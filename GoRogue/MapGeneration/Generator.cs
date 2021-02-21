using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Exception thrown when maximum retries for map generation is reached.
    /// </summary>
    [PublicAPI]
    public class MapGenerationFailedException : Exception
    {
        /// <summary>
        /// Creates map gen failed exception with no message.
        /// </summary>
        public MapGenerationFailedException()
        { }

        /// <summary>
        /// Creates a map gen failed exception with a customized message.
        /// </summary>
        /// <param name="message" />
        public MapGenerationFailedException(string message)
            : base(message)
        { }

        /// <summary>
        /// Creates a map gen failed exception with a customized message an inner exception.
        /// </summary>
        /// <param name="message" />
        /// <param name="innerException" />
        public MapGenerationFailedException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Map generator that applies a series of <see cref="GenerationStep" /> instances to a
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
        /// Clears the context and generation steps, resetting the generator back to a pre-configured state.
        /// </summary>
        public void Clear()
        {
            _generationSteps.Clear();
            Context.Clear();
        }

        /// <summary>
        /// Applies the generation steps added, in the order in which they were added, to the <see cref="Context" /> to
        /// generate the map.  If you want to automatically handle <see cref="RegenerateMapException"/>, call
        /// <see cref="ConfigAndGenerateSafe"/> instead.
        /// </summary>
        /// <remarks>
        /// Depending on the generation steps used, this function may throw RegenerateMapException if it detects
        /// that the map generated does not meet invariants due to RNG, in which case the map generation will need
        /// to be performed again.  See <see cref="ConfigAndGenerateSafe"/> for a method of ensuring this happens
        /// in a convenient way.
        /// </remarks>
        /// <returns>This generator (for chaining).</returns>
        public Generator Generate()
        {
            foreach (var step in _generationSteps)
                step.PerformStep(Context);

            return this;
        }

        /// <summary>
        /// Calls the <paramref name="generatorConfig"/> function to add components/steps to the generator, then calls
        /// <see cref="Generate"/>.  If a <see cref="RegenerateMapException"/> is thrown, re-generates the map by
        /// calling the configure function then generate again, up to the maximum retries specified.
        /// </summary>
        /// <remarks>
        /// This is a safe wrapper to work with generation procedures that can get themselves into an invalid state
        /// that requires re-generating the entire map.  Generation steps are clearly marked in documentation if they
        /// can produce such states.
        ///
        /// Ensure you do NOT create/use an RNG with a static seed within this function, as it could easily create
        /// an infinite loop (that would re-generate the same invalid map over and over).
        /// </remarks>
        /// <param name="generatorConfig">Function to configure the generator.</param>
        /// <param name="maxAttempts">Maximum times to attempt map generation, before throwing a MapGenerationFailed
        /// exception.  Defaults to infinite.</param>
        /// <returns>This generator (for chaining).</returns>
        public Generator ConfigAndGenerateSafe(Action<Generator> generatorConfig, int maxAttempts = -1)
        {
            int currentAttempts = 0;
            while (maxAttempts == -1 || currentAttempts < maxAttempts)
            {
                try
                {
                    Clear();
                    generatorConfig(this);
                    Generate();
                    break;
                }
                catch (RegenerateMapException)
                { }

                currentAttempts++;
            }

            if (currentAttempts == maxAttempts)
                throw new MapGenerationFailedException("Maximum retries for regenerating map exceeded.");

            return this;
        }

        /// <summary>
        /// Returns an enumerator that, when evaluated to completion, performs each stage sequentially (as defined
        /// by the generation steps in their implementation); one stage per MoveNext call.
        /// Typically you will want to use <see cref="Generate"/> instead.  If you want to automatically handle
        /// <see cref="RegenerateMapException"/>, call <see cref="ConfigAndGetStageEnumeratorSafe"/> or
        /// <see cref="ConfigAndGenerateSafe"/> instead as applicable.
        /// </summary>
        /// <remarks>
        /// For traditional cases, you will want to call the <see cref="Generate"/> function which simply completes
        /// all steps.  However, if you want to visually examine each stage of the generation algorithm, you can call
        /// this function, then call the resulting enumerator's MoveNext function each time you want to complete a
        /// stage.  This can be useful for demonstration purposes and debugging.
        ///
        /// Note that a <see cref="RegenerateMapException"/> may be raised during this iteration, and it must be handled
        /// manually.  See <see cref="ConfigAndGetStageEnumeratorSafe"/>  for a method of handling this automatically.
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

        /// <summary>
        /// Calls the <paramref name="generatorConfig"/> function to add components/steps to the generator, then calls
        /// <see cref="GetStageEnumerator"/> and evaluates its enumerator, returning at each step.  Restarts map
        /// generation automatically if <see cref="RegenerateMapException"/> is thrown. Typically you will want
        /// to use <see cref="ConfigAndGenerateSafe"/> instead.
        /// </summary>
        /// <remarks>
        /// For traditional cases, you will want to call the <see cref="ConfigAndGenerateSafe"/> function which
        /// takes the same parameters and simply completes all steps.  However, if you want to visually examine each
        /// stage of the generation algorithm, you can call this function, then call the resulting enumerator's MoveNext
        /// function each time you want to complete a stage.  This can be useful for demonstration purposes and debugging.
        /// </remarks>
        /// <param name="generatorConfig">Function to configure the generator.</param>
        /// <param name="maxAttempts">Maximum times to attempt map generation, before throwing a MapGenerationFailed
        /// exception.  Defaults to infinite.</param>
        /// <returns>
        /// An enumerator that will complete a stage of the generation step each time its MoveNext function
        /// is called.
        /// </returns>
        public IEnumerator<object?> ConfigAndGetStageEnumeratorSafe(Action<Generator> generatorConfig,
                                                                    int maxAttempts = -1)
        {
            int currentAttempts = 0;
            while (maxAttempts == -1 || currentAttempts < maxAttempts)
            {
                // Break for step after reset if we're in the middle of an iteration
                if (Context.Count != 0 || _generationSteps.Count != 0)
                {
                    Clear();
                    generatorConfig(this);
                    yield return null;
                }
                else // Otherwise, just config (nothing to clear)
                    generatorConfig(this);

                // Enumerate, but catch exception and restart if we hit RegenerateMapException
                var stepEnumerator = GetStageEnumerator();
                bool hasNext;
                bool regenMap = false;
                do
                {
                    try
                    {
                        hasNext = stepEnumerator.MoveNext();
                    }
                    catch (RegenerateMapException)
                    {
                        regenMap = true;
                        break;
                    }

                    // If we're past the end, we don't want to introduce an additional stopping point
                    if (hasNext)
                        yield return null;

                } while (hasNext);

                // If we succeeded, just return out.  Otherwise, continue loop.  No pause is needed, as the pause occurs
                // after clear, or automatically at end of function..
                if (!regenMap)
                    break;

                currentAttempts++;
            }

            if (currentAttempts == maxAttempts)
                throw new MapGenerationFailedException("Maximum retries for regenerating map exceeded.");
        }
    }
}
