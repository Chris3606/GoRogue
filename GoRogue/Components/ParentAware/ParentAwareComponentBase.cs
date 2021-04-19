using System;
using System.Linq;
using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    /// Simple (and optional) base class for components attached to a class implementing
    /// <see cref="IObjectWithComponents"/>.  Adds useful events and some helper functions to allow performing
    /// type-checking of parent, or requiring that the object it's attached to has or does not have certain types of
    /// components.
    /// </summary>
    [PublicAPI]
    public class ParentAwareComponentBase : IParentAwareComponent
    {
        /// <summary>
        /// Fires when the component is attached to an object.
        /// </summary>
        public event EventHandler? Added;

        /// <summary>
        /// Fires when the component is unattached from an object
        /// </summary>
        public event EventHandler? Removed;

        private IObjectWithComponents? _parent;
        /// <summary>
        /// The object the component is attached to.
        /// </summary>
        public virtual IObjectWithComponents? Parent
        {
            get => _parent;
            set
            {
                if (value == null)
                {
                    _parent = value;
                    Removed?.Invoke(this, EventArgs.Empty);
                }
                else if (_parent != null)
                    throw new Exception($"{nameof(ParentAwareComponentBase)} components inherit from " +
                                        $"{nameof(IParentAwareComponent)}, so they can't be attached to multiple " +
                                        "objects simultaneously.");

                _parent = value;
                Added?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Add as a handler to <see cref="Added"/> to enforce that this component must be parented to an object that
        /// inherits from/implements <typeparamref name="TParent"/>.
        /// </summary>
        /// <typeparam name="TParent">Type of object that must be this component's parent.</typeparam>
        /// <param name="s" />
        /// <param name="e" />
        public static void ParentTypeCheck<TParent>(object? s, EventArgs e)
        {
            // Null override because we control when these events are sent
            var componentBase = (ParentAwareComponentBase)s!;
            if (componentBase.Parent is TParent)
                return;

            throw new Exception($"{componentBase.GetType().Name} components are marked with a " +
                                $"{nameof(ParentTypeCheck)}, so can only be attached to something that inherits " +
                                $"from/implements {typeof(TParent).Name}.");
        }

        /// <summary>
        /// Add as a handler to <see cref="Added"/> to enforce that this component may not be added to an object that
        /// has a component of type <typeparamref name="TComponent"/>.  May also be used to enforce that the component
        /// can't have multiple instances of itself attached to the same object by using
        /// Added += IncompatibleWith&lt;MyOwnType&gt;.
        /// </summary>
        /// <typeparam name="TComponent">Type of the component this one is incompatible with.</typeparam>
        /// <param name="s"/>
        /// <param name="e"/>
        public void IncompatibleWith<TComponent>(object s, EventArgs e)
            where TComponent : class
        {
            if (Parent!.GoRogueComponents.GetAll<TComponent>().Any(i => !ReferenceEquals(this, i)))
                throw new Exception($"{s.GetType().Name} components are marked as incompatible with {typeof(TComponent).Name} components, so the component couldn't be added.");
        }
    }

    /// <summary>
    /// Optional base class for components attached to a a class implementing <see cref="IObjectWithComponents"/>.
    /// Adds all functionality of <see cref="ParentAwareComponentBase"/>, and additionally type-checks the object it's
    /// attached to to make sure it is of the given type.  It also exposes its <see cref="Parent"/> property as that type
    /// instead of IObjectWithComponents.
    /// </summary>
    /// <typeparam name="TParent">Type of the component's parent.</typeparam>
    [PublicAPI]
    public class ParentAwareComponentBase<TParent> : ParentAwareComponentBase where TParent : class, IObjectWithComponents
    {
        /// <summary>
        /// The object the component is attached to.
        /// </summary>
        public new virtual TParent? Parent
        {
            // Safe because of type check
            get => (TParent?)(base.Parent);
            set => base.Parent = value;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParentAwareComponentBase() => Added += ParentTypeCheck<TParent>;
    }
}
