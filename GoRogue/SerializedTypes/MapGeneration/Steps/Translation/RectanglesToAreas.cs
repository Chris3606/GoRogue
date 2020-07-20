using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using GoRogue.MapGeneration.Steps.Translation;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.MapGeneration.Steps.Translation
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="RectanglesToAreas"/>
    /// </summary>
    [PublicAPI]
    [DataContract]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct RectanglesToAreasSerialized
    {
        /// <summary>
        /// Name of the generation step.
        /// </summary>
        [DataMember] public string Name;

        /// <summary>
        /// Tag that must be associated with the component used to store the resulting areas.
        /// </summary>
        [DataMember] public string AreasComponentTag;

        /// <summary>
        /// Tag that must be associated with the component used as input rectangles.
        /// </summary>
        [DataMember] public string RectanglesComponentTag;

        /// <summary>
        /// Whether or not to remove the input list of rectangles from the context.
        /// </summary>
        [DataMember] public bool RemoveSourceComponent;

        /// <summary>
        /// Converts <see cref="RectanglesToAreas"/> to <see cref="RectanglesToAreasSerialized"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        public static implicit operator RectanglesToAreasSerialized(RectanglesToAreas step)
            => FromRectanglesToAreas(step);

        /// <summary>
        /// Converts <see cref="RectanglesToAreasSerialized"/> to <see cref="RectanglesToAreas"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        public static implicit operator RectanglesToAreas(RectanglesToAreasSerialized step)
            => step.ToRectanglesToAreas();

        /// <summary>
        /// Converts <see cref="RectanglesToAreas"/> to <see cref="RectanglesToAreasSerialized"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Static method is required to implement implicit ops
        public static RectanglesToAreasSerialized FromRectanglesToAreas(RectanglesToAreas step)
            => new RectanglesToAreasSerialized
            {
                Name = step.Name,
                AreasComponentTag = step.AreasComponentTag,
                RectanglesComponentTag = step.RectanglesComponentTag,
                RemoveSourceComponent = step.RemoveSourceComponent
            };

        /// <summary>
        /// Converts <see cref="RectanglesToAreasSerialized"/> to <see cref="RectanglesToAreas"/>.
        /// </summary>
        /// <returns/>
        public RectanglesToAreas ToRectanglesToAreas()
            => new RectanglesToAreas(Name, RectanglesComponentTag, AreasComponentTag)
            {
                RemoveSourceComponent = RemoveSourceComponent
            };
    }
}
