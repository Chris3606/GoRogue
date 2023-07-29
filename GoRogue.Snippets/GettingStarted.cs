#region RequiredIncludes
using GoRogue.FOV;
using GoRogue.MapGeneration;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;
#endregion

namespace GoRogue.Snippets
{
    public static class GettingStarted
    {
        public static void ExampleCode()
        {
            #region ExampleMainFunction
            var testMap = new Generator(5, 5)
                .ConfigAndGenerateSafe( gen =>
                    gen.AddSteps(DefaultAlgorithms.RectangleMapSteps()))
                .Context
                .GetFirst<IGridView<bool>>("WallFloor");

            var fov = new RecursiveShadowcastingFOV(testMap);
            fov.Calculate(new Point(2, 2), 5);

            Console.WriteLine(
                fov.BooleanResultView.ExtendToString(elementStringifier: i => i ? "T" : "F"));

            // Used to stop window from closing until a key is pressed.
            Console.Read();
            #endregion
        }
    }
}
