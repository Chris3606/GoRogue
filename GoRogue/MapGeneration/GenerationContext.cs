﻿using System;
using System.Diagnostics.CodeAnalysis;
using GoRogue.MapViews;

namespace GoRogue.MapGeneration
{
    /// <summary>
    /// A context object used for map generation.  Map generation steps will require and retrieve components that have been added to this context
    /// when they need to get previously generated data about the map.
    /// </summary>
    public class GenerationContext : ComponentContainer
    {
        /// <summary>
        /// Width of the map this context represents.
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Height of the map this context represents.
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Creates a map context with no components, with the given width/height values.
        /// </summary>
        /// <param name="width">The width of the map this context represents.</param>
        /// <param name="height">The height of the map this context represents.</param>
        public GenerationContext(int width, int height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Retrives a context component (optionally with a given tag), or utilizes the specified function to create a new one and adds it if an existing one does not exist.
        /// </summary>
        /// <typeparam name="TComponent">Type of component to retrieve.</typeparam>
        /// <param name="newFunc">Function to use to create a new component, if there is no existing component.</param>
        /// <param name="tag">An optional tag that must be associated with the retrieved or created component.  If null is specified, no tag is associated with a new object, and
        /// any object meeting the type requirement will be allowed as the return value.</param>
        /// <returns>An existing component of the appropriate type if one exists, or the newly created/added component if not.</returns>
        public TComponent GetComponentOrNew<TComponent>(Func<TComponent> newFunc, string? tag = null) where TComponent : notnull
        {
            TComponent contextComponent = GetComponent<TComponent>(tag);
            if (contextComponent == null)
            {
                contextComponent = newFunc();
                AddComponent(contextComponent, tag);
            }

            return contextComponent;
        }
    }
}