# GoRogue
Welcome to the homepage for GoRogue, the .NET Standard roguelike/2D game utility library!  This library is compatible with both .NET Framework and .NET Core projects, and offers a number of features that may be useful in roguelike development, including features such as FOV, lighting, map generation utilities, and much more!  See feature list below (which will be added soon).

## Documentation
A tutorial-style demo of GoRogue features is on the roadmap.  Currently, the API documentation is hosted on GitHub pages [here](https://chris3606.github.io/GoRogue).  The same documentation can be found in the docs folder in the root of the repository.

## Roadmap
This library is still in development - there are a number of important features on the horizon! These include:
- Pathfinding (Coming Soon)
  - At least AStar pathing, as well as Dijkstra maps (commonly known as Goal Maps) will be provided soon!
- Statistics Library
  - Utility classes to assist in dealing with interdependent character/monster statistics.
- Demo Project/writeup
- Map generation improvements (Coming soon)
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
