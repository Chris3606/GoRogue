using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static bool _exit; // Used to decide whether or not to exit the program
        private static bool _dirty = true; // Whether or not to redraw the map

        // The routine that we're running in this test.  Null override because we initialize in Init
        private static IRoutine _routine = null!;

        // Viewport of routine's map.  Null override because we initialize in Init
        private static RoutineViewport _mapView = null!;

        #region Setup

        /// <summary>
        /// Select the routine and initialize it.
        /// </summary>
        public static void Init()
        {
            // Pick routine, and generate its map and views
            _routine = PickRoutine();
            _routine.GenerateMap();
            _routine.CreateViews();

            // Set up viewport, defaulting to first item in views list
            _mapView = new RoutineViewport(_routine, Console.WindowWidth - 1, Console.WindowHeight - 1);
        }

        /// <summary>
        /// Picks a routine out of the possible candidates
        /// </summary>
        /// <returns>The Routine we're running in this test</returns>
        private static IRoutine PickRoutine()
        {
            IRoutine? chosenRoutine = null;
            do
            {
                // Display selection menu for routines
                Console.WriteLine("Pick your routine by using its corresponding number:");
                List<IRoutine> routines = GetRoutines();
                foreach (var (routine, index) in routines.Select((routine, index) => (routine, index)))
                    Console.WriteLine(index + ") " + routine.Name);

                // Read user input and parse into index
                var key = Console.ReadKey().Key;
                try
                {
                    string number = key.ToString().Replace("D", "").Replace("NumPad", "");
                    var index = int.Parse(number);
                    chosenRoutine = routines[index];
                }
                catch
                {
                    Console.WriteLine("Invalid selection.  Please select from the available options.");
                }
            } while (chosenRoutine == null);

            return chosenRoutine;
        }

        /// <summary>
        /// Gets all of the classes that implement IRoutine.
        /// </summary>
        /// <returns>A list of all routines available.</returns>
        private static List<IRoutine> GetRoutines()
        {
            // Get all types implementing IRoutine in current assembly
            Type[] routineTypes = Assembly.GetAssembly(typeof(IRoutine))?.GetTypes() ??
                           throw new Exception("Can't find assembly that defines IRoutine.");
            routineTypes = routineTypes
                .Where(t => t.GetInterface(nameof(IRoutine)) != null)
                .Where(t => !t.IsAbstract)
                .ToArray();

            // Use the parameterless constructor for each type to create an instance of that type.
            // Overriding to non-nullable because the next stage will exit the function if there are any nulls
            List<IRoutine> routineInstances = routineTypes
                .Select(type => Activator.CreateInstance(type) as IRoutine)
                .ToList()!;

            // Null indicates Nullable types, which should not occur; IRoutines can't be a type that implements
            // Nullable<T>
            if (routineInstances.Any(inst => inst == null))
                throw new Exception("Cannot use Nullable<T> instances as implementers of IRoutine.");

            // Sort the list by name then return
            routineInstances.Sort(
                (r1, r2) => string.CompareOrdinal(r1.Name, r2.Name));

            return routineInstances;
        }
        #endregion

        #region Run

        /// <summary>
        /// Start listening for keypresses.
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
                    _routine.NextTimeUnit();
                    _dirty = true;
                    break;
                case ConsoleKey.Backspace:
                    _routine.LastTimeUnit();
                    _dirty = true;
                    break;
                case ConsoleKey.LeftArrow:
                    moveViewportDir = Direction.Left;
                    break;
                case ConsoleKey.RightArrow:
                    moveViewportDir = Direction.Right;
                    break;
                case ConsoleKey.UpArrow:
                    moveViewportDir = Direction.Up;
                    break;
                case ConsoleKey.DownArrow:
                    moveViewportDir = Direction.Down;
                    break;
                case ConsoleKey.OemPlus:
                    _mapView.NextView();
                    _dirty = true;
                    break;
                case ConsoleKey.OemMinus:
                    _mapView.PreviousView();
                    _dirty = true;
                    break;
                default:
                    _routine.InterpretKeyPress(key);
                    break;
            }

            if (moveViewportDir != Direction.None)
            {
                _dirty = _mapView.CenterViewOn(_mapView.CurrentViewport.ViewArea.Center + moveViewportDir);
            }
        }

        private static void DrawMap()
        {
            // Calculate available console space.  Make sure to subtract one to ensure we actually fit
            // text instead of going 1 over (because the write will include a newline).
            int width = Console.WindowWidth - 1;
            int height = Console.WindowHeight - 1;

            // Resize viewport as needed to match console size
            _mapView.ResizeViewport(width, height);

            // Get string constituting the viewport, ensuring to allow no space between characters
            var lines = _mapView.CurrentViewport.ExtendToString(elementSeparator: "").Split('\n');
            foreach (var line in lines)
                Console.WriteLine(line);

            // Print as many new lines as required to ensure that we don't end up with part of the old map on screen
            for (int i = lines.Length; i < height; i++)
                Console.WriteLine();

            // Reset dirty flag because we just drew
            _dirty = false;
        }
        #endregion
    }
}
