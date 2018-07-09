using GoRogue.MapViews;

namespace GoRogue.Pathing
{
    /// @cond PRIVATE
    // Test class only - likely to change in the future.
    public class DijkstraMap
    {
        private double[,] dijkstraMap;
        private IMapView<bool> walkabilityMap;

        private static readonly double MAX_VAL = 2000;


        public DijkstraMap(IMapView<bool> walkabilityMap)
        {
            this.walkabilityMap = walkabilityMap;
            dijkstraMap = new double[walkabilityMap.Width, walkabilityMap.Height];

            for (int x = 0; x < walkabilityMap.Width; x++)
                for (int y = 0; y < walkabilityMap.Height; y++)
                    dijkstraMap[x, y] = double.MaxValue - 1;
        }

        public double this[int x, int y]
        {
            get => dijkstraMap[x, y];
        }

        public void AddGoal(int x, int y)
        {
            dijkstraMap[x, y] = 0;
        }

        public void Calculate()
        {
            for (int x = 0; x < walkabilityMap.Width; x++)
                for (int y = 0; y < walkabilityMap.Height; y++)
                    if (dijkstraMap[x, y] != 0)
                        dijkstraMap[x, y] = double.MaxValue - 1;

            int changes = 1;

            int numTimes = 0;
            while (changes > 0)
            {
                changes = 0;

                if (numTimes % 2 == 0)
                {
                    for (int x = 0; x < walkabilityMap.Width; x++)
                        for (int y = 0; y < walkabilityMap.Height; y++)
                            if (mapChecks(x, y))
                                changes++;
                }
                else
                {
                    for (int x = walkabilityMap.Width - 1; x >= 0; x--)
                        for (int y = walkabilityMap.Height - 1; y >= 0; y--)
                            if (mapChecks(x, y))
                                changes++;
                }
                numTimes++;
            }
        }

        public void RemoveGoal(int x, int y)
        {
            dijkstraMap[x, y] = double.MaxValue - 1;
        }

        public override string ToString()
        {
            return dijkstraMap.ExtendToStringGrid(4, elementStringifier: num => (num == double.MaxValue - 1) ? MAX_VAL.ToString() : num.ToString());
        }

        private bool mapChecks(int x, int y)
        {
            if (!walkabilityMap[x, y])
                return false;

            double lVal = double.MaxValue;

            foreach (var dir in AdjacencyRule.EIGHT_WAY.DirectionsOfNeighborsClockwise())
            {
                int nX = x + dir.DeltaX;
                int nY = y + dir.DeltaY;

                if (nX < 0 || nY < 0 || nX >= walkabilityMap.Width || nY >= walkabilityMap.Height)
                    continue;

                if (dijkstraMap[nX, nY] < lVal)
                    lVal = dijkstraMap[nX, nY];
            }

            if (dijkstraMap[x, y] > lVal + 1)
            {
                dijkstraMap[x, y] = lVal + 1;
                return true;
            }

            return false;
        }
    }

    /// @endcond
}