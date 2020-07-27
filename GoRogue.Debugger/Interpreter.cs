using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using GoRogue.Debugger.Extensions;
using GoRogue.GameFramework;
using GoRogue.MapViews;
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
        //private static bool _calculateFov;//whether to view an FOV radius, or the whole map
        private static Point _position;//the printing location of the viewport on the Map
        private static bool _dirty = true; //whether or not to redraw the map
        private static Viewport<char> Viewport => GenerateMapView();//the "visible" region of the map
        private static Map? Map => Routine?.TransformedMap; //the map that we're printing to the console
        private static IRoutine? Routine { get; set; } //the routine that we're running in this test

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
            Routine?.GenerateMap();
            if (Map != null) _position = (Map.Width / 2 - _width / 2, Map.Height / 2 - _height / 2);
        }

        /// <summary>
        /// Creates the map view to display
        /// </summary>
        private static Viewport<char> GenerateMapView()
        {
            Debug.Assert(Map != null, nameof(Map) + " != null");
            return new Viewport<char>(Map.CharMap(), new Rectangle(_position, _position + (_width, _height)));
        }

        /// <summary>
        /// Picks a routine out of the possible candidates
        /// </summary>
        /// <returns>The Routine we're running in this test</returns>
        private static IRoutine? PickRoutine()
        {
            Console.WriteLine("Pick your routine using numbers");
            List<IRoutine?> routines = GetRoutines();
            int i = 0;
            foreach (var routine in routines)
            {
                Console.WriteLine(i + ") " + routine?.Name);
                i++;
            }
            var key = Console.ReadKey().Key;
            try
            {
                string number = key.ToString().Replace("D", "").Replace("NumPad", "");
                i = Int32.Parse(number);
                return routines[i];
            }
            catch
            {
                Console.WriteLine("Could not understand input; please use numbers");
                return PickRoutine();
            }
        }

        /// <summary>
        /// Gets all of the classes that implement IRoutine
        /// </summary>
        /// <returns>A list of all routines available</returns>
        private static List<IRoutine?> GetRoutines()
        {
            List<IRoutine?> objects = new List<IRoutine?>();
            Type[]? types = Assembly.GetAssembly(typeof(IRoutine))?.GetTypes();
            types = types.Where(t => t.GetInterface(nameof(IRoutine)) != null).ToArray();
            foreach (Type type in types)
            {
                objects.Add(Activator.CreateInstance(type) as IRoutine);
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
        private static void GoRogueGameFrame() => Routine?.ElapseTimeUnit();

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
            _width = System.Console.WindowWidth - 1;
            _height = System.Console.WindowHeight - 1;
            string output = Viewport.ExtendToString(elementSeparator:"");
            Console.Write(output);
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
