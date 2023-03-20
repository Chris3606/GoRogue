using System.Linq;
using GoRogue.Pathing;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;

namespace GoRogue.UnitTests.Pathing
{
    public class GoalMapTests
    {
        private const int Width = 40;
        private const int Height = 35;
        private static readonly Point s_goal = (5, 5);

        [Fact]
        public void GoalMapLeadsToGoal()
        {
            var map = MockMaps.Rectangle(Width, Height);

            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.ApplyOverlay(
                new LambdaTranslationGridView<bool, GoalState>(map, i => i ? GoalState.Clear : GoalState.Obstacle));
            goalMapData[s_goal] = GoalState.Goal;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            foreach (var startPos in goalMap.Positions().Where(p => map[p] && p != s_goal))
            {
                var pos = startPos;

                var infiniteLoopBreakCount = map.Width * map.Height;
                while (infiniteLoopBreakCount > 0)
                {
                    var dir = goalMap.GetDirectionOfMinValue(pos);
                    if (dir == Direction.None)
                        break;
                    pos += dir;

                    infiniteLoopBreakCount--;
                }

                Assert.Equal(s_goal, pos);
            }
        }

        [Fact]
        public void OpenEdgedMapSupported()
        {
            var goalMapData = new ArrayView<GoalState>(Width, Height);
            goalMapData.Fill(GoalState.Clear);
            var goal = new Point(Width / 2, Height / 2);
            goalMapData[goal] = GoalState.Goal;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            foreach (var startPos in goalMap.Positions().Where(p => p != goal))
            {
                var pos = startPos;

                var infiniteLoopBreakCount = Width * Height;
                while (infiniteLoopBreakCount > 0)
                {
                    var dir = goalMap.GetDirectionOfMinValue(pos);
                    if (dir == Direction.None)
                        break;
                    pos += dir;
                    infiniteLoopBreakCount--;
                }

                Assert.Equal(goal, pos);
            }
        }

        [Fact]
        public void GetDirectionOfMinValueChecksMapBoundaryForOpenEdgedMaps()
        {
            var map = MockMaps.Rectangle(2, 1);

            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.Fill(GoalState.Clear);

            var goalPos = new Point(1, 0);

            goalMapData[goalPos] = GoalState.Goal;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            var startPos = new Point(0, 0);
            var dir = goalMap.GetDirectionOfMinValue(startPos);

            Assert.Equal(goalPos, startPos + dir);
        }

        [Fact]
        public void GetDirectionOfMinValueShouldPreferMovementOverNonMovementWhenNoGoals()
        {
            var map = MockMaps.Rectangle(2, 1);

            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.Fill(GoalState.Clear);

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            var dir1 = goalMap.GetDirectionOfMinValue(0, 0);
            var dir2 = goalMap.GetDirectionOfMinValue(1, 0);

            Assert.Equal(dir1, Direction.Right);
            Assert.Equal(dir2, Direction.Left);
        }

        [Fact]
        public void GoalMapShouldHaveMaxValuesWhenNoGoals()
        {
            var map = MockMaps.Rectangle(2, 1);

            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.Fill(GoalState.Clear);

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            var maxValue = map.Width * map.Height;

            Assert.Equal(maxValue, goalMap[0, 0]!.Value);
            Assert.Equal(maxValue, goalMap[1, 0]!.Value);
        }

        [Fact]
        public void GoalMapShouldHaveNullValueOnObstacles()
        {
            var map = MockMaps.Rectangle(1, 1);

            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height)
            {
                [0, 0] = GoalState.Obstacle
            };

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            Assert.False(goalMap[0, 0]!.HasValue);
        }

        [Fact]
        public void GetDirectionOfMinValueShouldReturnNoneIfStuckInsideObstacles()
        {
            //012
            //###
            //# #
            //###
            var map = MockMaps.Rectangle(3, 4);
            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.Fill(GoalState.Obstacle);
            goalMapData[0, 0] = GoalState.Goal;
            goalMapData[1, 0] = GoalState.Clear;
            goalMapData[2, 0] = GoalState.Clear;
            goalMapData[1, 2] = GoalState.Clear;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            var dir = goalMap.GetDirectionOfMinValue(1, 2);

            Assert.Equal(dir, Direction.None);
        }

        [Fact]
        public void GetDirectionOfMinValueShouldPreferMovementOverNonMovementIfStuckInsideObstacles()
        {
            //0123
            //####
            //#  #
            //####
            var map = MockMaps.Rectangle(4, 4);
            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.Fill(GoalState.Obstacle);
            goalMapData[0, 0] = GoalState.Goal;
            goalMapData[1, 0] = GoalState.Clear;
            goalMapData[2, 0] = GoalState.Clear;
            goalMapData[3, 0] = GoalState.Clear;
            goalMapData[1, 2] = GoalState.Clear;
            goalMapData[2, 2] = GoalState.Clear;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            var dir1 = goalMap.GetDirectionOfMinValue(1, 2);
            var dir2 = goalMap.GetDirectionOfMinValue(2, 2);

            Assert.Equal(dir1, Direction.Right);
            Assert.Equal(dir2, Direction.Left);
        }

        [Fact]
        public void GoalMapShouldContainMaxValuesForAreasStuckInsideObstacles()
        {
            //012
            //###
            //# #
            //###
            var map = MockMaps.Rectangle(3, 4);
            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.Fill(GoalState.Obstacle);
            goalMapData[0, 0] = GoalState.Goal;
            goalMapData[1, 0] = GoalState.Clear;
            goalMapData[2, 0] = GoalState.Clear;
            goalMapData[1, 2] = GoalState.Clear;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            Assert.Equal(map.Width * map.Height, goalMap[1, 2]!.Value);

            // Areas that have a goal reachable should still have normal calculated values
            Assert.Equal(0, goalMap[0, 0]!.Value);
            Assert.Equal(1, goalMap[1, 0]!.Value);
            Assert.Equal(2, goalMap[2, 0]!.Value);
        }
    }
}
