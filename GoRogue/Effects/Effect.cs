using JetBrains.Annotations;

namespace GoRogue.Effects
{
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
    /// <see cref="Effect.OnTrigger(out bool)" /> function, which should accomplish whatever the effect should
    /// do when it is triggered.
    ///
    /// The concept of a duration is also built into the interface, and is considered to be in arbitrary units.  The duration
    /// concept is designed to be used with <see cref="EffectTrigger" /> instances, and has no effect when an effect is not
    /// utilized with an EffectTrigger.  The duration is interpreted as simply the number of times the effect's
    /// <see cref="Effect.Trigger(out bool)" />) function will be called before it will be removed from an EffectTrigger. If the
    /// effect is instantaneous, eg. it happens only when Trigger is called, on no particular event (such as a simple instant damage
    /// effect), then the duration specified in the constructor should be the static class constant
    /// <see cref="EffectDuration.Instant" />. If the effect is meant to have an infinite duration, or the effect wears off on some
    /// condition other than time passing, the duration may be set to <see cref="EffectDuration.Infinite" />, and then manipulated
    /// appropriately to 0 when the effect has expired. More explanation of Effects and EffectTriggers, and usage examples,
    /// can be found at the GoRogue documentation site <a href="http://www.roguelib.com/articles/howtos/effects-system.html">here</a>.
    /// </remarks>
    [PublicAPI]
    public abstract class Effect : EffectBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name for the effect.</param>
        /// <param name="startingDuration">Starting duration for the effect.</param>
        protected Effect(string name, int startingDuration)
            : base(name, startingDuration)
        { }


        /// <summary>
        /// Triggers the effect.  If you're calling this function manually, you should use the
        /// <see cref="Trigger()"/> function instead, unless you intend to manually support cancellation of a trigger.
        /// </summary>
        /// <remarks>
        /// Any effect that has Instant duration or duration 0 when this function is called
        /// will still have its <see cref="OnTrigger(out bool)" /> function called.
        /// </remarks>
        /// <param name="cancelTrigger">
        /// When set to true, if the effect is being called by an EffectTrigger, the trigger will be cancelled;
        /// eg. any events which have yet to be triggered will not be triggered during the current call to
        /// <see cref="EffectTrigger.TriggerEffects"/>.  If the effect is not being called by an EffectTrigger,
        /// this parameter has no effect.
        /// </param>
        public void Trigger(out bool cancelTrigger)
        {
            OnTrigger(out cancelTrigger);

            if (Duration != 0)
                Duration = Duration == EffectDuration.Infinite ? EffectDuration.Infinite : Duration - 1;
        }

        /// <summary>
        /// Triggers the effect, ignoring any result set to the boolean value in <see cref="Trigger(out bool)"/>.
        /// Should be called to trigger instantaneously occuring effects or effects that aren't part of an EffectTrigger
        /// and thus don't support trigger cancellation.
        /// </summary>
        /// <remarks>
        /// Any effect that has Instant duration or duration 0 when this function is called
        /// will still have its <see cref="OnTrigger(out bool)" /> function called.
        /// </remarks>
        public void Trigger() => Trigger(out _);

        /// <summary>
        /// Implement to take whatever action(s) the effect is supposed to accomplish.
        /// This function is called automatically when <see cref="Trigger(out bool)" /> is called.
        /// </summary>
        /// <param name="cancelTrigger">
        /// When set to true, if the effect is being called by an EffectTrigger, the trigger will be cancelled;
        /// eg. any events which have yet to be triggered will not be triggered during the current call to
        /// <see cref="EffectTrigger.TriggerEffects"/>.  If the effect is not being called by an EffectTrigger,
        /// this parameter has no effect.
        /// </param>
        protected abstract void OnTrigger(out bool cancelTrigger);
    }
}
