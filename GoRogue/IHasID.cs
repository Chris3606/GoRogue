using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// Interface for a class that has an ID value, typically used for items in a spatial map, or generally
    /// for purposes of hashing.
    /// </summary>
    /// <remarks>
    /// The ID assigned should be unique or close to unique over all instances of the class (to avoid hash collisions).
    /// A typical implementation could be simply randomly generating the ID value.  To assign completely unique IDs, an
    /// <see cref="IDGenerator"/> can be used:
    /// <example>
    /// <code>
    /// class SomeClass : IHasID
    /// {
    ///     // Static instance used to assign IDs to ANY new SomeClass instance
    ///     private static IDGenerator generator = new IDGenerator();
    ///     public uint ID { get; }
    ///
    ///     public SomeClass(...)
    ///     {
    ///         ID = generator.UseID();
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    [PublicAPI]
    public interface IHasID
    {
        /// <summary>
        /// ID assigned to this object.
        /// </summary>
        uint ID { get; }
    }
}
