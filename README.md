# GoRogue
[![Chat on discord](https://img.shields.io/discord/660952837572001804.svg)](https://discord.gg/fxj5kPq)
[![Join us on Reddit](https://img.shields.io/badge/reddit-GoRogueLib-red.svg)](http://reddit.com/r/goroguelib)
[![NuGet](https://img.shields.io/nuget/v/GoRogue.svg)](http://www.nuget.org/packages/GoRogue/)

Welcome to the homepage for GoRogue, a modern .NET Standard roguelike/2D game utility library!  This library offers a number of features that may be useful for 2D grid-based/roguelike game development, including algorithms for calculating FOV, pathfinding, generating maps, drawing lines, generating random numbers, creating messaging system architectures, and much more!  See feature list below for details.

## Documentation
You can find getting started instructions, tutorial articles, and the API documentation on the [documentation website](http://www.roguelib.com). Additionally, the API documentation will show up in your IDE as you would expect.  GoRogue also has a subreddit at [r/GoRogueLib](https://www.reddit.com/r/GoRogueLib/) and a [discord server](https://discord.gg/fxj5kPq).

## Feature List
**Convenient Primitive Types:** GoRogue is based on the `SadRogue.Primitives` library, which provides comprehensive, easy-to-use, and flexible primitive types for coordinates, rectangles, grids, and more.  As well, `SadRogue.Primitives` provides integration packages for other common libraries that define those types (MonoGame, SFML, etc) that allow easy integration with those library's equivalent types.  It also provides functionality to easily operate on and work with grids, which includes operations such as determining locations in a radius, moving a position around on a grid, and calculating distance.

**Unobtrusive Algorithms:** GoRogue algorithms are based upon a simple abstraction for input/output data, so that GoRogue can easily integrate into many different existing systems/libraries without requiring duplication of data or merging of data structures.

**Flexible Component System:** GoRogue implements a performant, type-safe component "container" structure that allows you to easily attach instances of arbitrary classes to objects as components, and retrieve them by type or tag.

**Dice Notation Parser:** GoRogue implements a dice notation parser that can take complex dice expresssions, simulate them using RNGs, and return the results.

**Factory Framework:** GoRogue provides an object-oriented framework for implementing the [factory method pattern](https://en.wikipedia.org/wiki/Factory_method_pattern) by defining "blueprints" and calling upon them to instantiate an object by their name.

**Concrete System for Map/Object Representation:** In addition to providing generic core algorithms, GoRogue also provides a concrete "GameFramework" system that provides a ready-to-use way to represent a map and objects on it, and integrates many core GoRogue features such as FOV, Pathing, and the component system such that they work out of the box.

**Versatile Map Generation Framework:** GoRogue provides quick ways to get started with map generation that allow you to generate maps in common ways.  It includes each step as a distinct "step" that is usable in custom map generation as well.  It also provides a class framework for map generation that makes it easy to design debuggable, distinct custom map generation steps and apply them in sequence to generate a final map.

**Message Bus System:** GoRogue provides a simple, type-safe system for creating a "message-bus" architecture wherein messages can be sent across a message bus, and various systems can subscribe and respond to messages of the appropriate type.

**Pathfinding:** GoRogue provides a number of pathfinding algorithms.  These include an extremely performant A* pathfinding algorithm, and an implementation of "goal maps", also known as [dijkstra maps](http://www.roguebasin.com/index.php?title=The_Incredible_Power_of_Dijkstra_Maps).  GoRogue also provides a "flee map" implementation.

**Random Number Generation:** GoRogue builds off of the [Troschuetz.Random](https://gitlab.com/pomma89/troschuetz-random) library, which provides generators that use various methods for conventiently generating random numbers and sequences, as well as the capability to serialize those generator.  GoRogue adds some facilities for easily providing a custom generator to use for GoRogue functions that require one, as well as some custom number generators that may be useful in debugging/unit testing.

**Field-of-View:** GoRogue provides a versatile implementation of [recursive shadowcasting](http://www.roguebasin.com/index.php?title=FOV_using_recursive_shadowcasting) for calculating field-of-view that supports both distance-restricted and angle-restricted cones.

**Sense Maps:** In addition to its field-of-view algorithms, GoRogue provides a framework for primitively modeling the spread of sound, light, heat, etc. using various algorithms.

**Useful Data Structures:** GoRogue provides a number of data structures that are commonly created during roguelike development.  These include "spatial maps", which are an efficient way to store and layer objects positioned on a grid, and a [disjoint set](https://en.wikipedia.org/wiki/Disjoint-set_data_structure) structure that implements path compression.

**Effects System:**  GoRogue provides a robust, type-safe effects system that can be used to model "effects", with or without a duration, in a turn-based game system.  It provides methods of dealing with effects with duration (in arbitrary units), instant effects, and infinite effects.  It also provides a structure to group and automatically trigger effects that also supports effect cancellation.

**Line Algorithms:**  GoRogue currently provides implementations of [Bresenham's](https://en.wikipedia.org/wiki/Bresenham%27s_line_algorithm) line algorithm, as well as a few other algorithms for determining lines on a grid.  These will be moved to `TheSadRogue.Primitives` before GoRogue 3.0 is out of alpha.

**Math Utility:** GoRogue adds to the mathematical utility functions provided by `TheSadRogue.Primitives` with a number of useful methods to wrap numbers around array indices, round to the nearest multiple of a number, and approximate certain trigonometric functions.  These may be moved to `TheSadRogue.Primitives` before GoRogue 3.0 is out of alpha.

**Utility Functions:** GoRogue adds many miscellaneous utility functions as extension methods to various classes.  These include methods to select random items/indices out of lists with GoRogue's random number generation framework, methods to create a string representing the elements of lists and other enumerable objects sensibly, methods to shuffle the items in a list, and more!

**Serialization (WIP):** Support for serialization in GoRogue 3 is still a work-in-progress.  Currently, GoRogue plans to provide serialization via the [data-contract serialization](https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.datacontractserializer?view=net-5.0) facilities built into C#.  A `GoRogue.JSON` package is also planned that will allow the serialization facilities to function cleanly with [Newtonsoft.JSON](https://www.newtonsoft.com/json).

## Licensing
### GoRogue
This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details.

### Other Licenses
Licenses for other projects which GoRogue depends on or from which inspiration was taken are listed in the credits section.

## Credits
GoRogue depends on some other .NET Standard libraries for some of its functionality, and additionally takes some inspiration from other great roguelike/2D game related projects.  Those projects and their licenses are listed below.

### TheSadRogue.Primitives
This library provides the foundational classes and algorithms for working with 2D grids that GoRogue uses in its core algorithms.  Many of its features were originally part of GoRogue v2.  This project is also licensed under MIT, and is maintained by myself and Thraka (creator of SadConsole):
- [TheSadRogue.Primitives](https://github.com/thesadrogue/TheSadRogue.Primitives)
- [TheSadRogue.Primitives License](https://github.com/thesadrogue/TheSadRogue.Primitives/blob/master/LICENSE)

### Troschuetz.Random
GoRogue depends on this library for the foundation of its RNG functionality.  This project is also licensed under MIT:
- [Troschuetz.Random](https://gitlab.com/pomma89/troschuetz-random/-/tree/master)
- [Troschuetz.Random License](https://gitlab.com/pomma89/troschuetz-random/-/blob/master/LICENSE)

### Optimized Priority Queue
GoRogue depends on this library to provide the queue it uses in its pathfinding algorithms.  This project is also licensed under MIT:
- [OptimizedPriorityQueue](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp)
- [OptimizedPriorityQueue License](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/blob/master/LICENSE.txt)

### SquidLib
This Java roguelike library is another big inspiration for much of the functionality of GoRogue.  A similar RIPPLE algorithm is used in `SenseMap`, and the concept of "spatial map" was originally taken from SquidLib's implementations.  No source code from SquidLib is directly used, and no project in GoRogue depends on SquidLib or uses SquidLib binaries.
- [SquidLib](https://github.com/SquidPony/SquidLib)
- [SquidLib License](https://github.com/SquidPony/SquidLib/blob/master/LICENSE.txt)

### Dice Notation .NET
General inspiration for the functionality of the `GoRogue.DiceNotation` namespace was taken from the Dice Notatation .NET library.  This project is also licensed under MIT:
- [Dice Notation .NET](https://github.com/eropple/DiceNotation)
- [Dice Notation .NET License](https://github.com/eropple/DiceNotation/blob/develop/LICENSE.txt)

### RogueSharp
General inspiration for some algorithms available in the `GoRogue.MapGeneration` namespace were taken from the C# library RogueSharp.  This project is also licensed under MIT:
- [RogueSharp](https://bitbucket.org/FaronBracy/roguesharp)
- [RogueSharp License](https://bitbucket.org/FaronBracy/roguesharp/src/master/LICENSE.txt?at=master)
### Doryen Library (libtcod)
This classic roguelike toolkit library gave general inspiration for a number of GoRogue's features.
- [Libtcod](https://github.com/libtcod/libtcod)
- [Libtcod License](https://github.com/libtcod/libtcod/blob/develop/LICENSE.txt)

