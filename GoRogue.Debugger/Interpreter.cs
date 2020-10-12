using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GoRogue.MapViews;
using Mindmagma.Curses;
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

        private static IntPtr _window;

        #region Setup

        /// <summary>
        /// Select the routine and initialize it.
        /// </summary>
        public static void Init()
        {
            // Initialize NCurses
            _window = NCurses.InitScreen();
            NCurses.CBreak();
            NCurses.NoEcho();
            NCurses.Keypad(_window, true);

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
                int y = 0;

                // Clear current data from console
                NCurses.Erase();

                // Display selection menu for routines
                NCurses.MoveAddString(y, 0, "Pick your routine by using its corresponding number:");
                y += 1;

                List<IRoutine> routines = GetRoutines();
                foreach (var (routine, index) in routines.Select((routine, index) => (routine, index)))
                {
                    NCurses.MoveAddString(y, 0, index + ") " + routine.Name);
                    y += 1;
                }

                // Move cursor to new line
                NCurses.Move(y, 0);

                // Render menu to screen
                NCurses.Refresh();

                // Read user input and parse into index
                int key = NCurses.GetChar();
                try
                {
                    // Convert the character code we get to string, then from that to the integer that was typed.
                    string number = ((char)key).ToString();
                    int index = int.Parse(number);
                    chosenRoutine = routines[index];
                }
                catch // Invalid selection: Display retry message to user.
                {
                    // Clear old menu
                    NCurses.Erase();

                    // Print retry message
                    NCurses.MoveAddString(0, 0,
                        "Invalid selection.  Please select from the available options.  Press [Enter] to continue.");

                    // Display to screen and wait for user to acknowledge
                    NCurses.Refresh();
                    NCurses.GetChar();
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
                .Where(t => t.GetInterface(nameof(IRoutine)) != null && !t.IsAbstract)
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

            NCurses.EndWin();

        }

        #endregion
        #region UI
        private static void InterpretKeyPress()
        {
            Direction moveViewportDir = Direction.None;

            int key = NCurses.GetChar();
            switch (key)
            {
                case CursesKey.ESC:
                    _exit = true;
                    break;
                case ' ':
                    _routine.NextTimeUnit();
                    _dirty = true;
                    break;
                // Backspace can be 3 different key-codes, depending on platform
                case CursesKey.BACKSPACE:
                case '\b':
                case 127:
                    _routine.LastTimeUnit();
                    _dirty = true;
                    break;
                case CursesKey.LEFT:
                    moveViewportDir = Direction.Left;
                    break;
                case CursesKey.RIGHT:
                    moveViewportDir = Direction.Right;
                    break;
                case CursesKey.UP:
                    moveViewportDir = Direction.Up;
                    break;
                case CursesKey.DOWN:
                    moveViewportDir = Direction.Down;
                    break;
                case '+':
                    _mapView.NextView();
                    _dirty = true;
                    break;
                case '-':
                    _mapView.PreviousView();
                    _dirty = true;
                    break;
                default:
                    _routine.InterpretKeyPress(key);
                    break;
            }

            if (moveViewportDir != Direction.None)
                _dirty = _mapView.CenterViewOn(_mapView.CurrentViewport.ViewArea.Center + moveViewportDir);
        }

        private static void DrawMap()
        {
            // Clear current data from console
            NCurses.Erase();

            // Calculate available console space.
            NCurses.GetMaxYX(_window, out int height, out int width);

            // Resize viewport as needed to match console size
            _mapView.ResizeViewport(width, height);

            // Change each character of the console as appropriate
            foreach (var pos in _mapView.CurrentViewport.Positions())
            {
                NCurses.Move(pos.Y, pos.X);
                NCurses.InsertChar(_mapView.CurrentViewport[pos]);
            }

            // Render data to screen
            NCurses.Refresh();

            // Reset dirty flag because we just drew
            _dirty = false;
        }
        #endregion
    }
}
