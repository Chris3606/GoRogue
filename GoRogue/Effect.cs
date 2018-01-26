namespace GoRogue
{
    /// <summary>
    /// Default argument for any effect.  Any class that is used as the template
    /// argument for an effect must either be this or a class that inherits from this.
    /// </summary>
    /// <remarks>
    /// This allows cancellation of EffectTriggers, as detailed in that class's documentation.
    /// </remarks>
    public class EffectArgs
    {
        /// <summary>
        /// Whether or not the EffectTrigger should stop calling all subsequent effect's
        /// Trigger functions.  See that class's documentation for details.
        /// </summary>
        public bool CancelTrigger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public EffectArgs() { CancelTrigger = false; }
    }

    /// <summary>
    /// Class designed to represent any sort of in-game effect.  This could be anything from
    /// a simple physical damage effect to a heal effect, including area of effects,
    /// damage over time effects, or even potentially a special effect that simply boosts
    /// a stat.
    /// </summary>
    /// <remarks>
    /// Effectively, the class is nothing more than an interface for the concept of something
    /// that happens, potentially instantaneously or potentially one or more times on a
    /// certain event (beginning of a turn, end of a turn, on taking damage, etc).  The
    /// standard way to use the Effect class is to create a subclass
    /// of Effect, that at the very least implements the OnTrigger function, which should
    /// accomplish whatever the effect should do when it is triggered.
    ///
    /// The subclass can
    /// specify what parameter(s) it needs to take in via the class's type parameter.
    /// If multiple arguments are needed, one should create a class that subclasses EffectArgs
    /// that contains all the parameters, and the effect subclass should then take an
    /// instance of that EffectArgs subclass as the single parameter.  If no arguments
    /// are needed, then one may pass null as the parameter to Trigger.
    ///
    /// The concept of a duration is also built in to the interface (see EffectTrigger class
    /// for details on Effect durations.  The duration is to be interpreted as the number of
    /// times the effect's Trigger function will be called before it will be removed
    /// from an EffectTrigger.
    ///
    /// If the effect is
    /// instantaneous, eg. it happens only when Trigger is called, on no
    /// particular event (such as a simple instant physical damage effect), then the duration
    /// specified in the constructor should be the static class constant INSTANT.  Otherwise,
    /// one may specify the duration as a positive integer, or the INFINITE static class
    /// constant.  See EffectTrigger class documentation for details on durations.
    /// </remarks>
    /// <typeparam name="TriggerArgs">The type of the parameter that will be specified to the
    /// Trigger function when called.</typeparam>
    abstract public class Effect<TriggerArgs> where TriggerArgs : EffectArgs
    {
#pragma warning disable RECS0108

        /// <summary>
        /// The value one should specify as the effect duration for
        /// an infinite effect, eg. an effect that will never expire
        /// and be automatically removed from an EffectTrigger.
        /// </summary>
        public static readonly int INFINITE = -1;

        /// <summary>
        /// The value one should specify as the effect duaration for an
        /// instantaneous effect, eg. an effect that only occurs when Trigger
        /// is manually called, and thus cannot be added to an EffectTrigger.
        /// </summary>
        public static readonly int INSTANT = 0;

#pragma warning restore RECS0108

        /// <summary>
        /// The name of the effect
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The duration of the effect.  When the duration reaches 0, the Effect will be
        /// automatically removed from an EffectTrigger.  The duration can be changed
        /// from a subclass, which can be used in OnTrigger to cause an effect to be
        /// "cancelled", eg. immediately expire, or to extend/reduce its duration.
        /// </summary>
        public int Duration { get; protected set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name for the effect</param>
        /// <param name="startingDuration">Starting duration for the effect.</param>
        public Effect(string name, int startingDuration)
        {
            Name = name;
            Duration = startingDuration;
        }

        /// <summary>
        /// Should be called on instantaneous effects to Trigger the effect.  Can also
        /// be called manually (not by an EffectTrigger) on non-instantaneous effects,
        /// however note that it will still cause the duration to decrease.
        /// </summary>
        /// <remarks>Any effect that has INSTANT duration (eg. duration 0) will still
        /// have OnTrigger called when Trigger is called.</remarks>
        /// <param name="args">Parameters that are passed to OnTrigger.  Can be null.</param>
        public void Trigger(TriggerArgs args)
        {
            OnTrigger(args);

            if (Duration != 0)
                Duration = (Duration == INFINITE) ? INFINITE : Duration - 1;
        }

        /// <summary>
        /// Should be implemented to take whatever action(s) the effect is supposed to
        /// accomplish.  This function is called automatically when Trigger is called.
        /// </summary>
        /// <param name="e">Class containing all arguments OnTrigger requires to function.</param>
        abstract protected void OnTrigger(TriggerArgs e);

        /// <summary>
        /// Yields a string of the effect's name and duration.
        /// </summary>
        /// <returns>String representation of the effect.</returns>
        public override string ToString()
        {
            string durationStr = (Duration == INFINITE) ? "Infinite" : Duration.ToString();
            return $"{Name}: {durationStr} duration remaining";
        }
    }
}