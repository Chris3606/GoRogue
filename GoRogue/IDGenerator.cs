using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue
{
    /// <summary>
    /// Class designed as a helper for situations where you need to generate and assign a unique
    /// integer to each instance of a class, eg. for a class implementing <see cref="IHasID" />.
    /// </summary>
    /// <remarks>
    /// The class may be initialized with a starting unsigned integer -- if none is given, 0 is the default
    /// starting point. To assign an ID, call <see cref="UseID" />, and assign the value that it returns.
    /// This class is NOT thread-safe on its own -- if it needs to be, you can simply use a lock to wrap
    /// any calls to UseID.
    /// </remarks>
    [PublicAPI]
    [DataContract]
    public class IDGenerator : IMatchable<IDGenerator>
    {
        /// <summary>
        /// The integer ID that will be returned the next time <see cref="UseID"/> is called.
        /// </summary>
        [DataMember]
        public uint CurrentInteger { get; private set; }

        /// <summary>
        /// If true, the ID generator has assigned the last ID in the uint range, and will throw an InvalidOperationException
        /// the next time UseID is called.
        /// </summary>
        [DataMember]
        public bool LastAssigned { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="startingInt">
        /// Unsigned integer to start at (one that will be returned first time <see cref="UseID" /> is called).
        /// Defaults to 0.
        /// </param>
        /// <param name="lastAssigned">
        /// Whether or not the last possible ID has been assigned.  If this is set to true, the
        /// <paramref name="startingInt"/> value must be uint.maxValue.  Generally, you should leave this parameter alone;
        /// it is designed primarily to facilitate serialization/deserialization.
        /// </param>
        public IDGenerator(uint startingInt = 0, bool lastAssigned = false)
        {
            if (lastAssigned && startingInt != uint.MaxValue)
                throw new ArgumentException(
                    $"If {lastAssigned} is true, then {startingInt} must by definition be equal to uint.MaxValue.", nameof(lastAssigned));

            CurrentInteger = startingInt;
            LastAssigned = lastAssigned;
        }

        /// <summary>
        /// Call every time you wish to "assign" an ID. The integer returned will never be returned
        /// again (each integer returned by this function will be unique).
        /// </summary>
        /// <returns>The ID that has been assigned.</returns>
        public uint UseID()
        {
            if (LastAssigned)
                throw new InvalidOperationException($"An {nameof(IDGenerator)} ran out of IDs to assign, as uint.MaxValue was hit.");

            // We're about to assign the last ID, so ensure we don't assign it again and don't overflow.
            if (CurrentInteger == uint.MaxValue)
            {
                LastAssigned = true;
                return CurrentInteger;
            }

            return CurrentInteger++;
        }

        /// <summary>
        /// Compares the two <see cref="IDGenerator"/> objects to see if they are in an identical state.
        /// </summary>
        /// <param name="other"/>
        /// <returns>True if the IDGenerator object specified is in the same state as the current one; false otherwise.</returns>
        public bool Matches(IDGenerator? other) => !(other is null) && CurrentInteger == other.CurrentInteger && LastAssigned == other.LastAssigned;
    }
}
