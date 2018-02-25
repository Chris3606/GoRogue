using System.Collections.Generic;

namespace GoRogue
{
    /// <summary>
    /// Represents an "event" that can trigger one or more Effects of the appropriate type. Typically
    /// instnaces of this class can simply be created, however a subclass may be required for custom
    /// add/remove actions and ordering.
    /// </summary>
    /// <remarks>
    /// EffectTrigger's primary purpose is to represent an event that can trigger one or more effects
    /// automatically, and manage the automatic removal of those effects when their duration reaches 0.
    ///
    /// Each EffectTrigger instance can have one or more non-instantaneous effects added to it. All
    /// Effects must take the same type of argument to their Trigger function, as specified by this
    /// class's TriggerArgs type parameter.
    ///
    /// Each time the EffectTrigger's TriggerEffects function is called, every added Effect has its
    /// Trigger function called (provided its duration is not 0). Each Effect may, via the
    /// TriggerArgs CancelTrigger member, stop the effect from being sent to subsequent Effects in
    /// the EffectTrigger's list.
    ///
    /// Once all effects have had Trigger called as applicable, or some effect has cancelled the
    /// trigger, any effect whose duration has reached 0 is removed from the EffectTrigger automatically.
    ///
    /// Typically, one instance of this class is created per "event" that can trigger effects, and
    /// then the instance's TriggerEffects function is called whenever that event happens. For
    /// example, in a typical roguelike, all damageable creatures might have an instance of this
    /// class called OnDamageTaken. Any effect that should trigger when that creature takes damage
    /// would then be added to that creature's OnDamageTaken EffectTrigger. The TakeDamage function
    /// of that creature would then need to call OnDamageTaken.TriggerEffects(...). In this way, all
    /// effects added to the OnDamageTaken EffectTrigger would be triggered automatically whenever
    /// the creature takes damage.
    ///
    /// For some complex game mechanics, it may be desireable to control how effects stack, the order
    /// they appear in the Effects list of EffectTriggers, etc. In these cases, subclassing
    /// EffectTrigger and overriding add/remove can allow this functionality.
    /// </remarks>
    /// <typeparam name="TriggerArgs">
    /// The type of argument that must be accepted by the Trigger function of any Effect added to
    /// this EffectTrigger.
    /// </typeparam>
    public class EffectTrigger<TriggerArgs> where TriggerArgs : EffectArgs
    {
        private List<Effect<TriggerArgs>> _effects;

        private List<int> indicesForRemoval;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EffectTrigger()
        {
            _effects = new List<Effect<TriggerArgs>>();
            indicesForRemoval = new List<int>();
        }

        /// <summary>
        /// List of all effects that are part of this EffectTrigger.
        /// </summary>
        public IReadOnlyList<Effect<TriggerArgs>> Effects { get => _effects.AsReadOnly(); }

        /// <summary>
        /// Adds the given effect to this EffectTrigger, provided the effect's duration is not 0. If
        /// the effect's duration is 0, an ArgumentException is thrown.
        /// </summary>
        /// <param name="effect">The effect to add to this trigger.</param>
        public virtual void Add(Effect<TriggerArgs> effect)
        {
            if (effect.Duration == 0)
                throw new System.ArgumentException($"Tried to add effect {effect.Name} to an EffectTrigger, but it has duration 0!", nameof(effect));

            _effects.Add(effect);
        }

        /// <summary>
        /// Removes the given effect from this EffectTrigger.
        /// </summary>
        /// <param name="effect">The effect to remove</param>
        public virtual void Remove(Effect<TriggerArgs> effect) => _effects.Remove(effect);

        /// <summary>
        /// For each effect in the list, calls its Trigger function if its duration is not 0, then
        /// remvoes any effect that has duration 0.
        /// </summary>
        /// <remarks>
        /// The argument given is passed along to the Trigger function of each effect that has
        /// Trigger called. If some effect sets the CancelTrigger flag in the argument to true, the
        /// loop will be broken and no subsequent effects in the list will have Trigger called. After
        /// either this occurs or all effects have had Trigger called, any effect in the list that
        /// has a duration of 0 is automatically removed from the list.
        ///
        /// It is valid to pass null as the argument to this function, if the effects need no actual parameters.
        /// </remarks>
        /// <param name="args">Argument to pass to the Trigger function of each effect.</param>
        public void TriggerEffects(TriggerArgs args)
        {
            foreach (var effect in _effects)
            {
                if (effect.Duration != 0)
                {
                    effect.Trigger(args);
                    if (args != null && args.CancelTrigger)
                        break;
                }
            }

            for (int i = 0; i < _effects.Count; i++)
                if (_effects[i].Duration == 0)
                    indicesForRemoval.Add(i);

            for (int i = indicesForRemoval.Count - 1; i >= 0; i--)
                _effects.RemoveAt(indicesForRemoval[i]);

            indicesForRemoval.Clear();
        }

        /// <summary>
        /// Yields a string representation of each effect that has been added to the effect trigger.
        /// </summary>
        /// <returns>A string representation of each effect that has been added to the effect trigger.</returns>
        public override string ToString() => _effects.ExtendToString();
    }
}