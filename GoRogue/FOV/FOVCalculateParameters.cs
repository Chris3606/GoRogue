using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using SadRogue.Primitives;

namespace GoRogue.FOV
{
    /// <summary>
    /// Represents a set of parameters that were passed to a call to Calculate.
    /// </summary>
    [PublicAPI]
    public readonly struct FOVCalculateParameters : IEquatable<FOVCalculateParameters>, IMatchable<FOVCalculateParameters>
    {
        /// <summary>
        /// Position of the FOV origin point.
        /// </summary>
        [DataMember] public readonly Point Origin;

        /// <summary>
        /// The maximum radius -- eg. the maximum distance of the field of view if completely unobstructed.
        /// </summary>
        [DataMember] public readonly double Radius;

        /// <summary>
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </summary>
        [DataMember] public readonly Distance DistanceCalc;

        /// <summary>
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points upward, and increases move clockwise (like a compass)
        /// </summary>
        [DataMember] public readonly double Angle;

        /// <summary>
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <see cref="Span"/> / 2 degrees are included on either side of the span line.
        /// </summary>
        [DataMember] public readonly double Span;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="origin">Position of the FOV origin point.</param>
        /// <param name="radius">The maximum radius -- eg. the maximum distance of the field of view if completely unobstructed.</param>
        /// <param name="distanceCalc">
        /// The distance calculation used to determine what shape the radius has (or a type
        /// implicitly convertible to <see cref="SadRogue.Primitives.Distance" />, eg. <see cref="SadRogue.Primitives.Radius" />).
        /// </param>
        /// <param name="angle">
        /// The angle in degrees that specifies the outermost center point of the field of view cone. 0 degrees
        /// points up, and increases move the cone clockwise (like a compass).
        /// </param>
        /// <param name="span">
        /// The angle, in degrees, that specifies the full arc contained in the field of view cone --
        /// <paramref name="span"/>> / 2 degrees are included on either side of the span line.
        /// </param>
        public FOVCalculateParameters(Point origin, double radius, Distance distanceCalc, double angle = 0.0, double span = 360.0)
        {
            Origin = origin;
            Radius = radius;
            DistanceCalc = distanceCalc;
            Angle = angle;
            Span = span;
        }

        #region Tuple Compatibility

        /// <summary>
        /// Supports C# Deconstruction syntax.
        /// </summary>
        /// <param name="origin"/>
        /// <param name="radius"/>
        /// <param name="distanceCalc"/>
        /// <param name="angle"/>
        /// <param name="span"/>
        public void Deconstruct(out Point origin, out double radius, out Distance distanceCalc, out double angle, out double span)
        {
            origin = Origin;
            radius = Radius;
            distanceCalc = DistanceCalc;
            angle = Angle;
            span = Span;
        }

        /// <summary>
        /// Implicitly converts an FOVCalculateParameters object to an equivalent tuple.
        /// </summary>
        /// <param name="pair"/>
        /// <returns/>
        public static implicit operator (Point origin, double radius, Distance distanceCalc, double angle, double span)(FOVCalculateParameters pair)
            => pair.ToTuple();

        /// <summary>
        /// Implicitly converts a tuple to its equivalent FOVCalculateParameters.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        public static implicit operator FOVCalculateParameters((Point origin, double radius, Distance distanceCalc, double angle, double span) tuple)
            => FromTuple(tuple);

        /// <summary>
        /// Converts this FOVCalculateParameters object to an equivalent tuple.
        /// </summary>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public (Point origin, double radius, Distance distanceCalc, double angle, double span) ToTuple()
            => (Origin, Radius, DistanceCalc, Angle, Span);

        /// <summary>
        /// Converts the tuple to an equivalent FOVCalculateParameters object.
        /// </summary>
        /// <param name="tuple"/>
        /// <returns/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static FOVCalculateParameters FromTuple((Point origin, double radius, Distance distanceCalc, double angle, double span) tuple)
            => new FOVCalculateParameters(tuple.origin, tuple.radius, tuple.distanceCalc, tuple.angle, tuple.span);
        #endregion

        #region EqualityComparison

        /// <summary>
        /// True if the given objects have the same parameter values; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        public bool Equals(FOVCalculateParameters other)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return Origin == other.Origin && Radius == other.Radius && DistanceCalc == other.DistanceCalc && Angle == other.Angle && Span == other.Span;
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }


        /// <summary>
        /// True if the given object has the same parameter values; false otherwise.
        /// </summary>
        /// <param name="other"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(FOVCalculateParameters other) => Equals(other);

        /// <summary>
        /// True if the given object is a FOVCalculateParameters and has the same parameter values; false otherwise.
        /// </summary>
        /// <param name="obj"/>
        /// <returns/>
        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object? obj) => obj is FOVCalculateParameters pair && Equals(pair);

        /// <summary>
        /// Returns a hash code based on all of the object's fields.
        /// </summary>
        /// <returns/>
        public override int GetHashCode() => HashCode.Combine(Origin, Radius, DistanceCalc, Angle, Span);

        /// <summary>
        /// True if the given objects have the same parameter values; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator ==(FOVCalculateParameters left, FOVCalculateParameters right) => left.Equals(right);

        /// <summary>
        /// True if the given objects have different parameter values; false otherwise.
        /// </summary>
        /// <param name="left"/>
        /// <param name="right"/>
        /// <returns/>
        public static bool operator !=(FOVCalculateParameters left, FOVCalculateParameters right) => !(left == right);
        #endregion
    }
}
