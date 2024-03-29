﻿using System;
using System.Collections.Generic;
using System.Linq;
using GoRogue.Components;
using JetBrains.Annotations;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// Raised by generation steps in <see cref="GenerationStep.OnPerform(GenerationContext)" /> when a parameter has been
    /// misconfigured.
    /// </summary>
    [PublicAPI]
    public class InvalidConfigurationException : Exception
    {
        /// <summary>
        /// Creates a configuration exception with a customized message.
        /// </summary>
        /// <param name="message" />
        public InvalidConfigurationException(string message)
            : base(message)
        { }

        /// <summary>
        /// Creates a configuration exception with a customized message an inner exception.
        /// </summary>
        /// <param name="message" />
        /// <param name="innerException" />
        public InvalidConfigurationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Creates a configuration exception with a helpful message.
        /// </summary>
        /// <param name="step">The generation step that the misconfigured parameter was encountered in.</param>
        /// <param name="parameterName">The name of the misconfigured parameter.</param>
        /// <param name="message">A message explaining the requirements for the parameter's value.</param>
        public InvalidConfigurationException(GenerationStep step, string parameterName, string message)
            : base("Invalid configuration encountered for generation step parameter:\n" +
                   $"    Generation Step: ${step.GetType().Name} (name: {step.Name})\n" +
                   $"    Parameter Name : ${parameterName}\n" +
                   $"    Message        : ${message}")
        {
            ParameterName = parameterName;
            Step = step;
        }

        /// <summary>
        /// Creates an empty configuration exception.
        /// </summary>
        public InvalidConfigurationException()
        { }

        /// <summary>
        /// Name of parameter that was misconfigured.
        /// </summary>
        public string? ParameterName { get; }

        /// <summary>
        /// Generation step that had a misconfigured parameter.
        /// </summary>
        public GenerationStep? Step { get; }
    }

    /// <summary>
    /// Raised by <see cref="GenerationStep" /> when required components are not present when
    /// <see cref="GenerationStep.PerformStep(GenerationContext)" /> is called.
    /// </summary>
    [PublicAPI]
    public class MissingContextComponentException : Exception
    {
        /// <summary>
        /// Tag of the required component that was not found, or null if no tag was required.
        /// </summary>
        public readonly string? RequiredComponentTag;

        /// <summary>
        /// Type of the required component that was not found.
        /// </summary>
        public readonly Type? RequiredComponentType;

        /// <summary>
        /// Generation step that failed to find its required components.
        /// </summary>
        public readonly GenerationStep? Step;

        /// <summary>
        /// Creates a new exception with a helpful error message.
        /// </summary>
        /// <param name="step">Generation step that failed to find its required components.</param>
        /// <param name="requiredComponentType">Type of the required component that was not found.</param>
        /// <param name="requiredComponentTag">Tag of the required component that was not found, or null if no tag was required.</param>
        public MissingContextComponentException(GenerationStep step, Type requiredComponentType,
                                                string? requiredComponentTag)
            : base("Generation step was performed on a context that did not have the required components:\n" +
                   $"    Generation Step   : {step.GetType().Name} (name: {step.Name})\n" +
                   $"    Required Component: {requiredComponentType.Name} " +
                   (requiredComponentTag != null ? $"(tag: {requiredComponentTag})" : "") + "\n")
        {
            Step = step;
            RequiredComponentType = requiredComponentType;
            RequiredComponentTag = requiredComponentTag;
        }

        /// <summary>
        /// Creates an exception with a fully customized message.
        /// </summary>
        /// <param name="message" />
        public MissingContextComponentException(string message)
            : base(message)
        { }

        /// <summary>
        /// Creates an exception with a fully customized message and inner exception.
        /// </summary>
        /// <param name="message" />
        /// <param name="innerException" />
        public MissingContextComponentException(string message, Exception innerException)
            : base(message, innerException)
        { }

        /// <summary>
        /// Creates an empty exception.
        /// </summary>
        public MissingContextComponentException()
        { }
    }

    /// <summary>
    /// Raised by map generation steps if they detect that an invalid state (that can occasionally happen) is detected,
    /// and the map should be regenerated.  Automatically used by the <see cref="Generator.ConfigAndGenerateSafe"/> and
    /// <see cref="Generator.ConfigAndGetStageEnumeratorSafe"/> functions.
    /// </summary>
    [PublicAPI]
    public class RegenerateMapException : Exception
    {
        /// <summary>
        /// Creates map regeneration exception with no message.
        /// </summary>
        public RegenerateMapException()
        { }

        /// <summary>
        /// Creates a map regeneration exception with a customized message.
        /// </summary>
        /// <param name="message" />
        public RegenerateMapException(string message)
            : base(message + "  This exception is expected for some generation steps; consider using ConfigAndGenerateSafe to automatically handle it.")
        { }

        /// <summary>
        /// Creates a map regeneration exception with a customized message an inner exception.
        /// </summary>
        /// <param name="message" />
        /// <param name="innerException" />
        public RegenerateMapException(string message, Exception innerException)
            : base(message + "  This exception is expected for some generation steps; consider using ConfigAndGenerateSafe to automatically handle it.", innerException)
        { }
    }

    // TODO: Figure out way to check for tags AND types that are the same (for some generation steps)?  This wrecks ClosestMapAreaConnector
    /// <summary>
    /// Base class for implementing custom map generation steps.
    /// </summary>
    [PublicAPI]
    public abstract class GenerationStep
    {
        private readonly ComponentTypeTagPair[] _requiredComponents;

        /// <summary>
        /// The name of the generation step.
        /// </summary>
        public readonly string Name;

        // This constructor is required to remove ambiguous constructor call issues because both of the other ones use params
        /// <summary>
        /// Creates a generation step, optionally with a custom name.
        /// </summary>
        /// <param name="name">The name of the generation step being created.  Defaults to the name of the (runtime) class.</param>
        protected GenerationStep(string? name = null)
            : this(name, Array.Empty<Type>())
        { }

        /// <summary>
        /// Creates a generation step that requires the given component(s) on the <see cref="GenerationContext" /> to
        /// function.
        /// </summary>
        /// <param name="name">
        /// The name of the generation step being created.  Defaults to the name of the (runtime) class.
        /// </param>
        /// <param name="requiredComponents">
        /// Components (and associated tags) that <see cref="OnPerform(GenerationContext)" /> will require from the
        /// context.  Null specified as a tag means no particular tag is required; only a component of the given type.
        /// </param>
        protected GenerationStep(string? name = null, params Type[] requiredComponents)
            : this(name, requiredComponents.Select(type => new ComponentTypeTagPair(type, null)).ToArray())
        { }

        /// <summary>
        /// Creates a generation step that requires the given component(s) on the <see cref="GenerationContext" /> to function.
        /// </summary>
        /// <param name="requiredComponents">
        /// Components that <see cref="OnPerform(GenerationContext)" /> will require from the context, and the tag
        /// required for each component.  Null means no particular tag is required.
        /// </param>
        /// <param name="name">The name of the generation step being created.  Defaults to the name of the (runtime) class.</param>
        protected GenerationStep(string? name = null, params ComponentTypeTagPair[] requiredComponents)
        {
            Name = name ?? GetType().Name;
            _requiredComponents = requiredComponents;
        }

        /// <summary>
        /// Components that are required and enforced to be on the <see cref="GenerationContext" /> when it is passed to
        /// <see cref="OnPerform(GenerationContext)" />.
        /// Each component may optionally have a required tag.
        /// </summary>
        public IEnumerable<ComponentTypeTagPair> RequiredComponents => _requiredComponents;

        /// <summary>
        /// Performs the generation step on the given map context.  Throws exception if a required component is missing.
        /// This function is not virtual -- to implement actual generation logic, implement
        /// <see cref="OnPerform(GenerationContext)" />.
        /// </summary>
        /// <param name="context">Context to perform the generation step on.</param>
        public void PerformStep(GenerationContext context)
        {
            // Ensure required components exist
            CheckForRequiredComponents(context);

            // Evaluate entire enumerator to complete the entire step
            var enumerator = OnPerform(context);
            bool isNext;
            do
            {
                isNext = enumerator.MoveNext();
            } while (isNext);
        }

        /// <summary>
        /// Return an enumerator that, when evaluated to completion, will perform each "stage" of the generation step
        /// sequentially.
        /// </summary>
        /// <param name="context">Context to perform the generation step on.</param>
        /// <returns>An enumerator which, when evaluated, performs each stage of the step sequentially.</returns>
        public IEnumerator<object?> GetStageEnumerator(GenerationContext context)
        {
            // Check for required components
            CheckForRequiredComponents(context);

            // Return enumerator which, when evaluated, will perform each stage of the step.
            return OnPerform(context);
        }

        /// <summary>
        /// Implement to perform the actual work of the generation step.  Use "yield return null" to indicate the end
        /// of a "stage", eg. a point where execution can be paused when using
        /// <see cref="Generator.GetStageEnumerator"/>.
        /// </summary>
        /// <param name="context">Context to perform the generation step on.</param>
        protected abstract IEnumerator<object?> OnPerform(GenerationContext context);

        private void CheckForRequiredComponents(GenerationContext context)
        {
            foreach (var (componentType, tag) in _requiredComponents)
                if (!context.Contains(componentType, tag))
                    throw new MissingContextComponentException(this, componentType, tag);
        }
    }
}
