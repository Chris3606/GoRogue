using JetBrains.Annotations;

namespace GoRogue.Factories
{
    /// <summary>
    /// Interface that can optionally be implemented by objects created via a <see cref="Factory{TBlueprintID, TProduced}" /> or
    /// <see cref="AdvancedFactory{TBlueprintID, TBlueprintConfig, TProduced}" />.  The <see cref="DefinitionID" /> property
    /// will be automatically set to the ID of the blueprint used to create the object when the factory's Create function is
    /// called.
    /// </summary>
    [PublicAPI]
    public interface IFactoryObject<TBlueprintID>
    {
        /// <summary>
        /// The identifier of the blueprint that created this object. Do not set manually -- the factory
        /// will automatically set this field when the object is created.
        /// </summary>
        TBlueprintID DefinitionID { get; set; }
    }
}
