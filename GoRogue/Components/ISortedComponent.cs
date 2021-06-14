using JetBrains.Annotations;

namespace GoRogue.Components
{
    /// <summary>
    /// Interface that you may optionally implement on objects added to a <see cref="IComponentCollection"/>
    /// (for example, <see cref="ComponentCollection"/>) that enforces an order in which components are returned from
    /// functions that retrieve components.
    /// </summary>
    /// <remarks>
    /// When functions that return components are called on <see cref="ComponentCollection"/> or some other
    /// <see cref="IComponentCollection"/>, components with a lower <see cref="SortOrder"/> are returned before
    /// components with a higher one.  Components that do not implement <see cref="ISortedComponent"/> are returned
    /// after any that do.
    ///
    /// This can be useful to enforce that certain types of or instances of components are processed before some other
    /// type of or instance of components.
    /// </remarks>
    [PublicAPI]
    public interface ISortedComponent
    {
        /// <summary>
        /// Value indicating the relative ordering of this component.  A lower value cause a component to be retrieved
        /// before any components of the same type with a higher value.
        /// </summary>
        uint SortOrder { get; }
    }
}
