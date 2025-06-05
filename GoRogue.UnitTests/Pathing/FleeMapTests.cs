using System.Collections.Generic;
using System.Linq;
using GoRogue.Pathing;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;
using Xunit.Abstractions;

namespace GoRogue.UnitTests.Pathing
{
    public class FleeMapTests
    {
        private readonly ITestOutputHelper output;
        private const int _width = 40;
        private const int _height = 35;
        private static readonly Point _goal = (5, 5);

        public FleeMapTests(ITestOutputHelper output)
        {
            this.output = output;
        }
        [Fact]
        public void FleeMapDoesNotLeadToGoal()
        {
            var map = (ArrayView<bool>)MockMaps.Rectangle(_width, _height);

            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.ApplyOverlay(
                new LambdaTranslationGridView<bool, GoalState>(map, i => i ? GoalState.Clear : GoalState.Obstacle));
            goalMapData[_goal] = GoalState.Goal;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            using var fleeMap = new FleeMap(goalMap);
            goalMap.Update();

            foreach (var startPos in fleeMap.Positions().Where(p => map[p] && p != _goal))
            {
                var pos = startPos;
                var moves = 0;
                while (moves < map.Width * map.Height)
                {
                    var dir = fleeMap.GetDirectionOfMinValue(pos);
                    if (dir == Direction.None)
                        break;
                    pos += dir;

                    moves++;
                }

                Assert.NotEqual(_goal, pos);
            }
        }

        [Fact]
        public void OpenEdgedMapSupported()
        {
            var goalMapData = new ArrayView<GoalState>(_width, _height);
            goalMapData.Fill(GoalState.Clear);
            goalMapData[_width / 2, _height / 2] = GoalState.Goal;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            var fleeMap = new FleeMap(goalMap);
            goalMap.Update();

            foreach (var pos in fleeMap.Positions())
                Assert.NotNull(fleeMap[pos]);

            // TODO: Verify flee map leads away from goal
        }

        [Theory]
        [InlineData(1.2)] //go right
        [InlineData(1.6)] //go right
        [InlineData(1.7)] //go left !?
        public void test(double magnitude)
        {
            //map:
            //  #
            // #C
            //  #
            // Character (C) at (5,5) should flee threats (#) by going right
            var threatsCells = new List<SadRogue.Primitives.Point>
            {
                (5,4),
                (4,5),
                (5,6)
            };

            var gridView = new LambdaGridView<GoalState>(11, 11, cell => threatsCells.Contains(cell) ? GoalState.Goal : GoalState.Clear);
            var goalMap = new GoalMap(gridView, Distance.Manhattan);
            var fleeMap = new FleeMap(goalMap, magnitude);
            var currentCell = new SadRogue.Primitives.Point(5, 5);


            output.WriteLine(fleeMap.ToString(4));

            //Direction direction = fleeMap.GetDirectionOfMinValue(currentCell, AdjacencyRule.Cardinals);



            //Assert.Equal(Direction.Right, direction);
        }
    }
}
