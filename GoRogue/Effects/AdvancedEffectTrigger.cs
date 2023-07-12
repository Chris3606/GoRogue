using JetBrains.Annotations;

namespace GoRogue.Effects
{
    /// <summary>
    /// More advanced version of <see cref="EffectTrigger"/> which allows for a parameter to be passed to the
    /// TriggerEffects method.
    /// </summary>
    /// <remarks>
    /// This effect trigger type is useful when information about a particular trigger needs to be passed to the effects
    /// in order for them to work.  For example, effects which react to damage might need to know how much damage
    /// is being dealt in order to function.
    /// </remarks>
    /// <typeparam name="TTriggerArgs">
    /// The type of the parameter that will be specified to the <see cref="AdvancedEffect{TTriggerArgs}.Trigger(out bool, TTriggerArgs)" />
    /// function when called.
    /// </typeparam>
    [PublicAPI]
    public class AdvancedEffectTrigger<TTriggerArgs> : EffectTriggerBase<AdvancedEffect<TTriggerArgs>>
    {
        /// <summary>
        /// Calls the <see cref="AdvancedEffect{TTriggerArgs}.Trigger(out bool, TTriggerArgs)" /> function of each effect
        /// in the <see cref="Effects" /> list (as long as its duration is not 0), then
        /// removes any effect that has duration 0.
        /// </summary>
        /// <remarks>
        /// If some effect sets the boolean it receives as an "out" parameter to true, the loop will be broken and no
        /// subsequent effects in the list will have Trigger called. After either this occurs or all effects have had
        /// their Trigger function called, any effect in the list that has a duration of 0 is automatically removed from
        /// the list.
        /// </remarks>
        /// <param name="args">Arguments to pass to the Trigger function of each effect that is triggered.</param>
        public void TriggerEffects(TTriggerArgs args)
        {
            foreach (var effect in EffectsList)
                if (effect.Duration != 0)
                {
                    effect.Trigger(out var cancelTrigger, args);
                    if (cancelTrigger)
                        break;
                }

            EffectsList.RemoveAll(eff => eff.Duration == 0);
        }
    }
}
