using GoRogue;
using GoRogue.MapViews;
using System;
using System.Collections.Generic;
using System.IO;
using CA = EMK.Cartography;

namespace GoRogue_UnitTests
{
	// Adds functions commonly used in tests
	public static class Utility
	{
		public static void PrintHightlightedPoints(IMapView<bool> map, IEnumerable<Point> points, char wall = '#', char floor = '.', char path = '*')
		{
			char[,] array = new char[map.Width, map.Height];
			for (int y = 0; y < map.Height; y++)
				for (int x = 0; x < map.Width; x++)
					array[x, y] = (map[x, y]) ? floor : wall;

			foreach (var point in points)
				array[point.X, point.Y] = path;

			for (int y = 0; y < map.Height; y++)
			{
				for (int x = 0; x < map.Width; x++)
					Console.Write(array[x, y]);
				Console.WriteLine();
			}
		}

		public static void ReadMap(string filePath, ISettableMapView<bool> map, char wallChar = '#')
		{
			using (var reader = new StreamReader(filePath))
			{
				for (int row = 0; row < map.Height; row++)
				{
					string line = reader.ReadLine();

					for (int col = 0; col < map.Width; col++)
						map[col, row] = (line[col] == wallChar) ? false : true;
				}
			}
		}

		public static Tuple<Point, Point> ReadStartEnd(string filePath, char startChar = 's', char endChar = 'e')
		{
			Point start = Point.NONE;
			Point end = Point.NONE;

			using (var reader = new StreamReader(filePath))
			{
				int row = 0;
				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();

					for (int col = 0; col < line.Length; col++)
					{
						if (line[col] == startChar)
							start = (col, row);

						if (line[col] == endChar)
							end = (col, row);
					}
					row++;
				}
			}

			return new Tuple<Point, Point>(start, end);
		}

		public static IEnumerable<Point> ToPoints(IEnumerable<CA.Node> points)
		{
			foreach (var p in points)
				yield return ((int)p.X, (int)p.Y);
		}
	}

	internal class MyIDImpl : IHasID
	{
		private static IDGenerator idGen = new IDGenerator();

		public MyIDImpl(int myInt)
		{
			ID = idGen.UseID();
			MyInt = myInt;
		}

		public uint ID { get; private set; }
		public int MyInt { get; private set; }

		public override string ToString() => "Thing " + MyInt;
	}

	internal class ResMap : IMapView<double>
	{
		private IMapView<bool> view;

		public ResMap(IMapView<bool> view)
		{
			this.view = view;
		}

		public int Height => view.Height;
		public int Width => view.Width;
		public double this[Point pos] => (view[pos]) ? 0.0 : 1.0;
		public double this[int x, int y] => (view[x, y]) ? 0.0 : 1.0;
		public double this[int index1D] => (view[index1D]) ? 0.0 : 1.0;
	}
}
