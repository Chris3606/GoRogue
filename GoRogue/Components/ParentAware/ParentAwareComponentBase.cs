using System;
using System.Linq;
using JetBrains.Annotations;

namespace GoRogue.Components.ParentAware
{
    /// <summary>
    ///  EventArguments used for the <see cref="ParentAwareComponentBase.Removed"/> event.
    /// </summary>
    /// <typeparam name="T">The type of the parent being passed.</typeparam>
    [PublicAPI]
    public class ParentAwareComponentRemovedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// The parent from which the object was detached.
        /// </summary>
        public readonly T OldParent;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="oldParent">The parent from which the object was detached.</param>
        public ParentAwareComponentRemovedEventArgs(T oldParent)
        {
            OldParent = oldParent;
        }
    }

    /// <summary>
    /// Simple (and optional) base class for components attached to a class implementing
    /// <see cref="IObjectWithComponents"/>.  Adds useful events and some helper functions to allow performing
    /// type-checking of parent, or requiring that the object it's attached to has or does not have certain types of
    /// components.
    /// </summary>
    /// <remarks>
    /// This class may be added as a component to any class that implements <see cref="IObjectWithComponents"/>, however components
    /// inheriting from this class will not implicitly fail in any way if it is used with a class that does not implement that interface.
    /// Certain functions such as <see cref="IncompatibleWith{TComponent}"/> do require that their parent implement IObjectWithComponents,
    /// and in general there is no guarantee that the Parent field gets updated if the parent doesn't implement that interface; however
    /// you can sync the parent field manually as applicable.
    ///
    /// In most cases, the intended usage is to add this as a component to a class that implements <see cref="IObjectWithComponents"/>;
    /// however the option of not doing so and manually syncing the Parent field instead allows this functionality to be tied into other
    /// component systems as well.
    /// </remarks>
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
        public event EventHandler<ParentAwareComponentRemovedEventArgs<object>>? Removed;

        private object? _parent;
        /// <summary>
        /// The object the component is attached to.
        /// </summary>
        public virtual object? Parent
        {
            get => _parent;
            set
            {
                if (value == _parent) return;

                if (value == null)
                {
                    var oldValue = _parent;
                    _parent = value;
                    // Null for oldValue is not possible because value == null AND value != _parent
                    Removed?.Invoke(this, new ParentAwareComponentRemovedEventArgs<object>(oldValue!));
                }
                else
                {
                    if (_parent != null)
                        throw new Exception($"Components of type {nameof(ParentAwareComponentBase)} " +
                                            "can't be attached to multiple objects simultaneously.");

                    _parent = value;
                    Added?.Invoke(this, EventArgs.Empty);
                }
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
        /// <remarks>
        /// This function only works if the parent is of type IObjectWithComponents.  If the parent is not of that
        /// type, this function will throw an exception.
        /// </remarks>
        /// <typeparam name="TComponent">Type of the component this one is incompatible with.</typeparam>
        /// <param name="s"/>
        /// <param name="e"/>
        public void IncompatibleWith<TComponent>(object? s, EventArgs e)
            where TComponent : class
        {
            var parent = Parent as IObjectWithComponents;
            if (parent == null)
                throw new Exception($"{nameof(IncompatibleWith)} can only be used with components that are attached to " +
                                                                       $"objects that implement {nameof(IObjectWithComponents)}.");

            if (parent.GoRogueComponents.GetAll<TComponent>().Any(i => !ReferenceEquals(this, i)))
                throw new Exception($"{GetType().Name} components are marked as incompatible with {typeof(TComponent).Name} components, so the component couldn't be added.");
        }
    }

    /// <summary>
    /// Optional base class for components attached to a class implementing <see cref="IObjectWithComponents"/>.
    /// Adds all functionality of <see cref="ParentAwareComponentBase"/>, and additionally type-checks the object it's
    /// attached to to make sure it is of the given type.  It also exposes its <see cref="Parent"/> property as that type
    /// instead of object.
    /// </summary>
    /// <remarks>
    /// Like <see cref="ParentAwareComponentBase"/>, subclasses of this class may be added as a component to any class that implements
    /// <see cref="IObjectWithComponents"/>, however components inheriting from this class will not implicitly fail in any way if it is
    /// used with a class that does not implement that interface.  Certain functions such as
    /// <see cref="ParentAwareComponentBase.IncompatibleWith{TComponent}"/> do require that their parent implement IObjectWithComponents,
    /// and in general there is no guarantee that the Parent field gets updated if the parent doesn't implement that interface; however
    /// you can sync the parent field manually as applicable.
    ///
    /// In most cases, the intended usage is to add this as a component to a class that implements <see cref="IObjectWithComponents"/>;
    /// however the option of not doing so and manually syncing the Parent field instead allows this functionality to be tied into other
    /// component systems as well.
    /// </remarks>
    /// <typeparam name="TParent">Type of the component's parent.</typeparam>
    [PublicAPI]
    public class ParentAwareComponentBase<TParent> : ParentAwareComponentBase where TParent : class
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
        /// Fires when the component is unattached from an object
        /// </summary>
        public new event EventHandler<ParentAwareComponentRemovedEventArgs<TParent>>? Removed;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ParentAwareComponentBase()
        {
            Added += ParentTypeCheck<TParent>;
            base.Removed += OnRemoved;
        }

        private void OnRemoved(object? sender, ParentAwareComponentRemovedEventArgs<object> e)
            => Removed?.Invoke(sender, new ParentAwareComponentRemovedEventArgs<TParent>((TParent)e.OldParent));
    }
}
