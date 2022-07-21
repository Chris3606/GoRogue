using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mindmagma.Curses;
using SadRogue.Primitives;
using SadRogue.Primitives.GridViews;

namespace GoRogue.Debugger
{
    /// <summary>
    /// The "Interpreter" is a class designed to sit between GoRogue
    /// and System.Console. It prints output directly to the console,
    /// allowing debugging directly in a terminal.
    /// </summary>
    public static class Interpreter
    {
        private static bool s_exit; // Used to decide whether or not to exit the program
        private static bool s_dirty = true; // Whether or not to redraw the map

        // The routine that we're running in this test.  Null override because we initialize in Init
        private static IRoutine s_routine = null!;

        // Viewport of routine's map.  Null override because we initialize in Init
        private static RoutineViewport s_mapView = null!;

        private static IntPtr s_window;

        #region Setup

        /// <summary>
        /// Select the routine and initialize it.
        /// </summary>
        public static void Init()
        {
            // Initialize NCurses
            s_window = NCurses.InitScreen();
            NCurses.CBreak();
            NCurses.NoEcho();
            NCurses.Keypad(s_window, true);

            // Pick routine, and generate its map and views
            s_routine = PickRoutine();
            s_routine.GenerateMap();
            s_routine.CreateViews();

            // Set up viewport, defaulting to first item in views list
            s_mapView = new RoutineViewport(s_routine, Console.WindowWidth - 1, Console.WindowHeight - 1);
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
            while (!s_exit)
            {
                if (s_dirty)
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
                    s_exit = true;
                    break;
                case ' ':
                    s_routine.NextTimeUnit();
                    s_dirty = true;
                    break;
                // Backspace can be 3 different key-codes, depending on platform
                case CursesKey.BACKSPACE:
                case '\b':
                case 127:
                    s_routine.LastTimeUnit();
                    s_dirty = true;
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
                    s_mapView.NextView();
                    s_dirty = true;
                    break;
                case '-':
                    s_mapView.PreviousView();
                    s_dirty = true;
                    break;
                default:
                    s_routine.InterpretKeyPress(key);
                    break;
            }

            if (moveViewportDir != Direction.None)
                s_dirty = s_mapView.CenterViewOn(s_mapView.CurrentViewport.ViewArea.Center + moveViewportDir);
        }

        private static void DrawMap()
        {
            // Clear current data from console
            NCurses.Erase();

            // Calculate available console space.
            NCurses.GetMaxYX(s_window, out int height, out int width);

            // Resize viewport as needed to match console size
            s_mapView.ResizeViewport(width, height);

            // Change each character of the console as appropriate
            foreach (var pos in s_mapView.CurrentViewport.Positions())
            {
                NCurses.Move(pos.Y, pos.X);
                NCurses.InsertChar(s_mapView.CurrentViewport[pos]);
            }

            // Render data to screen
            NCurses.Refresh();

            // Reset dirty flag because we just drew
            s_dirty = false;
        }
        #endregion
    }
}
