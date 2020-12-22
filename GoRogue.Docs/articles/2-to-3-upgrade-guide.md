---
title: 2.x to 3.0 Upgrade Guide
---

# 2.x To 3.0 Upgrade Guide
This article is intended to be a useful resource for those that are familiar with GoRogue 2.x, and are looking to upgrade to 3.0, by summarizing/discussing the relevant changes.  It is not an all-inclusive list of new features, however discusses particularly breaking changes and some useful new features that result from them.

# Upgraded to .NET Standard 2.1
One of the biggest changes in GoRogue 3 is that it has moved to support a minimum .NET Standard version of .NET Standard 2.1, for reasons pertaining to run-time performance and C# language feature support.  This affects the minimum versions of various runtimes required to use GoRogue as described in [this Microsoft table](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

One of the biggest takeaways from this change is that support for .NET Framework has been removed entirely, as .NET Framework does not support .NET Standard 2.1 and there are no future plans for it to do so.  Microsoft has [officially pivoted away](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/) from adding new features to .NET Framework, and has committed to providing equivalent platform and feature support to their new .NET unified platform (previously .NET Core).  GoRogue 3 does support .NET 5+ and .NET Core 3.0+.

For those of you that still have .NET Framework projects, you may be surprised at how easy it can be to upgrade; so this may not be a big issue.  Some users, however (Unity users, for example), may have toolkit limitations that prevent an upgrade to a platform that supports .NET Standard 2.1 at the current time.  Virtually all .NET runtimes, including Mono and the Unity compiler, have either already upgraded to support this version, or have stated that they have definitive plans to do so.  Many game engines, including Unity, Godot, and Stride, have also either already upgraded or have stated that they will upgrade to a version of their runtime that will support it.

In either case, I intend to provide maintenance updates to GoRogue 2.x (to include primarily bugfixes) until such time as these upgrades are complete.

## Support for C# 8 Nullable Reference Types
A benefit of the shift to .NET Standard 2.1 is that it has enabled GoRogue 3 to be annotated to support [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references), which were introduced in C# 8.  GoRogue's entire API now has annotations indicating proper nullability.

This will not break any code that chooses not to enable the nullable reference types feature; such code will simply receive no benefit.  For projects that do enable it, however, it provides an extremely useful extra layer of compile-time safety.

# Core Features Moved to TheSadRogue.Primitives
Another significant change is that a number of features that were in GoRogue 2 have been moved to a new library called [TheSadRogue.Primitives](https://github.com/thesadrogue/TheSadRogue.Primitives).  These types include:
- `Coord` (now called `Point`)
- `Direction`
- `AdjacencyRule`, `Distance`, and `Radius`
- `RadiusAreaProvider` (moved to `Radius` functions)
- Everything in the `GoRogue.MapViews` namespace (renamed to `GridViews`)
- `MapArea` (now called `Area`) and `IReadOnlyMapArea` (now called `IReadOnlyArea`)
- `BoundedRectangle`
- `Rectangle`
- Most of the `MathHelpers` static class

`TheSadRogue.Primitives` is open source, and maintained by [Thraka](https://github.com/Thraka) and I.  It is compatible with all platforms that GoRogue is.  Additionally, GoRogue depends on this library, so it will be automatically installed via NuGet when GoRogue is.  Many changes will simply involve changing namespaces referred to from the GoRogue namespaces items were in, to `SadRogue.Primitives`.

The primitives library also has some basic classes that weren't in GoRogue 2, such as `Color`, `Gradient`, `Palette`, and `PolarCoordinate`.  These may be useful to replace equivalent types, and as well provide new distinct features.

The primitives library also has extension packages that allow its types to easily convert to the equivalent types in MonoGame, SFML, and some others; none of these extensions are required to use GoRogue but may be useful if you are using those libraries with GoRogue.

[SadConsole](https://github.com/Thraka/SadConsole), Thraka's console-emulation library, also uses `TheSadRogue.Primitives` for its types as of v9, so those of you using GoRogue with SadConsole will enjoy benefits there as well.

## Implicit Conversions to Equivalent MonoGame Types Removed
In GoRogue 2, types such as `Coord` and `Rectangle` had implicit conversions defined to their equivalent types in other libraries such as MonoGame, so that if you were using those libraries it was easy to convert between the two.  These conversions were included in such a way as to not create a hard dependency; unfortunately, however, some compilers (such as Unity's) did not parse the tags properly and considered these libraries hard dependencies rega.  Thus, "MinimalDependency" versions were created and released for users that ran into these scenarios, which made getting started more complex.

In GoRogue 3, the implicit conversions have been removed.  The types that had such conversions were moved to [TheSadRogue.Primitives](https://github.com/thesadrogue/TheSadRogue.Primitives), and the conversions themselves have been replaced by the extension packages for that library.  These extension packages define similar (explicit) conversion functions.  See that library's documentation for details.

# Value Tuples Removed from Public Interface
GoRogue 2 used value tuples introduced in C# 7 in some of its API; for example, the return type of the `MapGeneration.Connectors.RoomDoorDonnector.ConnectRooms` function was a series of value tuples.  Value _types_ are very good for performance when used correctly, and value _tuples_ make a convenient way of creating a basic value type.  However, it can make serialization more challenging, and is nowhere near as convenient to work with in other .NET languages.  Therefore, GoRogue elects not to use value tuples in its public API, although it makes extensive use of value _types_.

Instead of returning or taking as input a value tuple, GoRogue algorithms take a class whose name ends in "Pair"; for example, `ItemStepPair` or `AreaConnectionPointPair`.  These classes are implicitly convertible to and from a tuple of the two items, and implement support for [deconstruction syntax](https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct#deconstructing-tuple-elements-with-discards).  Therefore, they provide all the benefits of value tuples for C# users, and in fact can be used as if they _are_ value tuples in most cases; but they also provide the benefits of serialization control and strongly-named values for non C# users.

# Mutable Types No Longer Implement Equals/GetHashCode
In GoRogue 2, a number of mutable reference types implemented `Equals`, `operator==`, and `GetHashCode`, such as `MapArea`.  However, according to [Microsoft guidance](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals?view=net-5.0), types that are mutable should not implement `Equals`, due to semantics of the requirement to also implement `GetHashCode`.  Since `Equals`, `operator==` and `GetHashCode` are supposed to be implemented in tandem for things to work correctly, such classes should ideally not implement any of those functions.

Therefore, `Equals`, `operator==`, and `GetHashCode` implementations for any mutable types in GoRogue (including types in `TheSadRogue.Primitives` library) have been removed.  Instead, mutable types implement a new interface `IMatchable`.  It provides a function `Matches`, whose signature is identical to `IEquatable.Equals` (except for the difference in names).  This function can be used to perform the comparisons previously performed by the `Equals` function implementations.  Effectively, any calls on such types that in GoRogue 2 used the `==` operator or `.Equals` for a custom comparison can now use `.Matches`.

All value types in GoRogue 3 are immutable, and immutable value types such as `Point` and `Rectangle` do implement `Equals`, `GetHashCode`, and `IEquatable` as they previously did.  However, they also implement `IMatchable` in a way equivalent to their `Equals` function, for consistency.

# Naming Conventions for Static Variables and Enums Changed
Names of various enumerations and static variables have been changed, to bring them more in accordance with recommended C# naming conventions.  Effectively, this just results in ALL_CAPS style names being removed, and replaced with UpperFirstLetter style names; eg. `SourceType.RIPPLE` has been changed to `SourceType.Ripple`, and `Radius.SQUARE` has been changed to `Radius.Square`.

# MapView Changes (Now Called GridViews)
MapViews have undergone a refactor in GoRogue 3 as well.

## Views Renamed and Moved
First, the term "map view" has been replaced by "grid view", and all of the classes have been renamed accordingly:

| Old Name | New Name |
| -------- | -------- |
| `IMapView<T>` | `IGridView<T>` |
| `ISettableMapView<T>` | `ISettableGridView<T>` |
| `ArrayMap<T>` | `ArrayView<T>` |
| `ArrayMap2D<T>` | `ArrayView2D<T>` |
| `LambdaMapView<T>` | `LambdaGridView<T>` |
| `LambdaSettableMapView<T>` | `LambdaSettableGridView<T>` |
| `LambdaTranslationMapView<T>` | `LambdaTranslationGridView<T>` |
| `LambdaSettableTranslationMapView<T>` | `LambdaSettableTranslationGridView<T>` |
| `TranslationMapView<T>` | `TranslationGridView<T>` |
| `SettableTranslationMapView<T>` | `SettableTranslationGridView<T>` |

Additionally, all of these classes have been moved to [TheSadRogue.Primitives](https://github.com/thesadrogue/TheSadRogue.Primitives) library, and reside under the namespace `SadRogue.Primitives.GridViews`.

## New GridView Base Class
There is also a new base class that will make many custom `IGridView` implementations simpler.  In GoRogue 2, `IMapView` implementations often contained repetititive code that implemented one or more indexers in terms of the others.  To help alleviate this, `TheSadRogue.Primitives` library that now contains grid views has the abstract classes `GridViewBase` and `SettableGridViewBase`.

These classes may optionally be inherited from as an alternative to `IGridView` and `ISettableGridView`, respectively, and implement the repetitive code to define the location indexers in terms of a single, abstract indexer taking `Point`.  For cases where there is nothing preventing your custom implementations from inheriting from a base class, this is much more convenient, as you need only implement the basic properties, and a single indexer; the other indexers are implemented based on that one.

## New IGridView Properties
`IGridView` and `ISettableGridView` now require implementations of a `Count` property, which should be equal to `Width * Height` (the number of tiles in the view).  This property allows `IGridView` to support [indices](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#indices-and-ranges) as introduced in C# 8.  The `GridViewBase` and `SettableGridViewBase` implement this property automatically.

## New/Refactored Helper Functions
A number of useful extension methods for `ISettableGridView` implementations have also been added/refactored.  First, the `ArrayMap.SetToDefault` function was changed to `ArrayView.Clear()` to match more typical naming conventions for arrays.  Additionally, an `ApplyOverlay` overload has been added as an extension for `ISettableGridView` that takes a `Func<Point, T>` to use for determine overlay values, to avoid the need to use a `LambdaGridView` in these cases.  Finally, a `Fill` extension method has been added that sets each locatin in an `ISettableGridView` to a specified value.

## New IGridView Implementation
Additionally, `TheSadRogue.Primitives` contains one additional concrete implementation of `ISettableGridView` that was not included in GoRogue 2; the `DiffAwareGridView`.  This grid view may be useful for situations where you are using a grid view or array of value types, and want to interact with or display incremental (groups of) changes to that grid view/array.  See the class documentation for details.

# Map Generation Rewritten
The map generation system has been completely rewritten from the ground up in GoRogue 3.  It functions in a notably different manner, so it is recommended that you review [this article](~/articles/map-generation.md) which explains the new system's concepts and components.

## GoRogue 2 Equivalents
The basic map generation algorithms that were present in GoRogue 2 are all present in some form in GoRogue 3, but not at a level that produces full implementation parity.  Therefore, the new implementation of those algorthms is not guaranteed to produce the same level from a given seed that was produced in GoRogue 2.

### Quick Generators
The "full-map generation" algorithms that were present in GoRogue `GoRogue.MapGeneration.QuickGenerators` are represented in GoRogue 3 as functions in `GoRogue.MapGeneration.DefaultAlgorithms`.  These functions produce a set of generation steps that, when executed in a `Generator`, follow a roughly equivalent process:

| Old QuickGenerator | DefaultAlgorithm |
| ------------------ | --------------- |
| `QuickGenerators.GenerateCellularAutomataMap` | `DefaultAlgorithms.CellularAutomataGenerationSteps` |
| `QuickGenerators.GenerateDungeonMazeMap` | `DefaultAlgorithms.DungeonMazeMapSteps` |
| `QuickGenerators.GenerateRandomRoomsMap` | `DefaultAlgorithms.BasicRandomRoomsMapSteps` |
| `QuickGenerators.GenerateRectangleMap` | `DefaultAlgorithms.RectangleMapSteps` |

### Connection Algorithms
The "connection" algorithms provided in GoRogue 2 for connecting areas/rooms are provided as generation steps in GoRogue 3.  The following shows a rough mapping:

| Old Algorithm | New Step |
| ------------- | -------- |
| `MapGeneration.Connectors.ClosestMapAreaConnector` | `MapGeneration.Steps.ClosestMapAreaConnection` |
| `MapGeneration.Connectors.DeadEndTrimmer` | `MapGeneration.Steps.TunnelDeadEndTrimming` |
| `MapGeneration.Connectors.OrderedMapAreaConnector` | `MapGeneration.Steps.OrderedMapAreaConnection` |
| `MapGeneration.Connectors.RoomDoorConnector` | `MapGeneration.Steps.RoomDoorConnection` |

The configuration options are very similar functionally, however since these are now generation steps they're specified as public members of the class.

### Tunnel Creation Algorithms
The tunnel creation algorithms provided in GoRogue 2 are largely intact in GoRogue 3, and have just moved to different namespaces.  The API has changed a bit, but functionally they produce the same result.  The `ITunnelCreator`, `DirectLineTunnelCreator`, and `HorizontalVerticalTunnelCreator` classes are now located in the `MapGeneration.TunnelCreators` namespace.  The API has been modified so that the tunnel creators return an `Area` representing the tunnel they create; but otherwise the algorithms function in much the same way.  They are used as optional paramters in the appropriate map generation steps just like they were in the GoRogue 2 algorithms.

### Connection Point Selection Algorithms
The algorithms for selecting area connection points, much like the tunnel creation algorithms, are largely intact but have moved to different namespaces.  The API has some types that are changed, but generally the algorithms function the same way.  The `IAreaConnectionPointSelector` has been renamed to `IConnectionPointSelector`, and it along with the `CenterBoundsConnectionPointSelector`, `ClosestConnectionPointSelector`, and `RandomConnectionPointSelector` classes are now located in the `MapGeneration.ConnectionPointSelectors` namespace.  The API has been modified so that the `SelectConnectionPoint` function returns a value type `AreaConnectionPointPair` with the two selected points; that is implicitly convertible to a value tuple as detailed in [this section](#value-tuples-removed-from-public-interface).

### Other Room/World Generation Items
Most other GoRogue 2 map generation structures/algorithms exist in some form as generation steps in the `MapGeneration` or `MapGeneration.Steps` namespaces.  One notable exception is `MapArea`, which was renamed to `Area` and moved to `TheSadRogue.Primitives`.  The following is a rough mapping:

| Old Algorithm | New Equivalent |
| ------------- | -------------- |
| `MapGeneration.MapAreaFinder` | Unchanged **\*** |
| `MapGeneration.MapArea` | `TheSadRogue.Primitives.Area` |
| `MapGeneration.BasicRoomsGenerator` | `MapGeneration.Steps.RoomsGeneration` **\*\*** |
| `MapGeneration.CellularAutomataAreaGenerator` | Multiple Steps **\*\*\*** |
| `MapGeneration.MazeGenerator` | `MapGeneration.Steps.MazeGeneration` |
| `MapGeneration.RoomsGenerator` | `MapGeneration.Steps.RoomsGeneration` |

**\*** An actual map generation step that wraps this functionality is available in `MapGeneration.Steps.AreaFinder`\
**\*\*** Algorithm was similar to `MapGeneration.RoomsGenerator` in GoRogue 2; the implementations are similar enough that they can reasonably be substituted for each other\
**\*\*\*** `MapGeneration.Steps.RandomViewFill` should be performed first to randomly fill walls/floors, then `MapGeneration.Steps.CellularAutomataAreaGenerator` to smooth

## New Generation-Related Algorithms
A number of new generation steps/algorithms that have no GoRogue 2 equivalent are also provided in the `MapGeneration` namespace.

### Translation Steps
The article on map generation refers to "translation steps" as steps that purely transform existing data in the map generation context to a new form, as opposed to generating new, unique data.  It may definitely prove useful to review these steps, located in the `MapGeneration.Steps.Translation` namespace, as they may be useful in general cases.

### Finding Doors
GoRogue now provides a generation step for finding where to place doorways, given wall-floor tile states and a series of rectangular rooms.  The algorithm to do this is relatively simple, but it was requested a number of times as a feature in the past, and is much easier to implement generically in the new system.  See [DoorFinder class documentation](xref:GoRogue.MapGeneration.Steps.DoorFinder) for details.

## New Structures
There are also a number of new data structures that may be useful in map generation.  One is the `Region` class, which is covered in [its own section](#region-class).  Others are in the `MapGeneration.ContextComponents` namespace, or in the root `MapGeneration` namespace.  The API documentation explains the purpose of these classes in detail.

# Component System Changes
The functionality of GoRogue's component system has been expanded in GoRogue 3, and has been moved to its own namespace.  Additionally, some functions have changed names to make them more consistent with traditional collection names.

## Classes and Function Names Refactored
All component-related classes have been moved to the `GoRogue.Components` namespace.  Additionally, classes have been renamed as follows:

| Old Name | New Name |
| -------- | -------- |
| `ComponentContainer` | `Components.ComponentCollection` |
| `IHasComponents` | `Components.IBasicComponentCollection` |
| `ISortedComponent` | `Components.ISortedComponent` |

The names of functions on the component collection class/interface have also been modified to bring them more in-line with traditional C# collection names.  A mapping of equivalent functions is as follows:

| Old Name | New Name |
| -------- | -------- |
| `AddComponent` | `Add` |
| `RemoveComponent` | `Remove` |
| `RemoveComponents` | `Remove` |
| `HasComponent` | `Contains` |
| `HasComponents` | `Contains` |
| `GetComponent` | `GetFirstOrDefault` |
| `GetComponents` | `GetAll` |

Note that `GetFirstOrDefault` most closely replicates the behavior of GoRogue 2's `GetComponent`; however, there is an additional function called `GetFirst` that is now provided.  This function throws an exception if a component of the specified type is not found, and may be useful to replace cases where you're checking if the return value of `GetComponent` is null and erroring in that case.

## Events Added to ComponentContainer
Another change is that in GoRogue 3, the `Add` and `Remove` functions on `ComponentCollection` are no longer virtual.  Instead, `ComponentAdded` and `ComponentRemoved` events have been added to `ComponentCollection`, and these can be used to respond to the addition/removal of components.

## Added Support for Tags on Components
Another change to the component system is that `ComponentCollection` now supports associating a string "tag" with a component when it is added.  When components are removed or retrieved, a tag can be specified in addition to the type parameter, and any component returned must have the tag specified as well as be of the type given.  This can be useful if you need to manage multiple components that are of a single type or that share some inheritance or interface chain.

Similarly, the additional functions that this feature results in are defined in a separate interface, `Components.ITaggableComponentCollection`.  It is quite unlikely that a custom implementation of this interface would be necessary, but it is designed and used as such regardless.

# Factory System Changes
The factory system in GoRogue 3 has been refactored to address some naming/usability concerns.  First, note that the `GoRogue.Factory` namespace is now `GoRogue.Factories`, which more accurately matches naming conventions elsewhere in the library.

The remaining factory changes fall in line with splitting out the class structures for the two main use cases of factories; to create items when additional configuration information per instance _is_ needed, and to create items when such information _is not_ needed.  In GoRogue 2, both use cases were supported but the class structure for supporting this was somewhat unintuitive; it utilized an arbitrary class `BlueprintConfig` for a base configuration object, that served no purpose other than to provide a default value.  In GoRogue 3, the factory classes have been split out as follows:
- `Factory<TProduced>`
    - Produces objects of type `TProduced` via `IFactoryBlueprint` objects that DO NOT take a configuration object in the `Create` function.
    - Useful for cases that would previously have involved `SimpleBlueprint`
- `AdvancedFactory<TBlueprintConfig, TProduced>`
    - Produces objects of type `TProduced` via `IAdvancedFactoryBlueprint` objects that DO accept a configuration object of type `TBlueprintConfig` in the `Create` function.
    - Useful for cases that would have previously involved passing a configuration object of some type that subclassed `BlueprintConfig`
    - In GoRogue 3, there is NO required base class for configuration objects; it can be an arbitrary type

`IFactoryObject` will function the same way as it did, whether objects that implement it are created in a `Factory` or `AdvancedFactory`.  As well, a new `ItemNotDefinedException` with a useful message is thrown when their `Create` or `GetBlueprint` methods are called with a blueprint name that does not exist.

# Spatial Map Changes
A number of changes have been made to the interface for spatial maps in GoRogue 3.  First, all spatial map implementions and related interfaces have been moved to the `GoRogue.SpatialMaps` namespace, in order to clean up the root namespace.  There are also a number of API changes.

In GoRogue 2, functions for spatial maps like `Add` and `Move` simply returned `false` if an operation failed.  This design had some useful properties, but ultimately turned out to be a poor design decision, which didn't fit well with the rest of the library and was the source of many known bugs/desyncs that users, and I, accidentally created in code. Therefore, the `ISpatialMap` interface and all implementing classes have been modified such that, if their functions fail, they throw an `ArgumentException` with an error message that tells you exactly what went wrong.  Functions that work in this way now include `Add`, `Move`, `MoveAll`, and `Remove`.

This change makes it much more obvious to the user when something unexpected happens.  For situations where you _do_ need to check whether an operation is possible before completing it, the interface has been augmented to include functions that simply check if an operation is possible without doing it, and return the appropriate boolean value.  These functions include `CanAdd`, `CanMove`, and `CanMoveAll`.

Another associated change to spatial maps is the separation of the functionality for moving _all_ items at a location vs. moving only the ones that _can_ move.  In addition to `Move(T item)`, `ISpatialMap` now also contains `MoveAll` and `MoveValid` functions.  `MoveAll` attempts to move all items at a given location to the target location, throwing an `ArgumentException` if this fails.  `MoveValid` only moves the items that can move, returning precisely which items were moved.

# GameFramework Namespace Refactored
A number of refactors have been performed on the `GoRogue.GameFramework` namespace.

## Game Object Changes
`IGameObject` and the way it is used has undergone a notable refactor to address some usability issues present in GoRogue 2.

### Simplification of IGameObject Implementation
The class structure for `GameFramework.GameObject` has been simplified in GoRogue 3.  In GoRogue 2, support for use cases where you were unable to inherit from `GameObject` was somewhat complex.  You had to implement `IGameObject` in these cases; but, since implementation of the functionality defined by `IGameObject` was closely coupled with the code for `GameFramework.Map`, this was non-trivial.  The recommended approach for this in GoRogue 2 was to use a `GameObject` instance as a private backing field for your `IGameObject` implementation; and in fact GoRogue 2 had special support for this built into `GameObject` in the form of the `parent` parameter to the constructor.

However, this approach proved to be somewhat error prone, and very easily led to non-intuitive/difficult to debug behavior in error cases.  Further, it added complexity to code that needed to implement `IGameObject` instead of inheriting from `GameObject`.  So, in GoRogue 3, the `parent` parameter in the `GameObject` constructor has been removed, and it is no longer recommended (and in many cases no longer possible) to implement `IGameObject` via a backing field of type `GameObject`.

Instead, a focus was placed on decoupling the code required to implement `IGameObject` from the internal code in `Map`, which ultimately resulted in `IGameObject` being much easier to implement the more traditional way.  Helper functions have been provided that make implementing most functions in the `IGameObject` interface a one-line endeavor.  As such, if you need to implement `IGameObject` yourself, you should be able to more or less copy-paste the code from `GameObject`, and use it for the implementation, without making your code unintuitive or unnecessarily long.

### Removed IsStatic
In GoRogue 2, game objects had an `IsStatic` flag, that could be set via a constructor parameter to indicate that they could not move.  Objects on the terrain layer of a map (layer 0) were _required_ to have `IsStatic` set, to allow for some optimization by allowing them to reside on a grid view as opposed to a spatial map.  This was useful for performance, but inconvenient at times for users; particularly if a user wanted to use something like GoRogue's factory system for creation of terrain.  The position could only be set at construction due to the `IsStatic` flag being set, which meant that the position had to be known exactly when the instance was created.  Furthermore, the restriction of terrain objects being unable to move at all is actually more stringent than required; the only required portion for optimization is that they not move _while they are part of a map_.

Therefore, in GoRogue 3 the `IsStatic` flag has been removed.  Instead, if an object is on layer 0 of a map and it is moved, an exception will be thrown by the `Map`. Objects on layer 0 that are not added to a `Map` can be moved freely.  This allows the optimization to stay, but makes creation of objects more convenient for a user.

### Components Now Attached to a Property
In GoRogue 2, `IGameObject` implemented `IHasComponents` to allow components to be placed on game objects.  In GoRogue 3, this is no longer the case; instead, `IGameObject` defines a property `GoRogueComponents`, which is a collection that can have components added/removed.  This class also integrates the new "tag" functionality for tagging components.  The names of the functions for adding, removing, and retrieving components have also changed in a manner corresponding to the [component collection name changes](#component-system-changes).

The `GoRogueComponents` field in `GameObject` defaults to being of type `Components.ComponentCollection`, and it is generally rare to need that field to be represented by some other type.  Nonetheless, the field is of type `Components.ITaggableComponentCollection`, and an instance of some custom implementation of this interface may be passed to `GameObject` via its constructor.  This ensures that the component collection structure can be customized if it becomes necessary.

### Setting Position or Walkability may Throw Exception
In support of the [spatial map changes](#spatial-map-changes) in GoRogue 3, setting the `Position` or `IsWalkable` properties of a game object will throw an exception if they are set to a value that would violate map collision rules.  See the corresponding [Map changes section](#addingremovingmoving-objects-may-throw-exception) for details.

### Optional Base Class for Components
GoRogue 2 contained the `GameFramework.Components.IGameObjectComponent` interface, which is an optional interface that you could implement on components attached to a `GameObject`.  When implemented, the object would automatically have its `Parent` property value updated to reflect the `IGameObject` that it was attached to.  This still exists in GoRogue 3, despite the fact that components are now added to `IGameObject.GoRogueComponents` instead of the object directly.  If you attach something that implements `IGameObjectComponent` to a `GameObject` by calling its `GoRogueComponents.Add` function, the `Parent` property will automatically be updated to reflect the `IGameObject` that it was attached to.  Similarly, that property is also updated when a component is detached.

In addition to this behavior, there is now a base _class_ that you can optionally inherit from when implementing components that will be attached to `GameObjects`.  This class is `GameFramework.Components.ComponentBase`.  It implements `IGameObjectComponent`, and also adds some useful functionality in the form of events that are automatically fired when the component is attached/detached from an object.  It also uses these events to provide some additional functionality.  For example, the `ComponentBase<T>` class uses the events to add a run-time requirement that the parent inherit from the given type `T`, and exposes the `Parent` as that type.  This is very convenient if you need to access methods/properties of that type beyond what `IGameObject` provides in the implementation of that component.

### ID Generation Changes
One additional change alters the method that you use to assign IDs to game objects in a custom way.  In GoRogue 2, to assign IDs in a custom manner, you had to subclass `GameObject` and override the `GameObject.GenerateID()` function.  Given that `GameObject` instances can often be used with no subclasses at all, the requirement to create a subclass for this functinality can be inconvenient.

Therefore, in GoRogue 3, the `GameObject.GenerateID()` function has been removed.  Instead, an optional parameter of type `Func<uint>` can be passed to the `GameObject` constructor.  If the parameter is passed, the given function is used to generate an ID.  If not, the default method of randomly generating an ID is used.  This still allows a custom method to be used, but does not require that a user create a subclass to do so.

## Map Functionality Changes
There were also a number of relevant changes to the `GameFramework.Map` API in GoRogue 3.

### FOV Changes
In GoRogue 2, the `Map` had `CalculateFOV` functions that were used to calculate FOV for the map.  This made it more difficult to separate FOV from the map in use cases that required it, and required users to implement inconvenient overrides of the `CalculateFOV` functions in order to react to FOV changes.  In GoRogue 3, a number of refactors have taken place to address this.

First, the `Map.FOV` property has been renamed to `Map.PlayerFOV`.  This more clearly reflects the intended purpose of the field in games using multiple FOV instances for different entities.  Similarly, the `Explored` field has been renamed to `PlayerExplored`.

Additionally, the `CalculateFOV` functions have been completely removed from `Map`.  Instead, the `PlayerFOV` field is of type `FOV` instead of `IReadOnlyFOV`.  This allows you to call the `PlayerFOV.Calculate` function directly, with the same parameters previously passed to `Map.CalculateFOV`.  The `FOV` class has also been augmented to include a `Recalculated` event that fires whenever the FOV is recalculated, which allows you to respond to the FOV changing.  Since the `PlayerFOV` field is still settable, these changes make it much easier to separate the FOV from the `Map` when necessary and makes responding to FOV changes more convenient.

### Components Allowed on Map
One other change is that `GameFramework.Map` now has a `GoRogueComponents` property.  This allows you to attach components to a `Map` just like you do with game objects.  This may be useful functionality for component-based architectures.

### RemoveTerrain Functionality Added
A `RemoveTerrain` function has been added that takes a terrain object and removes it from the map, in support of ensuring that `SetTerrain` can be annotated as taking non-null parameters.  There is also a `RemoveTerrainAt` function to allow you to remove whatever terrain is at a given position.

### Naming Conventions for Position-Based Functions Changed
Functions that operate based on a position have changed name slightly; these functions will have "At" appended to the end of the name.  For example, `Map.GetTerrain` is now `Map.GetTerrainAt`, `Map.GetEntity` is now `Map.GetEntityAt`, and so forth.

### Adding/Removing/Moving Objects May Throw Exception
In GoRogue 3, spatial maps have been changed to throw exceptions in cases where invalid operations are performed (see the [corresponding section](#spatial-map-changes)).  Because `GameFramework.Map` is partially based upon spatial maps, and it exposes a number of similar operations via its API, similar changes have been made to `Map` and `IGameObject`.

The `AddEntity`, `RemoveEntity`, `SetTerrain`, `RemoveTerrain` and `RemoveTerrainAt` functions all return `void` and throw `ArgumentException` if they are called with a value that doesn't meet the necessary criteria, with an exception message detailing exactly what the issue was.  Similarly, `Map` is configured in such a way that if an object's `Position` or `IsWalkable` properties are set to something that is invalid, then an `InvalidOperationException` with a message detailing the issue will be thrown.

There are also extension methods provided for `IGameObject` that return a boolean value indicating whether it is valid to set the object's `Position` or an `IsWalkable` property a certain way.  the `CanMove` function returns whether or not an object's position can be set; `CanMoveIn` is similar, except it takes a direction and returns whether or not the object can move in that direction.  Similarly, `CanSetWalkability` and `CanToggleWalkability` return information about whether or not `IsWalkable` can be set.

# RadiusAreaProvider Functionality Moved
The functionality of `RadiusAreaProvider` was moved to `TheSadRogue.Primitives`, and merged into the `Radius` class in the form of `Radius.PositionsInRadius` functions.

# Random Number Generation Extension Changes
One breaking change pertaining to RNG in GoRogue 3 is that the `Random.SingletonRandom` class was renamed to `Random.GlobalRandom`, to more accurately reflect its purpose.  The `DefaultRNG` parameter should otherwise be the same, and simply require updating the class name in any references.

Additionally, a `PercentageCheck` function has been added as an extension method of `IGenerator`.  This makes it much more convenient to use an RNG to perform a "percentage check" (a check with a specified percent chance to succeed) with any RNG, including `DefaultRNG`.

# Region Class
GoRogue 3 has introduded a new class in the `MapGeneration` namespace called `Region`, which can represent an area of the map that is initially defined by 4 points.  It provides a host of operations, which include rotating regions about their center, recording subregions of regions, and much, much more.  A more detailed article will be posted about regions in the future; but for now, feel free to look through the [API documentation](xref:GoRogue.MapGeneration.Region), which describes each function.
