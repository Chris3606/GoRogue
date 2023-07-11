using JetBrains.Annotations;

namespace GoRogue.Effects
{
    /// <summary>
    /// Static class containing special constants used for the duration of effects.
    /// </summary>
    [PublicAPI]
    public static class EffectDuration
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
        /// effect trigger.
        /// </summary>
        public const int Instant = 0;
    }
}
