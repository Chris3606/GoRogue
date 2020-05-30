using System;
using JetBrains.Annotations;

namespace GoRogue
{
    /// <summary>
    /// Default argument for any effect. Any class that is used as the template argument for an
    /// effect must inherit from this class.
    /// </summary>
    /// <remarks>
    /// These arguments allow cancellation of the triggering of a chain of effects when triggered by
    /// an <see cref="EffectTrigger{T}"/>, as detailed in that class's documentation.
    /// </remarks>
    [PublicAPI]
    public class EffectArgs
    {
        /// <summary>
        /// Whether or not the <see cref="EffectTrigger{T}"/> should stop calling all subsequent effect's
        /// <see cref="Effect{T}.Trigger(T)"/> functions. See EffectTrigger's documentation for details.
        /// </summary>
        public bool CancelTrigger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EffectArgs() => CancelTrigger = false;
    }

    /// <summary>
    /// Class designed to represent any sort of in-game effect. This could be anything from a simple
    /// physical damage effect to a heal effect or permanent effects.  These might include AOE effects,
    /// damage over time effects, or even potentially a special effect that simply boosts a stat.
    /// </summary>
    /// <remarks>
    /// Effectively, the class is nothing more than a basis for the concept of something that
    /// happens, potentially instantaneously or potentially one or more times on a certain event
    /// (beginning of a turn, end of a turn, on taking damage, etc). The standard way to use the
    /// Effect class is to create a subclass of Effect, that at the very least implements the
    /// <see cref="Effect{T}.OnTrigger(T)"/> function, which should accomplish whatever the effect should
    /// do when it is triggered. The subclass can specify what parameter(s) the OnTrigger function
    /// needs to take in via the class's type parameter. If multiple arguments are needed, one should create
    /// a class that subclasses <see cref="EffectArgs"/> that contains all the parameters, and the effect subclass
    /// should then take an instance of the EffectArgs subclass as the single parameter. If no arguments are needed,
    /// then one may pass null as the parameter to Trigger.
    ///
    /// The concept of a duration is also built into the interface, and is considered to be in arbitrary units.  The duration
    /// concept is designed to be used with <see cref="EffectTrigger{T}"/> instances, and has no effect when an effect is not
    /// utilized with an EffectTrigger.  The duration is interpreted as simply the number of times the effect's
    /// <see cref="Effect{T}.Trigger(T)"/>) function will be called before it will be removed from an EffectTrigger. If the effect
    /// is instantaneous, eg. it happens only when Trigger is called, on no particular event (such as a simple instant damage
    /// effect), then the duration specified in the constructor should be the static class constant
    /// <see cref="Instant"/>. If the effect is meant to have an infinite duration, or the effect wears off on some
    /// condition other than time passing, the duration may be set to <see cref="Infinite"/>, and then manipulated
    /// appropriately to 0 when the effect has expired.
    ///
    /// More explanation of Effects and EffectTriggers, and usage examples, can be found at the GoRogue documentation site
    /// <a href="https://chris3606.github.io/GoRogue/articles">here</a>.
    /// </remarks>
    /// <typeparam name="TTriggerArgs">
    /// The type of the parameter that will be specified to the <see cref="Effect{T}.Trigger(T)"/> function when called.
    /// </typeparam>
    [PublicAPI]
    public abstract class Effect<TTriggerArgs> where TTriggerArgs : EffectArgs
    {
        /// <summary>
        /// The value one should specify as the effect duration for an infinite effect, eg. an effect
        /// that will never expire or whose expiration time is arbitrary (for example, based on a condition
        /// other than the passing of time).
        /// </summary>
        public const int Infinite = -1;

        /// <summary>
        /// The value one should specify as the effect duration for an instantaneous effect, eg. an
        /// effect that only occurs when Trigger is manually called, and thus cannot be added to an 
        /// <see cref="EffectTrigger{TriggerArgs}"/>.
        /// </summary>
        public const int Instant = 0;

        /// <summary>
        /// The name of the effect.
        /// </summary>
        public string Name { get; set; }

        private int _duration;

        /// <summary>
        /// The duration of the effect.
        /// </summary>
        /// <remarks>
        /// When the duration reaches 0, the Effect will be automatically removed from an <see cref="EffectTrigger{TTriggerArgs}"/>.
        /// The duration can be changed from a subclass, which can be used in <see cref="OnTrigger(TTriggerArgs)"/> to
        /// cause an effect to be "cancelled", eg. immediately expire, or to extend/reduce its duration.
        /// </remarks>
        public int Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    if (Expired != null && _duration == 0)
                        Expired.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event that fires as soon as the effect is about to expire. Fires after the
        /// <see cref="OnTrigger(TTriggerArgs)"/> function has been called but before it is
        /// removed from any <see cref="EffectTrigger{TTriggerArgs}"/> instances.
        /// </summary>
        public event EventHandler? Expired;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name for the effect.</param>
        /// <param name="startingDuration">Starting duration for the effect.</param>
        protected Effect(string name, int startingDuration)
        {
            Name = name;
            _duration = startingDuration;
        }

        /// <summary>
        /// Should be called on instantaneous effects to trigger the effect.
        /// </summary>
        /// <remarks>
        /// Any effect that has INSTANT duration or duration 0 when this function is called
        /// will still have its <see cref="OnTrigger(TTriggerArgs)"/> function called.
        /// </remarks>
        /// <param name="args">Parameters that are passed to <see cref="OnTrigger(TTriggerArgs)"/>.
        /// Can be null.</param>
        public void Trigger(TTriggerArgs? args)
        {
            OnTrigger(args);

            if (Duration != 0)
                Duration = (Duration == Infinite) ? Infinite : Duration - 1;
        }

        /// <summary>
        /// Implement to take whatever action(s) the effect is supposed to accomplish.
        /// This function is called automatically when <see cref="Trigger"/> is called.
        /// </summary>
        /// <param name="e">Class containing all arguments <see cref="OnTrigger"/> requires to function.</param>
        protected abstract void OnTrigger(TTriggerArgs? e);

        /// <summary>
        /// Returns a string of the effect's name and duration.
        /// </summary>
        /// <returns>String representation of the effect.</returns>
        public override string ToString()
        {
            string durationStr = (Duration == Infinite) ? "Infinite" : Duration.ToString();
            return $"{Name}: {durationStr} duration remaining";
        }
    }
}
