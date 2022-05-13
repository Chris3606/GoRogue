using System.Linq;
using GoRogue.Pathing;
using GoRogue.UnitTests.Mocks;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
using Xunit;

namespace GoRogue.UnitTests.Pathing
{
    public class FleeMapTests
    {
        private const int _width = 40;
        private const int _height = 35;
        private static readonly Point _goal = (5, 5);

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
    }
}
