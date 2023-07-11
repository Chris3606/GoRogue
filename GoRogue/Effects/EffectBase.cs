using System;
using JetBrains.Annotations;

namespace GoRogue.Effects
{
    /// <summary>
    /// Base class for <see cref="Effect"/> and <see cref="AdvancedEffect{TTriggerArgs}"/>.  Typically not useful
    /// unless you're creating a a custom implementation of effects and/or triggers.
    /// </summary>
    [PublicAPI]
    public abstract class EffectBase
    {
        private int _duration;
        /// <summary>
        /// The duration of the effect.
        /// </summary>
        /// <remarks>
        /// When the duration reaches 0, the effect will be automatically removed from an
        /// <see cref="EffectTrigger" />.
        /// The duration can be changed from a subclass, which can be used in OnTrigger to
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
        /// The name of the effect.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Event that fires as soon as the effect is about to expire. Fires after the
        /// OnTrigger function has been called but before it is
        /// removed from any <see cref="EffectTrigger" /> instances.
        /// </summary>
        public event EventHandler? Expired;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name for the effect.</param>
        /// <param name="startingDuration">Starting duration for the effect.</param>
        protected EffectBase(string name, int startingDuration)
        {
            Name = name;
            _duration = startingDuration;
        }

        /// <summary>
        /// Returns a string of the effect's name and duration.
        /// </summary>
        /// <returns>String representation of the effect.</returns>
        public override string ToString()
        {
            string durationStr = Duration == EffectDuration.Infinite ? "Infinite" : Duration.ToString();
            return $"{Name}: {durationStr} duration remaining";
        }
    }
}
