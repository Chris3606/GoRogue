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
        private const int _width = 40;
        private const int _height = 35;
        private static readonly Point _goal = (5, 5);

        [Fact]
        public void GoalMapLeadsToGoal()
        {
            var map = MockMaps.Rectangle(_width, _height);

            var goalMapData = new ArrayView<GoalState>(map.Width, map.Height);
            goalMapData.ApplyOverlay(
                new LambdaTranslationGridView<bool, GoalState>(map, i => i ? GoalState.Clear : GoalState.Obstacle));
            goalMapData[_goal] = GoalState.Goal;

            var goalMap = new GoalMap(goalMapData, Distance.Chebyshev);
            goalMap.Update();

            foreach (var startPos in goalMap.Positions().Where(p => map[p] && p != _goal))
            {
                var pos = startPos;
                while (true)
                {
                    var dir = goalMap.GetDirectionOfMinValue(pos);
                    if (dir == Direction.None)
                        break;
                    pos += dir;
                }

                Assert.Equal(_goal, pos);
            }
        }
    }
}
