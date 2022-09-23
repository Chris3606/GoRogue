using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoRogue.SenseMapping.Sources;
using JetBrains.Annotations;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.SenseMapping
{
    /// <summary>
    /// Class responsible for calculating a map for senses (sound, light, etc), or generally anything
    /// that can be modeled as sources propagating through a map that has degrees of resistance to spread.
    /// </summary>
    /// <remarks>
    /// Generally, this class can be used to model the result of applying ripple-like or shadow-casting like
    /// "spreading" of values from one or more sources through a map.  This can include modeling the spreading
    /// of light, sound, heat for a heat-map, etc. through a map.  You create one or more <see cref="ISenseSource" />
    /// instances representing your various sources, add them to the SenseMap, and call <see cref="ISenseMap.Calculate" />
    /// when you wish to re-calculate the SenseMap.
    /// Like most GoRogue algorithm implementations, SenseMap takes as a construction parameter an IGridView that represents
    /// the map.  Specifically, it takes an <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />, where the double value at each location
    /// represents the "resistance" that location has to the passing of source values through it.  The values must be >= 0.0,
    /// where 0.0 means that a location has no resistance to spreading of source values, and greater values represent greater
    /// resistance.  The scale of this resistance is arbitrary, and is related to the <see cref="ISenseSource.Intensity" /> of
    /// your sources.  As a source spreads through a given location, a value equal to the resistance value of that location
    /// is subtracted from the source's value (plus the normal fall-of for distance).
    /// The map can be calculated by calling the <see cref="ISenseMap.Calculate" /> function.
    /// This class exposes the resulting sensory values values to you via indexers -- SenseMap implements
    /// <see cref="SadRogue.Primitives.GridViews.IGridView{T}" />, where 0.0 indicates no sources were able to spread to the given location (eg, either
    /// it was
    /// stopped or fell off due to distance), and a value greater than 0.0 indicates the combined intensity of any sources
    /// that reached the given location.
    /// </remarks>

    /// <summary>
    /// Implementation of <see cref="ISenseMap"/> that implements the required fields/methods in a way applicable to many typical use cases.
    /// </summary>
    /// <remarks>
    /// This implementation of <see cref="ISenseMap"/> implements the enumerables by using a pair of hash maps to keep track of the positions
    /// which are non-0 in the current (and previous) calculate calls.  This provides relatively efficient implementations that should be applicable
    /// to a variety of use cases.
    ///
    /// The calculation, by default, is performed by first calling the <see cref="ISenseSource.CalculateLight"/> function of all sources.  This is performed
    /// in parallel via a Parallel.ForEach loop, if there is more than one and the <see cref="ParallelCalculate"/> property is set to true. Generally, even at
    /// 2 sense sources there is notable benefit to parallelizing the calculation; however feel free to use this flag to tweak this to your use case.
    ///
    /// After all calculations are complete, the <see cref="OnCalculate"/> implementation then takes the result view of each source and copies it to the appropriate
    /// section of the <see cref="IReadOnlySenseMap.ResultView"/> property.  This is done sequentially, in order to avoid any problems with overlapping sources.
    /// Values are aggregated by simply adding the current value and the new value together.
    ///
    /// If you want to customize the way values are aggregated together, you may customize the <see cref="ApplySenseSourceToResult"/> function.  This function is
    /// used to apply the result view of the specified sense source onto the result view.  If you simply want to change the aggregation method, then you can copy-paste
    /// the function and change the line that performs the aggregation; the aggregation method itself is not provided as a separate function for performance reasons.
    /// You may also override this function to customize the order/method of performing the aggregation.
    ///
    /// Most other customization would require overriding the <see cref="OnCalculate"/> function.
    ///
    /// You may also simply create your own implementation of <see cref="ISenseSource"/>, either by directly implementing that interface or inheriting from
    /// <see cref="SenseMapBase"/>.  This may be the best option if, for example, you want to avoid the use of a hash set in the enumerable implementation. 
    /// </remarks>
    [PublicAPI]
    public class SenseMap : SenseMapBase
    {
        /// <summary>
        /// A hash set which contains the positions which have non-0 values in the most current calculation result.
        /// </summary>
        /// <remarks>
        /// This hash set is the backing structure for <see cref="NewlyInSenseMap"/> and <see cref="NewlyOutOfSenseMap"/>,
        /// as well as <see cref="CurrentSenseMap"/>. During <see cref="ISenseMap.Calculate"/>, this value is cleared before
        /// the new calculations are performed.
        ///
        /// Typically you will only need to interact with this if you are overriding <see cref="ISenseMap.Calculate"/>; in this case, if
        /// you do not call this class's implementation, you will need to perform this clearing yourself.
        ///
        /// In order to preserve the use of whatever hasher was passed to the class at startup, it is recommended that you do _not_
        /// re-allocate this structure entirely.  See <see cref="OnCalculate"/> for a way to manage both this and <see cref="_previousSenseMap"/>
        /// that does not involve re-allocating.
        /// </remarks>
        protected HashSet<Point> CurrentSenseMapBacking;
        /// <inheritdoc />
        public override IEnumerable<Point> CurrentSenseMap => CurrentSenseMapBacking;

        /// <summary>
        /// A hash set which contains the positions which had non-0 values in the previous calculation result.
        /// </summary>
        /// <remarks>
        /// This hash set is the backing structure for <see cref="NewlyInSenseMap"/> and <see cref="NewlyOutOfSenseMap"/>.
        /// 
        /// Typically you will only need to interact with this if you are overriding <see cref="ISenseMap.Calculate"/>; in this case, if
        /// you do not call this class's implementation, you will need to ensure this is set as appropriate before the new calcluation is performed.
        ///
        /// In order to preserve the use of whatever hasher was passed to the class at startup, it is recommended that you do _not_
        /// re-allocate this structure entirely.  See <see cref="OnCalculate"/> for a way to manage both this and <see cref="CurrentSenseMapBacking"/>
        /// that does not involve re-allocating.
        /// </remarks>
        protected HashSet<Point> PreviousSenseMapBacking;

        /// <summary>
        /// Whether or not to calculate each sense source's spread algorithm in parallel.  Has no effect if there is only one source added.
        /// </summary>
        /// <remarks>
        /// When this is set to true, calling of <see cref="ISenseSource.CalculateLight"/> will happen in parallel via multiple threads.  A
        /// Parallel.ForEach will be used, which will enable the use of a thread pool.
        ///
        /// In either case, the default implementation of sense sources always have their own result views on which they perform their calculations,
        /// so there is no concern with overlapping sources.  This does NOT affect the copying of sense source's values from its local result view
        /// to the sense map's one, which in the default implementation is always sequential.
        /// </remarks>
        public bool ParallelCalculate { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="resistanceView">The resistance view to use for calculations.</param>
        /// <param name="resultViewAndResizer">
        /// The view in which the sense map calculation results are stored, along with a method to use to resize it as needed.
        ///
        /// If unspecified or null, an ArrayView will be used for the result view, and the resize function will allocate a new
        /// ArrayView of the appropriate size as needed.  This should be sufficient for most use cases.
        ///
        /// The resizer function must return a view with all of its values set to 0.0, which has the given width and height.
        /// </param>
        /// <param name="parallelCalculate">Whether or not to calculate the sense sources in parallel using Parallel.ForEach.  Has no effect if there is only one source added.</param>
        /// <param name="hasher">The hashing algorithm to use for points in hash sets.  Defaults to the default hash algorithm for Points.</param>
        public SenseMap(IGridView<double> resistanceView, CustomResultViewWithResize? resultViewAndResizer = null,
            bool parallelCalculate = true, IEqualityComparer<Point>? hasher = null)
            : base(resistanceView, resultViewAndResizer)
        {
            ParallelCalculate = parallelCalculate;
            hasher ??= EqualityComparer<Point>.Default;

            PreviousSenseMapBacking = new HashSet<Point>(hasher);
            CurrentSenseMapBacking = new HashSet<Point>(hasher);
        }

        /// <inheritdoc />
        public override IEnumerable<Point> NewlyInSenseMap => CurrentSenseMapBacking.Where(pos => !PreviousSenseMapBacking.Contains(pos));

        /// <inheritdoc />
        public override IEnumerable<Point> NewlyOutOfSenseMap => PreviousSenseMapBacking.Where(pos => !CurrentSenseMapBacking.Contains(pos));

        /// <inheritdoc />
        public override void Reset()
        {
            base.Reset();

            // Cycle current and previous hash sets to avoid re-allocation of internal buffers
            (PreviousSenseMapBacking, CurrentSenseMapBacking) = (CurrentSenseMapBacking, PreviousSenseMapBacking);
            CurrentSenseMapBacking.Clear();
        }

        /// <inheritdoc />
        protected override void OnCalculate()
        {
            // Anything past 1 sense source seems to benefit notably from parallel execution
            if (SenseSources.Count > 1 && ParallelCalculate)
                Parallel.ForEach(SenseSources, senseSource => { senseSource.CalculateLight(); });
            else
                foreach (var senseSource in SenseSources)
                    senseSource.CalculateLight();

            // Flush sources to actual senseMap
            foreach (var senseSource in SenseSources)
                ApplySenseSourceToResult(senseSource);
        }

        /// <summary>
        /// Takes the given source and applies its values to the appropriate sub-area of <see cref="SenseMapBase.ResultViewBacking"/>.  Adds any locations that
        /// end up with non-0 values to the <see cref="CurrentSenseMapBacking"/> hash set.
        /// </summary>
        /// <remarks>
        /// Override this if you need to control the aggregation function (eg. do something other than add values together), or if you want to apply results
        /// of sense source calculations to the sense map in a different way.
        /// </remarks>
        /// <param name="source">The source to apply.</param>
        protected virtual void ApplySenseSourceToResult(ISenseSource source)
        {
            // Calculate actual radius bounds, given constraint based on location
            var minX = Math.Min((int)source.Radius, source.Position.X);
            var minY = Math.Min((int)source.Radius, source.Position.Y);
            var maxX = Math.Min((int)source.Radius, ResistanceView.Width - 1 - source.Position.X);
            var maxY = Math.Min((int)source.Radius, ResistanceView.Height - 1 - source.Position.Y);

            // Use radius bounds to extrapolate global coordinate scheme mins
            var gMin = source.Position - new Point(minX, minY);

            // Use radius bound to extrapolate light-local coordinate scheme min and max bounds that
            // are actually blitted
            var lMin = new Point((int)source.Radius - minX, (int)source.Radius - minY);
            var lMax = new Point((int)source.Radius + maxX, (int)source.Radius + maxY);

            for (var xOffset = 0; xOffset <= lMax.X - lMin.X; xOffset++)
                for (var yOffset = 0; yOffset <= lMax.Y - lMin.Y; yOffset++)
                {
                    // Offset local/current by proper amount, and update light-map
                    var c = new Point(xOffset, yOffset);
                    var gCur = gMin + c;
                    var lCur = lMin + c;

                    // Null-forgiving because ResistanceView is set when sources are added, so this can't occur unless somebody has been
                    // messing with values they're not supposed to, and adding a check would cost performance.
                    ResultViewBacking[gCur.X, gCur.Y] += source.ResultView[lCur.X, lCur.Y];
                    if (ResultViewBacking[gCur.X, gCur.Y] > 0.0)
                        CurrentSenseMapBacking.Add(gCur);
                }
        }
    }
}
