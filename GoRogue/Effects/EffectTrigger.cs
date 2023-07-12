using JetBrains.Annotations;

namespace GoRogue.Effects
{
    /// <summary>
    /// Represents an "event" that can automatically trigger and manage one or more
    /// <see cref="Effect" /> instances, and acts as part of the implementation of
    /// duration in Effect.
    /// </summary>
    /// <remarks>
    /// EffectTrigger's primary purpose is to represent an event that can trigger one or more effects, and
    /// automatically remove those effects from the list when their duration reaches 0.  Each EffectTrigger
    /// instance can have one or more (non-instantaneous) effects added to it.
    ///
    /// Each time the <see cref="TriggerEffects()" /> function is called, every Effect has its
    /// Trigger function called (provided its duration is not 0). Each Effect may, via the cancelTrigger
    /// parameter, member, stop the event from being sent to subsequent effects in the EffectTrigger's list.
    /// Once either all effects in the list have had their Trigger function called, or some effect has cancelled the
    /// triggering, any effect whose duration has reached 0 is removed from the EffectTrigger automatically.
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
    /// For some complex game mechanics, it may be desirable to control how effects stack, the order they appear
    /// in the effects list of EffectTriggers, etc. In these cases, sub-classing EffectTrigger and overriding the
    /// add and remove functions can allow this functionality.
    ///
    /// If you need to pass a parameter with extra data to the Trigger function, you should use <see cref="AdvancedEffectTrigger{TTriggerArgs}"/>
    /// and <see cref="AdvancedEffect{TTriggerArgs}"/> instead.
    /// </remarks>
    [PublicAPI]
    public class EffectTrigger : EffectTriggerBase<Effect>
    {
        /// <summary>
        /// Calls the <see cref="Effect.Trigger(out bool)" /> function of each effect
        /// in the <see cref="Effects" /> list (as long as its duration is not 0), then
        /// removes any effect that has duration 0.
        /// </summary>
        /// <remarks>
        /// If some effect sets the boolean it receives as an "out" parameter to true, the loop will be broken and no
        /// subsequent effects in the list will have Trigger called. After either this occurs or all effects have had
        /// their Trigger function called, any effect in the list that has a duration of 0 is automatically removed from
        /// the list.
        /// </remarks>
        public void TriggerEffects()
        {
            foreach (var effect in EffectsList)
                if (effect.Duration != 0)
                {
                    effect.Trigger(out var cancelTrigger);
                    if (cancelTrigger)
                        break;
                }

            EffectsList.RemoveAll(eff => eff.Duration == 0);
        }
    }
}
