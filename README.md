# GoRogue
Welcome to the homepage for GoRogue, the .NET Standard roguelike/2D game utility library!  This library is compatible with both .NET Framework and .NET Core projects, and offers a number of features that may be useful in roguelike development, including coordinate/grid system utilities, random number generation interfaces, a robust effects system, unobtrusive and flexible algorithms for map generation, FOV, lighting/sense mapping, map generation, as well as various math/utility functions, data structures, and more features to come!  See feature list below (coming soon) for details.  Also see the roadmap for planned major features!

## Documentation
A tutorial-style demo of GoRogue features is on the roadmap.  Currently, the API documentation is hosted on GitHub pages [here](https://chris3606.github.io/GoRogue).  The same documentation can be found in the docs folder in the root of the repository.

## Feature List
### Unobtrusive Algorithms
- FOV, Lighting/SenseMapping, and Map Generation algorithms operate on an abstract interface (MapOf), thus allowing the features to be used without imposing limitations and how/where data is stored within the game.

- A default implementation of the MapOf interface is provided, to allow for ease of use in straightforward cases or during prototyping.
   - ArrayMapOf implements MapOf and stores data in a 2D array for cases when a simple/straightforward MapOf implementation is needed.
  
### Coordinate/Grid System
- Coord class provides a way to store 2D grid (integer) coordinates
   - Pooling is used to allow extensive use without significant allocation and memory overhead
   - Numerous operators are provided to allow for seamless addition, subtraction, multiplication, and division of Coord instances, as well as addition/subtraction/multiplication/division by constants.
    - Static flag provided for whether Y-values decrease or increase in the downward direction, so that such functions can be used regardless of what coordinate scheme is being used.
   - Functions are provided to perform utility functions such as determining the bearing of a line, as well as retrieval of all points on a given line (via Brensham's), or cardinal line.
   - Also provides methods that implement other mathmematical grid functions, including midpoint formula, and translation of 2D coordinates to a 1D array index and back.
   - Provides hashing function that has a very low collision rate, particularly when considering coordiates between (-3, -3) and (255, 255).
   
- Direction class pairs with Coord to provide convenient ways to model movement to adjacent grid coordinates, as well as iterations through adjacent "neighbors" of a given location in both 4-way and 8-way movement schemes.
   - Directions can be added to Coord instances to get teh Coord directly adjacent to the original, in the direction specified.
   - Methods that generation IEnumerables of neighboring directions in various orders are provided.
   - Functions are given to determine direction most closely matching a line between two points.
   
- Distance class models 2D distance calculations in an abstract way, allowing algorithms to function no matter which distance calculation is being used
   - Manhattan, Chebyshev, and Euclician distance calculations are implemented.
   
- Radius type models the radius shapes assumed by above distance calculations.
   - Explicitly castable to Distance types and back.
   - RadiusAreaProvider class allows the easy retrieval of all coordinates within a defined radius (and optionally a bounding box).
   
- Rectangle class represents a rectangular area on a 2D grid, and provides useful operations for such areas.

- ISpatialMap implementations provide storing of object(s) at a given location in an efficient way.
   - Provides average-case constant time lookup of object(s) at a location.
   - Retrieval of all objects in the SpatialMap in linear time (equivalent efficiency to vector).
   - Less memory overhead than storing objects in 2D array.

### Random Number Generation

### Dice Parser

## Roadmap
This library is still in development - there are a number of important features on the horizon! These include:
- Pathfinding
   - At least AStar pathing, as well as Dijkstra maps (commonly known as Goal Maps) will be provided soon!
- Statistics Library
   - Utility classes to assist in dealing with interdependent character/monster statistics.
- Demo Project/writeup
- Map generation improvements
   - More map generation algorithms (BSP Tree)
   - Possibly improve RandomRoomsMapGenerator - change room placement strategy to be more even, or replace with BSP tree.

## Licensing
### GoRogue
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
### Other Licenses
See links to licenses in the credits for respective libraries.

## Credits
### Dice Notation .NET
Significant portions of the GoRogue.DiceNotation namespace are taken directly from the Dice Notatation .NET library.  This project is also licensed under MIT:
- [Dice Notation .NET](https://dicenotation.codeplex.com/SourceControl/latest)
- [Dice Notation .NET License](https://dicenotation.codeplex.com/license)
### RogueSharp
Significant portions of code in the GoRogue.Random namespace, as well as some implementatation details in the GoRogue.MapGeneration and GoRogue.DiceNotation namespaces were taken directly from the C# Library RogueSharp.  This project is also licensed under MIT:
- [RogueSharp](https://bitbucket.org/FaronBracy/roguesharp)
- [RogueSharp License](https://bitbucket.org/FaronBracy/roguesharp/src/master/LICENSE.txt?at=master)
### Doryen Library (libtcod)
This classic roguelike toolkit library was a significant general inspiration for GoRogue.
- [Libtcod](https://bitbucket.org/libtcod/libtcod)
- [Libtcod License](https://bitbucket.org/libtcod/libtcod/src/default/LIBTCOD-LICENSE.txt?at=default)
### SquidLib
This Java roguelike library is another big inspiration for much of the functionality of GoRogue.  A similar RIPPLE algorithm is used in SenseMap/LOS. As well, inspiration for the way Coord and SpatialMap function is taken from SquidLib's implementations.  No source code from SquidLib is directly used, and no project in GoRogue depends on SquidLib or uses SquidLib binaries.
- [SquidLib](https://github.com/SquidPony/SquidLib)
- [SquidLib License](https://github.com/SquidPony/SquidLib/blob/master/LICENSE.txt)
