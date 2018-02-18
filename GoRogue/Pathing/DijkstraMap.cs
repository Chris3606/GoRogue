namespace GoRogue.Pathing
{
    /// @cond PRIVATE
    // Test class only - likely to change in the future.
    public class DijkstraMap
    {
        private int[,] dijkstraMap;
        private IMapView<bool> walkabilityMap;

        public DijkstraMap(IMapView<bool> walkabilityMap)
        {
            this.walkabilityMap = walkabilityMap;
            dijkstraMap = new int[walkabilityMap.Width, walkabilityMap.Height];

            for (int x = 0; x < walkabilityMap.Width; x++)
                for (int y = 0; y < walkabilityMap.Height; y++)
                    dijkstraMap[x, y] = int.MaxValue - 1;
        }

        public int this[int x, int y]
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
                        dijkstraMap[x, y] = int.MaxValue - 1;

            int changes = 1;
            while (changes > 0)
            {
                changes = 0;
                for (int x = 0; x < walkabilityMap.Width; x++)
                    for (int y = 0; y < walkabilityMap.Height; y++)
                    {
                        if (!walkabilityMap[x, y])
                            continue;

                        int lVal = int.MaxValue;

                        foreach (var dir in AdjacencyRule.CARDINALS.DirectionsOfNeighborsClockwise())
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
                            changes++;
                        }
                    }
            }
        }

        public void RemoveGoal(int x, int y)
        {
            dijkstraMap[x, y] = int.MaxValue - 1;
        }
    }

    /// @endcond
}