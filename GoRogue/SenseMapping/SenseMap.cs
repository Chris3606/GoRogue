﻿using GoRogue.MapViews;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoRogue.SenseMapping
{
	/// <summary>
	/// Class responsible for calculating a map for senses (sound, light, etc), or generally anything
	/// that can be modeled as sources propegating through a map that has degrees of resistance to spread.
	/// </summary>
	/// <remarks>
	/// Generally, this class can be used to model the result of applying ripple-like or shadowcasting-like
	/// "spreading" of values from one or more sources through a map.  This can include modeling the spreading
	/// of light, sound, heat for a heatmap, etc. through a map.  You create one or more <see cref="SenseSource"/>
	/// instances representing your various sources, add them to the SenseMap, and call <see cref="Calculate"/>
	/// when you wish to re-calculate the SenseMap.
	/// 
	/// Like most GoRogue algorithm implementations, SenseMap takes as a construction parameter an IMapView that represents
	/// the map.  Specifically, it takes an <see cref="IMapView{Double}"/>, where the double value at each location
	/// represents the "resistance" that location has to the passing of source values through it.  The values must be >= 0.0,
	/// where 0.0 means that a location has no resistance to spreading of source values, and greater values represent greater
	/// resistance.  The scale of this resistance is arbitrary, and is related to the <see cref="SenseSource.Intensity"/> of
	/// your sources.  As a source spreads through a given location, a value equal to the resistance value of that location
	/// is subtracted from the source's value (plus the normal fallof for distance).
	/// 
	/// The map can be calculated by calling the <see cref="Calculate"/> function.
	/// 
	/// This class exposes the resulting sensory values values to you via indexers -- SenseMap implements
	/// <see cref="IMapView{Double}"/>, where 0.0 indicates no sources were able to spread to the given location (eg, either it was
	/// stopped or fell off due to distance), and a value greater than 0.0 indicates the combined intensity of any sources
	/// that reached the given location.
	/// </remarks>
	public class SenseMap : IReadOnlySenseMap, IEnumerable<double>, IMapView<double>
	{
		private List<SenseSource> _senseSources;

		private HashSet<Coord> currentSenseMap;

		private int lastHeight;

		private int lastWidth;

		private HashSet<Coord> previousSenseMap;

		private IMapView<double> resMap;

		// Making these 1D didn't really affect performance that much, though may be worth it on
		// large maps
		private double[,] senseMap;

		/// <summary>
		/// Constructor. Takes the resistance map to use for calculations.
		/// </summary>
		/// <param name="resMap">The resistance map to use for calculations.</param>
		public SenseMap(IMapView<double> resMap)
		{
			this.resMap = resMap;
			senseMap = new double[resMap.Width, resMap.Height];
			lastWidth = resMap.Width;
			lastHeight = resMap.Height;

			_senseSources = new List<SenseSource>();

			previousSenseMap = new HashSet<Coord>();
			currentSenseMap = new HashSet<Coord>();
		}

		/// <summary>
		/// IEnumerable of only positions currently "in" the SenseMap, eg. all positions that have a
		/// value other than 0.0.
		/// </summary>
		public IEnumerable<Coord> CurrentSenseMap => currentSenseMap;

		/// <summary>
		/// Height of sense map.
		/// </summary>
		public int Height => resMap.Height;

		/// <summary>
		/// IEnumerable of positions that DO have a non-zero value in the sense map as of the most
		/// current <see cref="Calculate"/> call, but DID NOT have a non-zero value after the previous time
		/// <see cref="Calculate"/> was called.
		/// </summary>
		public IEnumerable<Coord> NewlyInSenseMap => currentSenseMap.Where(pos => !previousSenseMap.Contains(pos));

		/// <summary>
		/// IEnumerable of positions that DO NOT have a non-zero value in the sense map as of the
		/// most current <see cref="Calculate"/> call, but DID have a non-zero value after the previous time
		/// <see cref="Calculate"/> was called.
		/// </summary>
		public IEnumerable<Coord> NewlyOutOfSenseMap => previousSenseMap.Where(pos => !currentSenseMap.Contains(pos));

		/// <summary>
		/// Read-only list of all sources currently considered part of the SenseMap. Some may have their
		/// <see cref="SenseSource.Enabled"/> flag set to false, so all of these may or may not be counted
		/// when <see cref="Calculate"/> is called.
		/// </summary>
		public IReadOnlyList<SenseSource> SenseSources => _senseSources.AsReadOnly();

		/// <summary>
		/// Width of the sense map.
		/// </summary>
		public int Width => resMap.Width;

		/// <summary>
		/// Returns the "sensory value" for the given position.
		/// </summary>
		/// <param name="index1D">Position to return the sensory value for, as a 1d-index-style value.</param>
		/// <returns>The sense-map value for the given position.</returns>
		public double this[int index1D] => senseMap[Coord.ToXValue(index1D, Width), Coord.ToYValue(index1D, Width)];

		/// <summary>
		/// Returns the "sensory value" for the given position.
		/// </summary>
		/// <param name="pos">The position to return the sensory value for.</param>
		/// <returns>The sensory value for the given position.</returns>
		public double this[Coord pos] => senseMap[pos.X, pos.Y];

		/// <summary>
		/// Returns the "sensory value" for the given position.
		/// </summary>
		/// <param name="x">X-coordinate of the position to return the sensory value for.</param>
		/// <param name="y">Y-coordinate of the position to return the sensory value for.</param>
		/// <returns>The sensory value for the given position.</returns>
		public double this[int x, int y] => senseMap[x, y];

		/// <summary>
		/// Adds the given source to the list of sources. If the source has its
		/// <see cref="SenseSource.Enabled"/> flag set when <see cref="Calculate"/> is next called, then
		/// it will be counted as a source.
		/// </summary>
		/// <param name="senseSource">The source to add.</param>
		public void AddSenseSource(SenseSource senseSource)
		{
			_senseSources.Add(senseSource);
			senseSource.resMap = resMap;
		}

		/// <summary>
		/// Returns a read-only representation of the SenseMap.
		/// </summary>
		/// <returns>This SenseMap object as <see cref="IReadOnlySenseMap"/>.</returns>
		public IReadOnlySenseMap AsReadOnly() => this;

		/// <summary>
		/// Calcuates the map.  For each enabled source in the source list, it calculates
		/// the source's spreading, and puts them all together in the sense map's output.
		/// </summary>
		public void Calculate()
		{
			if (lastWidth != resMap.Width || lastHeight != resMap.Height)
			{
				senseMap = new double[resMap.Width, resMap.Height];
				lastWidth = resMap.Width;
				lastHeight = resMap.Height;
			}
			else
				Array.Clear(senseMap, 0, senseMap.Length);

			previousSenseMap = currentSenseMap;
			currentSenseMap = new HashSet<Coord>();

			if (_senseSources.Count > 1) // Probably not the proper condition, but useful for now.
			{
				Parallel.ForEach(_senseSources, senseSource =>
				{
					senseSource.calculateLight();
				});
			}
			else
				foreach (var senseSource in _senseSources)
					senseSource.calculateLight();

			// Flush sources to actual senseMap
			foreach (var senseSource in _senseSources)
				blitSenseSource(senseSource, senseMap, currentSenseMap, resMap);
		}

		/// <summary>
		/// Enumerator, in case you want to use this as a list of doubles.
		/// </summary>
		/// <returns>Enumerable of doubles (the sensory values).</returns>
		public IEnumerator<double> GetEnumerator()
		{
			for (int y = 0; y < resMap.Height; y++)
				for (int x = 0; x < resMap.Width; x++)
					yield return senseMap[x, y];
		}

		// Warning about hidden overload intentionally disabled -- the two methods are equivalent but
		// the ToString method that takes 0, as opposed to all optional, parameters is necessary to
		// override the one from base class object. That one calls this one so the "hidden" overload
		// is of no harm.
#pragma warning disable RECS0137

		/// <summary>
		/// ToString that customizes the characters used to represent the map.
		/// </summary>
		/// <param name="normal">The character used for any location not in the SenseMap.</param>
		/// <param name="center">
		/// The character used for any location that is the center-point of a source.
		/// </param>
		/// <param name="sourceValue">
		/// The character used for any location that is in range of a source, but not a center point.
		/// </param>
		/// <returns>The string representation of the SenseMap, using the specified characters.</returns>
		public string ToString(char normal = '-', char center = 'C', char sourceValue = 'S')
#pragma warning restore RECS0137

		{
			string result = "";

			for (int y = 0; y < resMap.Height; y++)
			{
				for (int x = 0; x < resMap.Width; x++)
				{
					if (senseMap[x, y] > 0.0)
						result += (isACenter(x, y)) ? center : sourceValue;
					else
						result += normal;

					result += " ";
				}

				result += '\n';
			}

			return result;
		}

		/// <summary>
		/// Returns a string representation of the map, where any location not in the SenseMap is
		/// represented by a '-' character, any position that is the center of some source is
		/// represented by a 'C' character, and any position that has a non-zero value but is not a
		/// center is represented by an 'S'.
		/// </summary>
		/// <returns>A (multi-line) string representation of the SenseMap.</returns>
		public override string ToString() => ToString();

		/// <summary>
		/// Returns a string representation of the map, with the actual values in the SenseMap,
		/// rounded to the given number of decimal places.
		/// </summary>
		/// <param name="decimalPlaces">The number of decimal places to round to.</param>
		/// <returns>
		/// A string representation of the map, rounded to the given number of decimal places.
		/// </returns>
		public string ToString(int decimalPlaces)
		=> senseMap.ExtendToStringGrid(elementStringifier: (double obj) => obj.ToString("0." + "0".Multiply(decimalPlaces)));

		/// <summary>
		/// Generic enumerator.
		/// </summary>
		/// <returns>Enumerator for looping.</returns>
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		/// <summary>
		/// Removes the given source from the list of sources. Genearlly, use this if a source is permanently removed
		/// from a map. For temporary disabling, you should generally use the <see cref="SenseSource.Enabled"/> flag.
		/// </summary>
		/// <remarks>
		/// The source values that this sense source was responsible for are NOT removed from the sensory output values
		/// until <see cref="Calculate"/> is next called.
		/// </remarks>
		/// <param name="senseSource">The source to remove.</param>
		public void RemoveSenseSource(SenseSource senseSource)
		{
			_senseSources.Remove(senseSource);
			senseSource.resMap = null;
		}

		private bool isACenter(int x, int y)
		{
			foreach (var source in _senseSources)
				if (source.Position.X == x && source.Position.Y == y)
					return true;

			return false;
		}

		// Blits given source's lightMap onto the global lightmap given
		private static void blitSenseSource(SenseSource source, double[,] destination, HashSet<Coord> sourceMap, IMapView<double> resMap)
		{
			// Calculate actual radius bounds, given constraint based on location
			int minX = Math.Min((int)source.Radius, source.Position.X);
			int minY = Math.Min((int)source.Radius, source.Position.Y);
			int maxX = Math.Min((int)source.Radius, resMap.Width - 1 - source.Position.X);
			int maxY = Math.Min((int)source.Radius, resMap.Height - 1 - source.Position.Y);

			// Use radius bounds to extrapalate global coordinate scheme mins and maxes
			Coord gMin = source.Position - new Coord(minX, minY);
			//Coord gMax = source.Position + Coord.Get(maxX, maxY);

			// Use radius bound to extrapalate light-local coordinate scheme min and max bounds that
			// are actually blitted
			Coord lMin = new Coord((int)source.Radius - minX, (int)source.Radius - minY);
			Coord lMax = new Coord((int)source.Radius + maxX, (int)source.Radius + maxY);

			for (int xOffset = 0; xOffset <= lMax.X - lMin.X; xOffset++)
			//Parallel.For(0, lMax.X - lMin.X + 1, xOffset => // By light radius 30 or so, there is enough work to get benefit here.  Manual thread splitting may also be an option.
			{
				for (int yOffset = 0; yOffset <= lMax.Y - lMin.Y; yOffset++)
				{
					// Offset local/current by proper amount, and update lightmap
					Coord c = new Coord(xOffset, yOffset);
					Coord gCur = gMin + c;
					Coord lCur = lMin + c;

					destination[gCur.X, gCur.Y] = destination[gCur.X, gCur.Y] + source.light[lCur.X, lCur.Y]; // Add source values,
					if (destination[gCur.X, gCur.Y] > 0.0)
						sourceMap.Add(gCur);
				}
			} //);
		}
	}
}