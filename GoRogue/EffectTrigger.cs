using System.Collections.Generic;

namespace GoRogue
{
	/// <summary>
	/// Represents an "event" that can automatically trigger and manage one or more
	/// <see cref="Effect{TriggerArgs}"/> instances, and acts as part of the implementation of
	/// duration in Effect.
	/// </summary>
	/// <remarks>
	/// EffectTrigger's primary purpose is to represent an event that can trigger one or more effects, and
	/// automatically remove those effects from the list when their duration reaches 0.  Each EffectTrigger
	/// instance can have one or more (non-instantaneous) effects added to it. All Effects must take the same
	/// type of argument to their <see cref="Effect{TriggerArgs}.Trigger(TriggerArgs)"/>
	/// function, as specified by this class's type parameter.
	///
	/// Each time the <see cref="TriggerEffects(TriggerArgs)"/> function is called, every Effect has its
	/// Trigger function called (provided its duration is not 0). Each Effect may, via the TriggerArgs
	/// <see cref="EffectArgs.CancelTrigger"/> member, stop the effect from
	/// being sent to subsequent effects in the EffectTrigger's list. Once either all effects in the list
	/// have had their Trigger function called, or some effect has cancelled the triggering, any effect
	/// whose duration has reached 0 is removed from the EffectTrigger automatically.
	/// 
	/// Typically, one instance of this class is created per "event" that can trigger effects, and then the
	/// instance's TriggerEffects function is called whenever that event happens. For example, in a
	/// typical roguelike, all damageable creatures might have an instance of this class called
	/// OnDamageTakenEffects. Any effect that should trigger when that creature takes damage would then be
	/// added to that creature's OnDamageTakenEffects EffectTrigger. The TakeDamage function of that
	/// creature would then need to call OnDamageTakenEffects.TriggerEffects(...). In this way, all effects
	/// added to the OnDamageTakenEffects EffectTrigger would be triggered automatically whenever the
	/// creature takes damage.
	///
	/// For some complex game mechanics, it may be desireable to control how effects stack, the order they appear
	/// in the effects list of EffectTriggers, etc. In these cases, subclassing EffectTrigger and overriding the
	/// add and remove functions can allow this functionality.
	/// </remarks>
	/// <typeparam name="TriggerArgs">
	/// The type of argument that must be accepted by the <see cref="Effect{TriggerArgs}.Trigger(TriggerArgs)"/> 
	/// function of any Effect added to this EffectTrigger.
	/// </typeparam>
	public class EffectTrigger<TriggerArgs> where TriggerArgs : EffectArgs
	{
		private List<Effect<TriggerArgs>> _effects;

		/// <summary>
		/// Constructor.
		/// </summary>
		public EffectTrigger()
		{
			_effects = new List<Effect<TriggerArgs>>();
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
		/// Yields a string representation of each effect that has been added to the effect trigger.
		/// </summary>
		/// <returns>
		/// A string representation of each effect that has been added to the effect trigger.
		/// </returns>
		public override string ToString() => _effects.ExtendToString();

		/// <summary>
		/// Calls the <see cref="Effect{TriggerArgs}.Trigger(TriggerArgs)"/> function of each effect
		/// in the <see cref="Effects"/> list (as long as its duration is not 0), then
		/// removes any effect that has duration 0.
		/// </summary>
		/// <remarks>
		/// The argument given is passed along to the <see cref="Effect{TriggerArgs}.Trigger(TriggerArgs)"/>
		/// function of each effect that has Trigger called. If some effect sets the <see cref="EffectArgs.CancelTrigger"/>
		/// flag in the argument to true, the loop will be broken and no subsequent effects in the list will have
		/// Trigger called. After either this occurs or all effects have had Trigger called, any effect in the list
		/// that has a duration of 0 is automatically removed from the list.  It is valid to pass null
		/// as the argument to this function, if the effects need no actual parameters.
		/// </remarks>
		/// <param name="args">Argument to pass to the <see cref="Effect{TriggerArgs}.Trigger(TriggerArgs)"/> function
		/// of each effect.</param>
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

			_effects.RemoveAll(eff => eff.Duration == 0);
		}
	}
}
