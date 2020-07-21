using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GoRogue.GameFramework;
using GoRogue.MapViews;
using GoRogue.Debugger.Implementations.GameObjects;
using SadRogue.Primitives;

namespace GoRogue.Debugger
{
    /// <summary>
    /// The "Interpreter" is a class designed to sit between GoRogue
    /// and System.Console. It prints output directly to the console,
    /// allowing debugging directly in a terminal.
    /// </summary>
    public static class Interpreter
    {
        private static bool _exit; // Used to decide whether or not to exit the program
        private static bool _dirty = true; // Whether or not to redraw the map

        private static IRoutine? _routine; // The routine that we're running in this test

        // TODO: Temp: this should be in routine
        private static IMapView<char>? _characterMap;

        // Viewport of visible map
        private static Viewport<char>? _mapView;

        #region Setup
        /// <summary>
        /// Get the Routine, Interpreter, and Map ready for action.
        /// </summary>
        public static void Init()
        {
            // Pick routine and set up its map
            _routine = PickRoutine();
            _routine.GenerateMap();

            // TODO: Temp; initialize character view to translate to terrain types
            _characterMap = new LambdaTranslationMap<IEnumerable<IGameObject>,char>(_routine.Map!,
                objs => (char)(objs.Cast<EntityBase>().FirstOrDefault()?.Glyph ?? '?'));

            // Initialize viewport
            _mapView = new Viewport<char>(_characterMap,
                new Rectangle(0, 0, Console.WindowWidth - 1, Console.WindowHeight - 1));
            _mapView.SetViewArea(_mapView.ViewArea.WithCenter((_routine.Map!.Width / 2, _routine.Map!.Height / 2)));

            Console.WriteLine("Initialized...");
        }

        /// <summary>
        /// Picks a routine out of the possible candidates
        /// </summary>
        /// <returns>The Routine we're running in this test</returns>
        private static IRoutine PickRoutine()
        {
            Console.WriteLine("Pick your routine using numbers");
            List<IRoutine> routines = GetRoutines();
            int i = 0;
            foreach (var routine in routines)
            {
                Console.WriteLine(i + ") " + routine.Name);
                i++;
            }
            var key = Console.ReadKey().Key;
            try
            {
                string number = key.ToString().Replace("D", "").Replace("NumPad", "");
                i = int.Parse(number);
                return routines[i];
            }
            catch (Exception)
            {
                Console.WriteLine("Could not understand input; please use numbers");
                return PickRoutine();
            }
        }

        /// <summary>
        /// Gets all of the classes that implement IRoutine
        /// </summary>
        /// <returns>A list of all routines available</returns>
        private static List<IRoutine> GetRoutines()
        {
            List<IRoutine> objects = new List<IRoutine>();
            var types = Assembly.GetAssembly(typeof(IRoutine))?.GetTypes() ?? Array.Empty<Type>();
            types = types.Where(t => t.GetInterface(nameof(IRoutine)) != null).ToArray();
            foreach (Type type in types)
            {
                var instance = Activator.CreateInstance(type) as IRoutine ??
                               throw new Exception("Failed to create instance of routine.");
                objects.Add(instance);
            }
            objects.Sort();
            return objects;
        }
        #endregion

        #region Run

        /// <summary>
        /// Start listening for keypresses
        /// </summary>
        public static void Run()
        {
            while (!_exit)
            {
                if (_dirty)
                    DrawMap();

                InterpretKeyPress();
            }
        }

        #endregion

        #region UI
        private static void InterpretKeyPress()
        {
            Direction moveViewportDir = Direction.None;
            ConsoleKey key = Console.ReadKey().Key;
            switch (key)
            {
                case ConsoleKey.Escape:
                    _exit = true;
                    break;
                case ConsoleKey.Spacebar:
                    _routine?.ElapseTimeUnit();
                    _dirty = true;
                    break;
                case ConsoleKey.UpArrow:
                    moveViewportDir = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                    moveViewportDir = Direction.Down;
                    break;
                case ConsoleKey.LeftArrow:
                    moveViewportDir = Direction.Left;
                    break;
                case ConsoleKey.RightArrow:
                    moveViewportDir = Direction.Right;
                    break;
            }

            if (moveViewportDir != Direction.None)
            {
                Point center = _mapView!.ViewArea.Center;
                _mapView!.SetViewArea(_mapView.ViewArea.Translate(moveViewportDir));

                if (center != _mapView.ViewArea.Center) // Actually changed, eg. we weren't on edge of map on update
                    _dirty = true;
            }
        }

        private static void DrawMap()
        {
            // Calculate available console space.  Make sure to subtract one to ensure we actually fit
            // text instead of going 1 over with newline.
            int width = Console.WindowWidth - 1;
            int height = Console.WindowHeight - 1;

            // If console size has changed, resize viewport and re-center on same location.
            if (_mapView!.ViewArea.Width != width || _mapView!.ViewArea.Height != height)
            {
                var center = _mapView.ViewArea.Center;
                _mapView.SetViewArea(_mapView.ViewArea.WithSize(width, height).WithCenter(center));
            }

            // Draw viewport, ensuring to allow no space between characters
            Console.WriteLine(_mapView.ExtendToString(elementSeparator: ""));

            // Reset dirty flag because we just drew
            _dirty = false;
        }
        #endregion
    }
}
