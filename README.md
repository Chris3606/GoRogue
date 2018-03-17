# GoRogue
Welcome to the homepage for GoRogue, the .NET Standard roguelike/2D game utility library!  This library is compatible with both .NET Framework and .NET Core projects, and offers a number of features that may be useful in roguelike development, including coordinate/grid system utilities, random number generation interfaces, dice notation parsing/rolling methods, unobtrusive and flexible algorithms for map generation, FOV, and lighting/sense mapping, a robust effects system, and various math/utility functions, data structures, and more features to come!  See feature list below for details.  Also see the roadmap for planned major features!

## Table of Contents
- [GoRogue](#gorogue)
	- [Documentation](#documentation)
	- [Feature List](#feature-list)
		- [.NET Standard 2.0 Compatibility](#net-standard-20-compatibility)
		- [Unobtrusive Algorithms](#unobtrusive-algorithms)
		- [Coordinate/Grid System](#coordinategrid-system)
		- [Random Number Generation](#random-number-generation)
		- [Dice Notation Parser](#dice-notation-parser)
		- [Map Generation](#map-generation)
		- [FOV/Lighting/Sense Mapping](#fovlightingsense-mapping)
		- [Pathfinding](#pathfinding)
		- [Line Drawing](#line-drawing)
		- [Robust Effects System](#robust-effects-system)
		- [Utility](#utility)
	- [Roadmap](#roadmap)
	- [Licensing](#licensing)
		- [GoRogue](#gorogue-1)
		- [Other Licenses](#other-licenses)
	- [Credits](#credits)
		- [Dice Notation .NET](#dice-notation-net)
		- [RogueSharp](#roguesharp)
		- [Doryen Library (libtcod)](#doryen-library-libtcod)
		- [SquidLib](#squidlib)

## Documentation
Instructions for getting started with GoRogue, as well as demonstrations of its features, can be found on the [wiki](https://github.com/Chris3606/GoRogue/wiki).  In addition, the API documentation is hosted on [GitHub pages](https://chris3606.github.io/GoRogue).  The same documentation can be found in the docs folder in the root of the repository.  GoRogue also has a subreddit at [r/GoRogueLib](https://www.reddit.com/r/GoRogueLib/).

## Feature List
### .NET Standard 2.0 Compatibility
   - Library will function with any framework that supports .NET Standard 2.0, which includes both .NET Framework and .NET Core.
   
### Unobtrusive Algorithms
- FOV, Lighting/SenseMapping, and Map Generation algorithms operate on an abstract interface (MapView), thus allowing the features to be used without imposing limitations and how/where data is stored within the game.
- A default implementation of the MapView interface is provided, to allow for ease of use in straightforward cases or during prototyping:
   - ArrayMap implements MapView and stores data in a 2D array for cases when a simple/straightforward MapView implementation is needed.
  
### Coordinate/Grid System
- Coord class provides a way to store 2D grid (integer) coordinates:
   - Pooling is used to allow extensive use without significant allocation and memory overhead.
   - Numerous operators are provided to allow for seamless addition, subtraction, multiplication, and division of Coord instances, as well as addition/subtraction/multiplication/division by constants.
    - Static flag provided for whether Y-values decrease or increase in the downward direction, so that such functions can be used regardless of what coordinate scheme is being used.
   - Functions are provided to perform utility functions such as determining the bearing of a line, as well as retrieval of all points on a given line (via Brensham's), or cardinal line.
   - Also provides methods that implement other mathmematical grid functions, including midpoint formula, and translation of 2D coordinates to a 1D array index and back.
   - Provides hashing function that has a very low collision rate, particularly when considering coordiates between (-3, -3) and (255, 255).
- Direction class pairs with Coord to provide convenient ways to model movement to adjacent grid coordinates, as well as iterations through adjacent "neighbors" of a given location in both 4-way and 8-way movement schemes:
   - Directions can be added to Coord instances to get the Coord directly adjacent to the original, in the direction specified.
   - Methods that generation IEnumerables of neighboring directions in various orders are provided.
   - Functions are given to determine direction most closely matching a line between two points.
- Distance class models 2D distance calculations in an abstract way, allowing algorithms to function no matter which distance calculation is being used:
   - Manhattan, Chebyshev, and Euclician distance calculations are implemented.
- Radius type models the radius shapes assumed by above distance calculations:
   - Explicitly castable to Distance types and back.
   - RadiusAreaProvider class allows the easy retrieval of all coordinates within a defined radius (and optionally a bounding box).
- Rectangle class represents a rectangular area on a 2D grid, and provides useful operations for such areas:
- ISpatialMap implementations provide storing of object(s) at a given location in an efficient way:
   - Provides average-case constant time lookup of object(s) at a location.
   - Retrieval of all objects in the SpatialMap in linear time (equivalent efficiency to vector).
   - Less memory overhead than storing objects in 2D array.

### Random Number Generation
- Provides easy to use random number generators, that wrap around the C# default RNGs, as well as create custom distributions:
   - DotNetRandom provides a wrapper around the default C# RNG that implements the IRandom interface.
   - GaussianRandom implements an RNG that returns numbers on a bell curve, with the capability to specify min and max values.
   - IRandom implementations intended for testing are provided, to allow for easier unit testing/debugging of functions that use RNGs:
      - KnownSeriesRandom returns a specified series of numbers (looping through).
      - MaxRandom always returns the max parameter specified to the Next function.
      - MinRandom always returns the min parameter specified to the Next function.
- SingletonRandom provides a static field that can be used to conveniently set the default RNG that functions needing RNGs will use if a particular one is not specified.

### Dice Notation Parser
- Provides a system for parsing expressions in Dice notation format, and rolling those dice using the library's provided number generators.
- Expression objects are provided to avoid expensive parsing operations every time a roll is completed.

### Map Generation
- Map generation algorithms operate on MapView<bool> types, to avoid intrusiveness.
- Algorithms are modularized, as to provide maximum reuse:
   - Generation and connectivity algorithms are seperated to provide maximum flexibility.
   - Different common components of connectivity algorithms are also separated.
   - In all these cases, reasonable defaults are provided to prevent the addition of unnecessary complexity to simple/prototyping cases.
- Various methods of generation are provided:
   - RectangleMapGenerator generates a simple rectangle, with walls along the edges, and floor elsewhere.
   - RandomRoomsGenerator generates a dungeon with multiple rectangular, connected rooms. Number of rooms, as well as size ranges for rooms and maximum number of tries to place a room, is parameterized.
   - CellularAutomataGenerator generates a dungeon using the cellular automata smoothing process detailed [here](http://www.roguebasin.com/index.php?title=Cellular_Automata_Method_for_Generating_Random_Cave-Like_Levels).
- MapArea and MapAreaFinder provide convenient ways of representing arbitrarily-shaped sections of the map, and locating all such distinct regions.

### FOV/Lighting/Sense Mapping
- LOS class offers fairly standard 2D FOV using shadowcasting:
   - FOV can be calculated in any of several shapes (modeled by Radius class instances).
   - Length of the radius can be specified, or infinite.
   - Resulting FOV is a lightmap -- an array of doubles in range \[0.0, 1.0], such that the value decreases from 1.0 proportionally as distance from the source increases.
   - Provides convenience fields to access only those (new) cells in/out of FOV.
- SenseMap/SenseSource pair to offer much more advanced FOV/lighting features:
   - RIPPLE algorithm can be used to model light spreading around corners, which allows locations to only partially block spread of light.
   - Sources may use either the RIPPLE algorithm variations, or shadowcasting, to model their spreading.
   - Tracks and calculates multiple, mobile light sources, in a multi-threaded manner, and consolidates them into a single light map.
   - SenseMap can also be used to model other sensory perceptions.
   
### Pathfinding
   - AStar pathfinding allows quick and efficient (shortest) paths between two points on a map.
      - Uses same MapView system as other algorithms to provide a convenient interface.
      
### Line Drawing
   - Provides functions implementing common-line drawing algorithms.
      - Bresenham's implementation
      - DDA (Digital Differential Analyzer) implementation that produces very similar results to Bresenham's, but often faster.
      - Orthogonal line-drawing algorithm that creates steps that follow only cardinal directions.

### Robust Effects System
- Provides a system of representing both instant duration, and over-time "effects", with arbitrary duration units.
- EffectTrigger can be used to trigger effects at certain arbitrary points, automatically reducing their duration:
   - Effects can be canceled and have infinite duration.
   - Effects are automatically removed from an EffectTrigger when their duration expires.

### Utility
- Provides math functions to handle properly wrapping array/menu indices, radian-degree conversions, and wrapping to nearest multiples.
- Extension method provided for List that implements a Fisher-Yates shuffle.
- Extension methods for IList are provided to select either a random index or random value from an IList.
- Extension method provided for IEnumerable to convert the IEnumerable to a List.
- Provides basic integer-based DisjointSet data structure, that implements path compression.

## Roadmap
This library is still in development - there are a number of important features on the horizon! These include:
- Pathfinding Additions
   - AStar implementation supporting custom heuristics/weights
   - Dijkstra maps (commonly known as Goal Maps)
- Statistics Library
   - Utility classes to assist in dealing with interdependent character/monster statistics.
- Demo Project/writeup
- Map generation improvements
   - More map generation algorithms (BSP Tree).
   - Possibly improve RandomRoomsMapGenerator - change room placement strategy to be more even, or replace with BSP tree.
- Additional FOV Algorithms
   - Permissive-style FOV
   - Others?

## Licensing
### GoRogue
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.
### Other Licenses
See links to licenses in the credits for respective libraries.

## Credits
### Dice Notation .NET
General inspiration for the architecture of the GoRogue.DiceNotation namespace was taken from the Dice Notatation .NET library.  This project is also licensed under MIT:
- [Dice Notation .NET](https://dicenotation.codeplex.com/SourceControl/latest)
- [Dice Notation .NET License](https://dicenotation.codeplex.com/license)
### RogueSharp
General inspiration for some algorithms available in the GoRogue.MapGeneration namespace were taken from the C# library RogueSharp.  This project is also licensed under MIT:
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
