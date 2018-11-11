using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GoRogue;

namespace GoRogue_PerformanceTests
{
	internal static class LayerMaskTests
	{
		private static readonly int[] POSSIBLE_LAYERS = Enumerable.Range(0, 32).ToArray();
		
		public static TimeSpan TimeForGetRandomLayers(int iterations, int numLayers)
		{
			var layersToCheck = new HashSet<int>();

			while (layersToCheck.Count != numLayers)
				layersToCheck.Add(POSSIBLE_LAYERS.RandomItem());

			return TimeForGetLayers(iterations, layersToCheck);
		}
		public static TimeSpan TimeForGetLayers(int iterations, IEnumerable<int> layersToCheck = null)
		{
			var layerMasking = new LayerMasker(POSSIBLE_LAYERS.Length);

			var stopwatch = new Stopwatch();

			int[] layers = layersToCheck == null ? new int[] { 0, 2, 5 } : layersToCheck.ToArray();

			uint mask = layerMasking.Mask(layers);

			// For caching
			int myLayer;
			foreach (var i in layerMasking.Layers(mask))
				myLayer = i;

			stopwatch.Start();
			for (int it = 0; it < iterations; it++)
				foreach (var i in layerMasking.Layers(mask))
					myLayer = i;
			stopwatch.Stop();

			return stopwatch.Elapsed;
		}
	}
}
