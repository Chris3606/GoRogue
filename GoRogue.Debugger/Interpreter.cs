using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GoRogue;
using GoRogue.GameFramework;
using GoRogue.MapGeneration;
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

        private static int _width; //width of the console
        private static int _height;//height of the console
        private static bool _invertY;//to use default, or make positions match cartesian graph
        private static bool _exit;//used to decide whether or not to exit the program
        private static bool _calculateFov;//whether to view an FOV radius, or the whole map
        private static Point _position;//the printing location of the viewport on the Map
        private static bool _dirty = true; //whether or not to redraw the map
        private static Map _map => Routine.TransformedMap; //the map that we're printing to the console
        public static IRoutine Routine { get; private set; } //the routine that we're running in this test

        //the "visible" region of the map
        private static Viewport<bool> _viewport =>
            new Viewport<bool>(_map.WalkabilityView, new Rectangle(_position.X, _position.Y, _width, _height));

        #region setup
        /// <summary>
        /// Get theRoutine, Interpreter, and Map ready for action.
        /// </summary>
        public static void Init()
        {
            _invertY = false;
            _exit = false;
            _width = System.Console.WindowWidth;
            _height = System.Console.WindowHeight;

            Routine = PickRoutine();
            Routine.GenerateMap();
            _position = (_map.Width / 2 - _width / 2, _map.Height / 2 - _height / 2);
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
                i = Int32.Parse(number);
                return routines[i];
            }
            catch (Exception e)
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
            var types = Assembly.GetAssembly(typeof(IRoutine)).GetTypes();
            types = types.Where(t => t.GetInterface(nameof(IRoutine)) != null).ToArray();
            foreach (Type type in types)
            {
                objects.Add((IRoutine)Activator.CreateInstance(type));
            }
            objects.Sort();
            return objects;
        }
        #endregion

        #region run

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

        /// <summary>
        /// Elapse a single unit of time
        /// </summary>
        private static void GoRogueGameFrame()
        {
            Routine.ElapseTimeUnit();
        }
        #endregion
        #region ui
        private static void InterpretKeyPress()
        {
            ConsoleKey key = Console.ReadKey().Key;

            if (key == ConsoleKey.Escape)
                _exit = true;
            else if (key == ConsoleKey.Spacebar)
                GoRogueGameFrame();

            else if (key == ConsoleKey.LeftArrow)
                ShiftLeft();
            else if (key == ConsoleKey.RightArrow)
                ShiftRight();
            else if (key == ConsoleKey.UpArrow)
                ShiftUp();
            else if (key == ConsoleKey.DownArrow)
                ShiftDown();
        }

        private static void DrawMap()
        {
            _width = System.Console.WindowWidth;
            _height = System.Console.WindowHeight;
            for (int i = _position.Y; i < _position.Y + _height - 1; i++)
            {
                string line = "";
                for (int j = _position.X; j < _position.X + _width - 1; j++)
                {
                    Terrain go = (Terrain) _map[j, i].FirstOrDefault();
                    if (go == null)
                        line += "?";

                    else
                        line += (char)go.Glyph;
                }
                Console.WriteLine(line);
            }
        }

        public static void ShiftLeft()
        {
            _position += (-1, 0);
            _dirty = true;
        }

        public static void ShiftUp()
        {
         _position += _invertY ? (0, 1) : (0, -1);
         _dirty = true;
        }
        public static void ShiftDown()
        {
            _position += _invertY ? (0, -1) : (0, 1);
            _dirty = true;
        }
        public static void ShiftRight()
        {
            _position += (1, 0);
            _dirty = true;
        }
        #endregion
    }
}
