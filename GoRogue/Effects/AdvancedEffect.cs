using JetBrains.Annotations;

namespace GoRogue.Effects
{
    /// <summary>
    /// More advanced version of <see cref="Effect"/> which allows for a parameter to be passed to the
    /// Trigger method.
    /// </summary>
    /// <remarks>
    /// This effect type is useful when information about a particular trigger needs to be passed to the effect
    /// in order for it to work.  For example, an effect which reacts to damage might need to know how much damage
    /// is being dealt in order to function.
    /// </remarks>
    /// <typeparam name="TTriggerArgs">
    /// The type of the parameter that will be specified to the <see cref="AdvancedEffect{TTriggerArgs}.Trigger(TTriggerArgs)" />
    /// function (or its overloads) when called.
    /// </typeparam>
    [PublicAPI]
    public abstract class AdvancedEffect<TTriggerArgs> : EffectBase
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name for the effect.</param>
        /// <param name="startingDuration">Starting duration for the effect.</param>
        protected AdvancedEffect(string name, int startingDuration)
            : base(name, startingDuration)
        { }

        /// <summary>
        /// Triggers the effect.  If you're calling this function manually, you should use the
        /// <see cref="Trigger(TTriggerArgs)"/> function instead, unless you intend to manually support cancellation of
        /// a trigger.
        /// </summary>
        /// <remarks>
        /// Any effect that has Instant duration or duration 0 when this function is called
        /// will still have its <see cref="OnTrigger(out bool, TTriggerArgs)" /> function called.
        /// </remarks>
        /// <param name="cancelTrigger">
        /// When set to true, if the effect is being called by an EffectTrigger, the trigger will be cancelled;
        /// eg. any events which have yet to be triggered will not be triggered during the current call to
        /// <see cref="EffectTrigger.TriggerEffects"/>.  If the effect is not being called by an EffectTrigger,
        /// this parameter has no effect.
        /// </param>
        /// <param name="args">The parameter to pass to the <see cref="OnTrigger(out bool, TTriggerArgs)" /> function.</param>
        public void Trigger(out bool cancelTrigger, TTriggerArgs args)
        {
            OnTrigger(out cancelTrigger, args);

            if (Duration != 0)
                Duration = Duration == EffectDuration.Infinite ? EffectDuration.Infinite : Duration - 1;
        }

        /// <summary>
        /// Triggers the effect, ignoring any result set to the boolean value in <see cref="Trigger(out bool, TTriggerArgs)"/>.
        /// Should be called to trigger instantaneously occuring effects or effects that aren't part of an EffectTrigger
        /// and thus don't support trigger cancellation.
        /// </summary>
        /// <remarks>
        /// Any effect that has Instant duration or duration 0 when this function is called
        /// will still have its <see cref="OnTrigger(out bool, TTriggerArgs)" /> function called.
        /// </remarks>
        /// <param name="args">The parameter to pass to the <see cref="OnTrigger(out bool, TTriggerArgs)" /> function.</param>
        public void Trigger(TTriggerArgs args) => Trigger(out _, args);

        /// <summary>
        /// Implement to take whatever action(s) the effect is supposed to accomplish.
        /// This function is called automatically when <see cref="Trigger(out bool, TTriggerArgs)" /> is called.
        /// </summary>
        /// <param name="cancelTrigger">
        /// When set to true, if the effect is being called by an EffectTrigger, the trigger will be cancelled;
        /// eg. any events which have yet to be triggered will not be triggered during the current call to
        /// <see cref="EffectTrigger.TriggerEffects"/>.  If the effect is not being called by an EffectTrigger,
        /// this parameter has no effect.
        /// </param>
        /// <param name="args">Arguments passed to the Trigger function.</param>
        protected abstract void OnTrigger(out bool cancelTrigger, TTriggerArgs args);
    }
}
