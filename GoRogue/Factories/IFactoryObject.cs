namespace GoRogue.Factories
{
    /// <summary>
    /// Interface that can optionally be implemented by objects created via a <see cref="Factory{TProduced}"/> or <see cref="AdvancedFactory{TBlueprintConfig, TProduced}"/>.  The <see cref="DefinitionId"/> property
    /// will be automatically set to the ID of the blueprint used to create the object when the factory's Create function is called.
    /// </summary>
    public interface IFactoryObject
    {
        /// <summary>
        /// The identifier of the blueprint that created this object. Do not set manually -- the factory
        /// will automatically set this field when the object is created.
        /// </summary>
        string DefinitionId { get; set; }
    }
}
