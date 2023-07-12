using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace GoRogue.Effects
{
    /// <summary>
    /// Base class for <see cref="EffectTrigger"/> and <see cref="AdvancedEffectTrigger{TTriggerArgs}"/>.  Typically not
    /// useful unless you're creating a a custom implementation of effects and/or triggers.
    /// </summary>
    [PublicAPI]
    public class EffectTriggerBase<TEffect> where TEffect : EffectBase
    {
        /// <summary>
        /// All effects that are part of this trigger.
        /// </summary>
        protected readonly List<TEffect> EffectsList;
        /// <summary>
        /// List of all effects that are part of this trigger.
        /// </summary>
        public IReadOnlyList<TEffect> Effects => EffectsList.AsReadOnly();

        /// <summary>
        /// Constructor.
        /// </summary>
        protected EffectTriggerBase() => EffectsList = new List<TEffect>();

        /// <summary>
        /// Adds the given effect to this trigger, provided the effect's duration is not 0. If
        /// the effect's duration is 0, an ArgumentException is thrown.
        /// </summary>
        /// <param name="effect">The effect to add to this trigger.</param>
        public virtual void Add(TEffect effect)
        {
            if (effect.Duration == 0)
                throw new ArgumentException(
                    $"Tried to add effect {effect.Name} to an effect trigger, but it has duration 0!", nameof(effect));

            EffectsList.Add(effect);
        }

        /// <summary>
        /// Adds the given effects to this trigger, provided the effect's durations are not 0. If
        /// an effect's duration is 0, an ArgumentException is thrown.
        /// </summary>
        /// <param name="effects">The effects to add to this trigger.</param>
        public virtual void AddRange(IEnumerable<TEffect> effects)
        {
            foreach (var effect in effects)
                Add(effect);
        }

        /// <summary>
        /// Removes the given effect from this trigger.
        /// </summary>
        /// <param name="effect">The effect to remove</param>
        public virtual void Remove(TEffect effect) => EffectsList.Remove(effect);

        /// <summary>
        /// Removes all given effects from this trigger which match the predicate.
        /// </summary>
        /// <param name="match">The predicate to decide which effects to remove.</param>
        public void RemoveAll([InstantHandle] Predicate<TEffect> match) => EffectsList.RemoveAll(match);

        /// <summary>
        /// Yields a string representation of each effect that has been added to the effect trigger.
        /// </summary>
        /// <returns>
        /// A string representation of each effect that has been added to the effect trigger.
        /// </returns>
        public override string ToString() => EffectsList.ExtendToString();
    }
}
