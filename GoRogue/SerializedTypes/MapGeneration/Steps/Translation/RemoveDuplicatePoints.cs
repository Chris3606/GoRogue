using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using GoRogue.MapGeneration.Steps.Translation;
using JetBrains.Annotations;

namespace GoRogue.SerializedTypes.MapGeneration.Steps.Translation
{
    /// <summary>
    /// Serializable (pure-data) object representing a <see cref="RemoveDuplicatePoints"/>
    /// </summary>
    [PublicAPI]
    [DataContract]
    [SuppressMessage("ReSharper", "CA1815")] // Type should only be used for serialization
    public struct RemoveDuplicatePointsSerialized
    {
        /// <summary>
        /// Name of the generation step.
        /// </summary>
        [DataMember] public string Name;

        /// <summary>
        /// Tag that must be associated with the component used as the area list from which duplicates are removed.
        /// </summary>
        [DataMember] public string ModifiedAreaListTag;

        /// <summary>
        /// Tag that must be associated with the component used as the unmodified area list.
        /// </summary>
        [DataMember] public string UnmodifiedAreaListTag;

        /// <summary>
        /// Converts <see cref="RemoveDuplicatePoints"/> to <see cref="RemoveDuplicatePointsSerialized"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        public static implicit operator RemoveDuplicatePointsSerialized(RemoveDuplicatePoints step)
            => FromRemoveDuplicatePoints(step);

        /// <summary>
        /// Converts <see cref="RemoveDuplicatePointsSerialized"/> to <see cref="RemoveDuplicatePoints"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        public static implicit operator RemoveDuplicatePoints(RemoveDuplicatePointsSerialized step)
            => step.ToRemoveDuplicatePoints();

        /// <summary>
        /// Converts <see cref="RemoveDuplicatePoints"/> to <see cref="RemoveDuplicatePointsSerialized"/>.
        /// </summary>
        /// <param name="step"/>
        /// <returns/>
        [SuppressMessage("ReSharper", "CA1000")] // Static method is required to implement implicit ops
        public static RemoveDuplicatePointsSerialized FromRemoveDuplicatePoints(RemoveDuplicatePoints step)
            => new RemoveDuplicatePointsSerialized
            {
                Name = step.Name,
                UnmodifiedAreaListTag = step.UnmodifiedAreaListTag,
                ModifiedAreaListTag = step.ModifiedAreaListTag
            };

        /// <summary>
        /// Converts <see cref="RemoveDuplicatePointsSerialized"/> to <see cref="RemoveDuplicatePoints"/>.
        /// </summary>
        /// <returns/>
        public RemoveDuplicatePoints ToRemoveDuplicatePoints()
            => new RemoveDuplicatePoints(Name, UnmodifiedAreaListTag, ModifiedAreaListTag);
    }
}
