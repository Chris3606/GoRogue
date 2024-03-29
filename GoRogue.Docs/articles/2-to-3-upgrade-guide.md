---
title: 2.x to 3.0 Upgrade Guide
---

# 2.x To 3.0 Upgrade Guide
This article is intended to be a useful resource for those that are familiar with GoRogue 2.x, and are looking to upgrade to 3.0, by summarizing/discussing the relevant changes.  It is not an all-inclusive list of new features, however it discusses particularly breaking changes and some useful new features that result from them.

# Upgraded to .NET Standard 2.1
One of the biggest changes in GoRogue 3 is that it has moved to support a minimum .NET Standard version of .NET Standard 2.1, for reasons pertaining to run-time performance and C# language feature support.  This affects the minimum versions of various runtimes required to use GoRogue as described in [this Microsoft-published table](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

One of the biggest takeaways from this change is that support for .NET Framework has been removed entirely, as .NET Framework does not support .NET Standard 2.1 and there are no future plans for it to do so.  Microsoft has [officially pivoted away](https://devblogs.microsoft.com/dotnet/announcing-net-5-0/) from adding new features to .NET Framework, and has committed to providing equivalent platform and feature support to their new .NET unified platform (previously .NET Core).  GoRogue 3 does support .NET 5+ and .NET Core 3.0+.

For those of you that still have .NET Framework projects, you may be surprised at how easy it can be to upgrade; so this may not be a big issue.  Some users, however, may have toolkit limitations that prevent an upgrade to a platform that supports .NET Standard 2.1 at the current time.  Virtually all .NET runtimes, including Mono and the Unity compiler, have either already upgraded to support this version, or have stated that they have definitive plans to do so.  Many game engines, including Unity, Godot, and Stride, have also either already upgraded or have stated that they will upgrade to a version of their runtime that will support it.

## Support for C# 8 Nullable Reference Types
A benefit of the shift to .NET Standard 2.1 is that it has enabled GoRogue 3 to be annotated to support [nullable reference types](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references), which were introduced in C# 8.  GoRogue's entire API now has annotations indicating proper nullability.

This will not break any code that chooses not to enable the nullable reference types feature; such code will simply receive no benefit.  For projects that do enable it, however, it provides an extremely useful extra layer of compile-time safety.

# Core Features Moved to TheSadRogue.Primitives
Another significant change is that a number of features which were in GoRogue 2 have been moved to a new library called [TheSadRogue.Primitives](https://github.com/thesadrogue/TheSadRogue.Primitives).  These types include:
- `Coord` (now called `Point`)
- `Direction`
- `AdjacencyRule`, `Distance`, and `Radius`
- The Lines class (with a slightly modified API)
- `RadiusAreaProvider` (moved to `Radius` functions)
- Everything in the `GoRogue.MapViews` namespace (renamed to `GridViews`)
- `MapArea` (now called `Area`) and `IReadOnlyMapArea` (now called `IReadOnlyArea`)
- `BoundedRectangle`
- `Rectangle`
- All spatial map related interfaces and implementations (moved to the `SadRogue.Primitives.SpatialMaps` namespace)
- `IHasID` and `IHasLayer` interfaces
- Most of the `MathHelpers` static class

`TheSadRogue.Primitives` is open source, and maintained by [Thraka](https://github.com/Thraka) and I.  It is compatible with all platforms that GoRogue is.  Additionally, GoRogue depends on this library, so it will be automatically installed via NuGet when GoRogue is.  Many changes will simply involve changing namespaces referred to from the GoRogue namespaces items were in, to `SadRogue.Primitives`.

The primitives library also has some basic classes that weren't in GoRogue 2, such as `Color`, `Gradient`, `Palette`, and `PolarCoordinate`.  These may be useful to replace equivalent types, and as well provide new distinct features.

The primitives library also has extension packages that allow its types to easily convert to the equivalent types in MonoGame, SFML, and some others; none of these extensions are required to use GoRogue, but they may be useful if you are using those libraries with GoRogue.

[SadConsole](https://github.com/Thraka/SadConsole), Thraka's console-emulation library, also uses `TheSadRogue.Primitives` for its types as of v9, so those of you using GoRogue with SadConsole will enjoy benefits there as well.

## Implicit Conversions to Equivalent MonoGame Types Removed
In GoRogue 2, types such as `Coord` and `Rectangle` had implicit conversions defined to their equivalent types in other libraries such as MonoGame, so that if you were using those libraries it was easy to convert between the two.  These conversions were included in such a way as to not create a dependency on other libraries; unfortunately, however, some compilers (such as Unity's) did not parse the tags properly and considered these libraries required dependencies regardless.  Thus, "MinimalDependency" versions were created and released for users that ran into these scenarios, which made getting started more complex.

In GoRogue 3, the implicit conversions have been removed.  The types that had such conversions were moved to [TheSadRogue.Primitives](https://github.com/thesadrogue/TheSadRogue.Primitives), and the conversions themselves have been replaced by the extension packages for that library.  These extension packages define similar (explicit) conversion functions.  See that library's documentation for details.

# Value Tuples Removed from Public Interface
GoRogue 2 used value tuples introduced in C# 7 in some of its API; for example, the return type of the `MapGeneration.Connectors.RoomDoorDonnector.ConnectRooms` function was a series of value tuples.  Value _types_ are very good for performance when used correctly, and value _tuples_ make a convenient way of creating a basic value type.  However, value tuples can make serialization more challenging, and are nowhere near as convenient to work with in other .NET languages.  Therefore, GoRogue elects not to use value tuples in its public API, although it makes extensive use of value _types_.

Instead of returning or taking as input a value tuple, GoRogue algorithms take a class whose name typically ends in "Pair"; for example, `ItemStepPair` or `AreaConnectionPointPair`.  These classes are implicitly convertible to and from a tuple of the two items, and implement support for [deconstruction syntax](https://docs.microsoft.com/en-us/dotnet/csharp/deconstruct#deconstructing-tuple-elements-with-discards).  Therefore, they provide all the benefits of value tuples for C# users, and in fact can be used as if they _are_ value tuples in most cases; but they also provide the benefits of serialization control and strongly-named values for non C# users.

# Mutable Types No Longer Implement Equals/GetHashCode
In GoRogue 2, a number of mutable reference types implemented `Equals`, `operator==`, and `GetHashCode`, such as `MapArea`.  However, according to [Microsoft guidance](https://docs.microsoft.com/en-us/dotnet/api/system.object.equals?view=net-6.0), types that are mutable should not implement `Equals`, due to semantics of the requirement to also implement `GetHashCode`.  Since `Equals`, `operator==` and `GetHashCode` are supposed to be implemented in tandem for things to work correctly, such classes should ideally not implement any of those functions.

Therefore, `Equals`, `operator==`, and `GetHashCode` implementations for any mutable types in GoRogue (including types in `TheSadRogue.Primitives` library) have been removed.  Instead, mutable types implement a new interface `IMatchable`.  It provides a function `Matches`, whose signature is identical to `IEquatable<T>.Equals` (except for the difference in names).  This function can be used to perform the comparisons previously performed by the `Equals` function implementations.  Effectively, any calls on such types that in GoRogue 2 used the `==` operator or `.Equals` for a custom comparison should now use `.Matches`.

With the exception of custom iterators, all value types in GoRogue 3 are immutable, and immutable value types such as `Point` and `Rectangle` still implement `Equals`, `GetHashCode`, and `IEquatable<T>` as they previously did.  However, they also implement `IMatchable` in a way equivalent to their `Equals` function, for consistency.

# Some Functions Return Custom Iterators Instead of IEnumerable<T>
In GoRogue 2, a number of classes and structures had functions that returned `IEnumerable<T>` in order to provide a list of items.  Many functions still do this in GoRogue 3, as the concept works particularly well for creating a flexible API; the results work with LINQ and can easily be converted to List, array, and other data structures when needed.  However, the use of `IEnumerable<T>` can have negative effects on performance in some cases.  This is primarily due to the fact that `IEnumerable` is an interface, and therefore requires boxing and/or state to function, which creates work for the GC.

Therefore, in GoRogue 3, some such functions instead use a concept very similar to `List<T>.Enumerator` to mitigate this.  Instead of returning an `IEnumerable`, they instead return a value type which is a custom iterator, whose name typically ends in `Enumerator`; for example, `RectanglePositionsEnumerator`.  The return types of these functions implement `IEnumerator<T>` and `IEnumerable<T>`, so they can be used in a foreach loop and even can be used with LINQ if needed; however the performance, particularly when used directly in a foreach loop, is notably better because the compiler no longer has to box a value type into an interface reference.  `Rectangle.Positions()` is one such function, for example:

[!code-csharp[](../../GoRogue.Snippets/UpgradeGuide2To3.cs#CustomEnumerators)]

# Naming Conventions for Static Variables and Enums Changed
Names of various enumerations and static variables have been changed, to bring them more in accordance with recommended C# naming conventions.  Effectively, this just results in ALL_CAPS style names being removed, and replaced with UpperFirstLetter style names; eg. `Radius.SQUARE` has been changed to `Radius.Square`.

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

## New GridView Base Classes
There is also a new base class that will make many custom `IGridView` implementations simpler.  In GoRogue 2, `IMapView` implementations often contained repetitive code that implemented one or more indexers in terms of the others.  To help alleviate this, `TheSadRogue.Primitives` library that now contains grid views has the abstract classes `GridViewBase`, `SettableGridViewBase`, `GridView1DIndexBase`, and `SettableGridView1DIndexBase`.

These classes may optionally be inherited from as an alternative to manually implementing `IGridView` and `ISettableGridView`, respectively, and implement the repetitive code to define the location indexers in terms of a single, abstract indexer taking `Point`.  In the cases of the classes with "1DIndex" in the name, all indexers are implemented in terms of the one which takes a 1D index; the others implement all indexers in terms of the one which takes a `Point`.  For cases where there is nothing preventing your custom implementations from inheriting from a base class, this is much more convenient, as you need only implement the basic properties, and a single indexer; the other indexers are implemented based on that one.

## New IGridView Properties
`IGridView` and `ISettableGridView` now require implementations of a `Count` property, which should be equal to `Width * Height` (the number of tiles in the view).  This property allows `IGridView` to support [indices](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#indices-and-ranges) as introduced in C# 8.  The base classes discussed above implement this property automatically.

## New/Refactored Helper Functions
A number of useful extension methods for `ISettableGridView` implementations have also been added/refactored.  First, the `ArrayMap.SetToDefault` function was changed to `ArrayView.Clear()` to match more typical naming conventions for arrays.  Additionally, an `ApplyOverlay` overload has been added as an extension for `ISettableGridView` that takes a `Func<Point, T>` to use for determine overlay values, to avoid the need to use a `LambdaGridView` in these cases.  Finally, a `Fill` extension method has been added that sets each location in an `ISettableGridView` to a specified value.

## New IGridView Implementations
Additionally, `TheSadRogue.Primitives` contains a number of additional concrete implementation of `ISettableGridView` that were not included in GoRogue 2.  The first is `DiffAwareGridView`.  This grid view may be useful for situations where you are using a grid view or array of value types, and want to interact with or display incremental (groups of) changes to that grid view/array.  See the class documentation for details.

The other is `BitArrayView`, which is an `ISettableGridView<bool>` implementation which is based on C#'s `BitArray`.  This should be used instead of `ArrayView<bool>` in nearly all cases (unless your data is already an array and you're just creating a grid view wrapper), because its memory usage will be up to 8x less with a negligible difference in the performance of its operations.

# Map Generation Rewritten
The map generation system has been completely rewritten from the ground up in GoRogue 3.  It functions in a notably different manner, so it is recommended that you review [this article](~/articles/howtos/map-generation.md) which explains the new system's concepts and components.

## GoRogue 2 Equivalents
The basic map generation algorithms that were present in GoRogue 2 are all present in some form in GoRogue 3, but not at a level that produces full implementation parity.  Therefore, the new implementations of those algorithms are not guaranteed to produce the same level from a given seed that was produced in GoRogue 2.

### Quick Generators
The "full-map generation" algorithms that were present in GoRogue 2 as functions in `GoRogue.MapGeneration.QuickGenerators` are represented in GoRogue 3 as functions in `GoRogue.MapGeneration.DefaultAlgorithms`.  These functions produce a set of generation steps that, when executed in a `Generator`, follow a roughly equivalent process:

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

The configuration options are very similar functionally for most of them, however since these are now generation steps they're specified as public members of the class.

One notable exception is `ClosestMapAreaConnector`; its GoRogue 3 counterpart functions somewhat differently.  The largest change is that it utilizes the new `MultiArea` class to more accurately represent the areas during the connection process, which allows it to take into account all points that are part of areas that have been previously joined by the algorithm.  It also uses the `ConnectionPointSelector` given to it not only to determine the points to use to connect the areas, but the points to use to determine distance between the areas as well, which allows for more control over its distance assumptions.

### Tunnel Creation Algorithms
The tunnel creation algorithms provided in GoRogue 2 are largely intact in GoRogue 3, and have just moved to different namespaces.  The API has changed a bit, but functionally they produce the same result.  The `ITunnelCreator`, `DirectLineTunnelCreator`, and `HorizontalVerticalTunnelCreator` classes are now located in the `MapGeneration.TunnelCreators` namespace.  The API has been modified so that the tunnel creators return an `Area` representing the tunnel they create; but otherwise the algorithms function in much the same way.  They are used as optional parameters in the appropriate map generation steps just like they were in the GoRogue 2 algorithms.

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

### MultiArea
GoRogue now provides a `MultiArea` class in the `MapGeneration` namespace.  This is a relatively simple class that implements the `IReadOnlyArea` interface, by querying a list of "sub-areas".  This class can prove useful because defining an area in terms of a set of other areas is a common need for map generation
and region representation.

### PolygonArea
GoRogue also now provides a `PolygonArea` class in the `MapGeneration` namespace.  This class also implements `IReadOnlyArea`, and allows you to define the area based on a sequence of points/vertices.  It keeps track of both the interior and exterior points of the polygon defined by those points, and allows rotation/mirroring of the area, and other useful operations.

### Translation Steps
The article on map generation refers to "translation steps" as steps that purely transform existing data in the map generation context to a new form, as opposed to generating new, unique data.  It may definitely prove useful to review these steps, located in the `MapGeneration.Steps.Translation` namespace, as they may be useful in general cases.

### Finding Doors
GoRogue now provides a generation step for finding where to place doorways, given wall-floor tile states and a series of rectangular rooms.  The algorithm to do this is relatively simple, but it was requested a number of times as a feature in the past, and is much easier to implement generically in the new system.  See [DoorFinder class documentation](xref:GoRogue.MapGeneration.Steps.DoorFinder) for details.

## New Structures
There are also a number of new data structures that may be useful for recording various forms of data during map generation.  Most of these are in the `MapGeneration.ContextComponents` namespace, or in the root `MapGeneration` namespace.  The API documentation explains the purpose of these classes in detail.

# Component System Changes
The functionality of GoRogue's component system has been expanded in GoRogue 3, and has been moved to its own namespace.  Additionally, some functions have changed names to make them more consistent with traditional collection names.

## Classes and Function Names Refactored
All component-related classes have been moved to the `GoRogue.Components` namespace.  Additionally, classes have been renamed as follows:

| Old Name | New Name |
| -------- | -------- |
| `ComponentContainer` | `Components.ComponentCollection` |
| `IHasComponents` | `Components.IComponentCollection` |
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

Additionally, the public API of `ComponentCollection` is also contained within the `IComponentCollection` interface.  This is a very rough equivalent to the `IHasComponents` interface in GoRogue 2.  It is quite unlikely that a custom implementation of this interface would be necessary, but it is designed and used as such regardless.

## ComponentCollection Focused on Composition
In GoRogue 2, the `ComponentContainer` class presented an interface consistent for use via both inheritance, and composition.  For example, you could have a `ComponentContainer` class as a member of your class and use that field to store components attached to the object, or you could have your class inherit from `ComponentContainer` directly.

Both are still possible in GoRogue 3, however because the interface for `ComponentCollection` is now more similar to traditional C# collections (as discussed above), it now lends itself more easily to composition (like you would generally do with any C# collection).  This falls in-line with changes to the `IGameObject` system in GoRogue 3, and addresses many of the same ease-of-use issues.  In general, this does not severely break APIs; any class that previously inherited from `ComponentContainer` would simply have a property of type `ComponentCollection` and store its components there.

### Interface for Attaching Components
In order to conveniently support common use cases where you want to "attach" components to an object using this new approach, the `IObjectWithComponents` interface has been added in the `GoRogue.Components.ParentAware` namespace.  This interface defines a single properly called `GoRogueComponents`, of type `IComponentCollection`.  This accurately represents an object that has an associated `ComponentCollection` which stores components attached to it.

The `GoRogue.Components.ParentAware` namespace also contains an interface called `IParentAwareComponent`.  This interface specifies a single `Parent` field, of type `object?`.  This is designed to capture the concept of a component which is aware of the parent object it's attached to.  The `ParentAwareComponentBase` class in the same namespace takes this farther by implementing this interface, and also providing events fired when it is attached to/detached from an object.  It also provides useful functionality for using those events to enforce certain invariants upon components.  `ParentAwareComponentBase<T>` adds onto the non-generic class by automatically enforcing that the object it's attached to is of a particular type, and exposing the `Parent` field as that type, which is useful to avoid constantly needing to cast the `Parent` field to the type you need.

The above concepts conveniently represent interfaces involved in attaching components to an object; to complete the implementation, `ComponentCollection` has built-in support for these interfaces.  `ComponentCollection` takes an optional parameter at construction of type `object?`.  If this parameter is specified, the value given is automatically set to the `Parent` field of any `IParentAwareComponent` that is added to it.  Similarly, the `Parent` field is set to `null` when the component is removed.  Components that do not implement this interface are still allowed, however ones that do implement it have their `Parent` property automatically updated.

Putting these features together allows you to write components that are attached to an arbitrary object, and are aware of their "Parent" and capable of interacting with it.  See GoRogue's `GameFramework.GameObject` and `GameFramework.Map` classes for examples of how to create such objects.

# Factory System Changes
The factory system in GoRogue 3 has been refactored to address some naming/usability concerns.  First, note that the `GoRogue.Factory` namespace is now `GoRogue.Factories`, which more accurately matches naming conventions elsewhere in the library.

Additionally, factory classes can now support blueprint IDs of arbitrary types, rather than only strings.  This is implemented via an additional type parameter passed to the factory classes called `TBlueprintID`.

The remaining factory changes fall in line with splitting out the class structures for the two main use cases of factories; to create items when additional configuration information per instance _is_ needed, and to create items when such information _is not_ needed.  In GoRogue 2, both use cases were supported but the class structure for supporting this was somewhat unintuitive; it utilized an arbitrary class `BlueprintConfig` for a base configuration object, that served no purpose other than to provide a default value.  In GoRogue 3, the factory classes have been split out as follows:
- `Factory<TBlueprintID, TProduced>`
    - Produces objects of type `TProduced` via `IFactoryBlueprint` objects that DO NOT take a configuration object in the `Create` function.
    - Useful for cases that would previously have involved `SimpleBlueprint`
- `AdvancedFactory<TBlueprintID, TBlueprintConfig, TProduced>`
    - Produces objects of type `TProduced` via `IAdvancedFactoryBlueprint` objects that DO accept a configuration object of type `TBlueprintConfig` in the `Create` function.
    - Useful for cases that would have previously involved passing a configuration object of some type that subclassed `BlueprintConfig`
    - In GoRogue 3, there is NO required base class for configuration objects; it can be an arbitrary type

`IFactoryObject` will function the same way as it did, whether objects that implement it are created in a `Factory` or `AdvancedFactory`.  As well, a new `ItemNotDefinedException` with a useful message is thrown when their `Create` or `GetBlueprint` methods are called with a blueprint name that does not exist.

Additionally, some helper classes were added that make blueprints signifincantly easier to implement.  These include
[LambdaFactoryBlueprint<TBlueprintID, TProduced>](xref:GoRogue.Factories.LambdaFactoryBlueprint`2) and [LambdaAdvancedFactoryBlueprint<TBlueprintID, TBlueprintConfig, TProduced>](xref:GoRogue.Factories.LambdaAdvancedFactoryBlueprint`3).  These classes allow you to specify the `Create` function as a `Func`, which enables you to avoid implementing the blueprint interfaces via a subclass in many cases.  Details on their intended usage can be found in the [Factories how-to article](~/articles/howtos/factories.md).

# Spatial Map Changes
A number of changes have been made to the interface for spatial maps in GoRogue 3.  First, all spatial map implementations and related interfaces have been moved to the TheSadRogue.Primitives, under the namespace `SadRogue.Primitives.SpatialMaps`.  There are also a number of API changes.

## API Refactor
In GoRogue 2, functions for spatial maps like `Add` and `Move` simply returned `false` if an operation failed.  This design had some useful properties, but ultimately turned out to be a poor design decision, which didn't fit well with the rest of the library and was the source of many known bugs/desyncs that users, and I, accidentally created in code. Therefore, the `ISpatialMap` interface and all implementing classes have been modified such that, if their functions fail, they throw an `ArgumentException` with an error message that tells you exactly what went wrong.  Functions that work in this way now include `GetPositionOf` (previously called `GetPosition`), `Add`, `Move`, `MoveAll`, and `Remove`.

This change makes it much more obvious to the user when something unexpected happens.  For situations where you _do_ potentially expect the operation to fail, versions of the functions are provided that begin with `Try`; eg. `TryMove`, `TryAdd`, `TryRemove` `TryGetPositionOf`, etc.  These default to the old behavior returning true or false based on success.  If you simply need to check whether an operation is possible, the interface also provides functions for this; these functions include `CanAdd`, `CanMove`, `CanMoveAll`, etc.

Another associated change to spatial maps is the separation of the functionality for moving _all_ items at a location vs. moving only the ones that _can_ move.  In addition to `Move(T item)`, `ISpatialMap` now also contains `MoveAll` and `MoveValid` functions.  `MoveAll` attempts to move all items at a given location to the target location, throwing an `ArgumentException` if this fails.  There is also a `TryMoveAll` function, which returns false instead of throwing an exception.  `MoveValid` only moves the items that can move, returning precisely which items were moved.

Finally, note that, as implied above, `GetPosition` has been renamed to `GetPositionOf`.  There are also a number of similar methods provided that handle the case where the position does not exist differently; these include `GetPositionOfOrNull` and `TryGetPositionOf`.

## Position Syncing
In GoRogue, spatial maps store a position for each object stored within them.  The `Move` functions and similar modify this position.  This is useful because it allows spatial maps to be self-contained; objects added to them don't have to have their own position field or meet any sort of arbitrary interface.  However, it also adds complexity to their usage for some use cases, because it is common that map objects will store their own position inside of a field.  When such objects are used in spatial maps, care must be taken to ensure that the spatial map's position for that object, and the position within the object's field, remain in sync.

In GoRogue 3, some new variants of spatial maps have been added to help make these use cases easier to manage.  These include `AutoSyncSpatialMap`, `AutoSyncMultiSpatialMap`, and `AutoSyncLayeredSpatialMap`.  These variants require that the items in them implement the new `IPositionable` interface, which contains a `Position` property and some events that are fired when that property changes.  It uses these events to automatically keep the `Position` property, and the spatial map's internal record of the object's position, in sync.  You may modify the `Position` property directly, or call any of the spatial map's move functions; and in either case, both the field and the spatial map's record update automatically.

# FOV Changes
In GoRogue 2, FOV functionality consisted of a single class, `FOV`, which implemented a recursive shadowcasting FOV algorithm.  GoRogue 2 also contained the `IReadOnlyFOV` interface, which was useful for exposing the result of FOV without allowing modification.

This worked well initially because users could either use the built-in FOV class, or create their own arbitrary algorithm if desired.  When `GameFramework` was introduced, however, this became problematic because its `Map` class had a property of type `FOV`; so if a user wanted to use a custom FOV algorithm, they had to give up the explored-tile functionality and anything else pertaining to FOV that the map provided.

## Introduction of Abstraction
GoRogue 3 addresses this by introducing a customizable abstraction for FOV calculations.  First, the `FOV` class has been renamed to `RecursiveShadowcastingFOV` and moved to the `GoRogue.FOV` namespace.  This new namespace also contains `IReadOnlyFOV`.  It also has an additional interface `IFOV`, which `RecursiveShadowcastingFOV` now implements.  This interface contains the entire public API of what used to be called `FOV`, thus representing an abstraction over a method of calculating FOV.  Finally, this namespace also contains `FOVBase`, which is an abstract base class that implements `IFOV` and simplifies the interface by ensuring that a minimal subset of functions must be implemented by a user.

In turn, `Map` now has a property of type `IFOV` for the player's FOV.  This allows a user to implement a custom FOV calculation and use it within the map framework.

## Change in Interface
In GoRogue 2, FOV classes implemented `IMapView<double>` in order to allow you to access the results.  In GoRogue 3, this is no longer the case; instead, there is a `DoubleResultView` property that exposes the results of the FOV calculation as a grid view.  Similarly, the `BooleanFOV` property has been renamed to `BooleanResultView`.  This change more easily facilitated the abstraction that was introduced, and as well provided a more consistent interface.

# GameFramework Namespace Refactored
A number of refactors have been performed on the `GoRogue.GameFramework` namespace.

## Game Object Changes
`IGameObject` and the way it is used has undergone a notable refactor to address some usability issues present in GoRogue 2.

### Simplification of IGameObject Implementation
The class structure for `GameFramework.GameObject` has been simplified in GoRogue 3.  In GoRogue 2, support for use cases where you were unable to inherit from `GameObject` involved writing fairly complex code.  You had to implement `IGameObject` in these cases; but, since implementation of the functionality defined by `IGameObject` was closely coupled with the code for `GameFramework.Map`, this was non-trivial.  The recommended approach for this in GoRogue 2 was to use a `GameObject` instance as a private backing field for your `IGameObject` implementation; and in fact GoRogue 2 had special support for this built into `GameObject` in the form of the `parent` parameter to the constructor.

However, this approach proved to be somewhat error prone, and very easily led to non-intuitive/difficult to debug behavior in error cases.  Furthermore, it added complexity to code that needed to implement `IGameObject` instead of inheriting from `GameObject`.  So, in GoRogue 3, the `parent` parameter in the `GameObject` constructor has been removed, and it is no longer recommended (and in many cases no longer possible) to implement `IGameObject` via a backing field of type `GameObject`.

Instead, a focus was placed on decoupling the code required to implement `IGameObject` from the internal code in `Map`, which ultimately resulted in `IGameObject` being much easier to implement the more traditional way.  Helper functions have been provided that make implementing most functions in the `IGameObject` interface a one-line endeavor.  As such, if you need to implement `IGameObject` yourself, you should be able to more or less copy-paste the code from `GameObject`, and use it for the implementation, without making your code unintuitive or unnecessarily long.

### Removed IsStatic
In GoRogue 2, `GameObject` instances had an `IsStatic` flag, that could be set via a constructor parameter to indicate that they could not move.  Objects on the terrain layer of a map (layer 0) were _required_ to have `IsStatic` set, to allow for some optimization by allowing them to reside on a grid view as opposed to a spatial map.  This was useful for performance, but inconvenient at times for users; particularly if a user wanted to use something like GoRogue's factory system for creation of terrain.  The position could only be set at construction due to the `IsStatic` flag being set, which meant that the position had to be known exactly when the instance was created.  Furthermore, the restriction of terrain objects being unable to move at all is actually more stringent than required; the only required portion for optimization is that they not move _while they are part of a map_.

Therefore, in GoRogue 3 the `IsStatic` flag has been removed.  Instead, if an object is on layer 0 of a map and it is moved, an exception will be thrown by the `Map`. Objects on layer 0 that are not added to a `Map` can be moved freely.  This allows the optimization to stay, but makes creation of objects more convenient for a user.

### Components Now Attached to a Property
In GoRogue 2, `IGameObject` implemented `IHasComponents` to allow components to be placed on game objects.  In GoRogue 3, this is no longer the case; instead, `IGameObject` defines a property `GoRogueComponents`, which is a collection that can have components added/removed.  This class also integrates the new "tag" functionality for tagging components.  The names of the functions for adding, removing, and retrieving components have also changed in a manner corresponding to the [component collection name changes](#component-system-changes).

The `GoRogueComponents` field in `GameObject` defaults to being of type `Components.ComponentCollection`, and it is generally rare to need that field to be represented by some other type.  Nonetheless, the field is of type `Components.IComponentCollection`, and an instance of some custom implementation of this interface may be passed to `GameObject` via its constructor.  This ensures that the component collection structure can be customized if it becomes necessary.



### Setting Position or Walkability may Throw Exception
In support of the [spatial map changes](#spatial-map-changes) in GoRogue 3, setting the `Position` or `IsWalkable` properties of a game object will throw an exception if they are set to a value that would violate map collision rules.  See the corresponding [Map changes section](#addingremovingmoving-objects-may-throw-exception) for details.

### Optional Base Class for Components
GoRogue 2 contained the `GameFramework.Components.IGameObjectComponent` interface, which was an optional interface that you could implement on components attached to a `GameObject`.  When implemented, the object would automatically have its `Parent` property value updated to reflect the `IGameObject` that it was attached to.  In GoRogue 3, this interface has been removed, and replaced with the more generic system defined in the `Components.ParentAware` namespace (as discussed [here](#interface-for-attaching-components)).  This new system is simply a more generic version of `IGameObjectComponent`, and provides nearly equivalent functionality.

This also means that `IGameObject` components may utilize and benefit from the `ParentAwareComponentBase` and `ParentAwareComponentBase<T>` defined in the `Components.ParentAware` namespace.  This can be very convenient if you need to safely represent advanced concepts like having components be able to access methods/properties of their specific parent type.

### ID Generation Changes
One additional change alters the method that you use to assign IDs to game objects in a custom way.  In GoRogue 2, to assign IDs in a custom manner, you had to subclass `GameObject` and override the `GameObject.GenerateID()` function.  Given that `GameObject` instances can often otherwise be used with no subclasses at all, the requirement to create a subclass for this functionality proved inconvenient.

Therefore, in GoRogue 3, the `GameObject.GenerateID()` function has been removed.  Instead, an optional parameter of type `Func<uint>` can be passed to the `GameObject` constructor.  If the parameter is passed, the given function is used to generate an ID.  If not, the default method of randomly generating an ID is used.  This still allows a custom method to be used, but does not require that a user create a subclass to do so.

## Map Functionality Changes
There were also a number of relevant changes to the `GameFramework.Map` API in GoRogue 3.

### FOV Changes
In GoRogue 2, the `Map` had `CalculateFOV` functions that were used to calculate FOV for the map.  This made it more difficult to separate FOV from the map in use cases that required it, and required users to implement inconvenient overrides of the `CalculateFOV` functions in order to react to FOV changes.  In GoRogue 3, a number of refactors have taken place to address this.

First, the `Map.FOV` property has been renamed to `Map.PlayerFOV`.  This more clearly reflects the intended purpose of the field in games using multiple FOV instances for different entities.  Similarly, the `Explored` field has been renamed to `PlayerExplored`.

Additionally, the `CalculateFOV` functions have been completely removed from `Map`.  Instead, the `PlayerFOV` field is of type `IFOV` instead of `IReadOnlyFOV`.  This allows you to call the `PlayerFOV.Calculate` function directly, with the same parameters previously passed to `Map.CalculateFOV`.  The FOV system has also been augmented to include a `Recalculated` event that fires whenever the FOV is recalculated, which allows you to respond to the FOV changing.  Since the `PlayerFOV` field is still settable, these changes make it much easier to separate the FOV from the `Map` when necessary and makes responding to FOV changes more convenient.

### Components Allowed on Map
One other change is that `GameFramework.Map` now has a `GoRogueComponents` property.  This allows you to attach components to a `Map` just like you do with game objects.  This may be useful functionality for component-based architectures.  These components may also make use of the system defined in `Components.ParentAware`.

### RemoveTerrain Functionality Added
A `RemoveTerrain` function has been added that takes a terrain object and removes it from the map, in support of ensuring that `SetTerrain` can be annotated as taking non-null parameters.  There is also a `RemoveTerrainAt` function to allow you to remove whatever terrain is at a given position.

### Naming Conventions for Position-Based Functions Changed
Functions that operate based on a position have changed name slightly; these functions will have "At" appended to the end of the name.  For example, `Map.GetTerrain` is now `Map.GetTerrainAt`, `Map.GetEntity` is now `Map.GetEntityAt`, and so forth.

### Adding/Removing/Moving Objects May Throw Exception
In GoRogue 3, spatial maps have been changed to throw exceptions in cases where invalid operations are performed (see the [corresponding section](#spatial-map-changes)).  Because `GameFramework.Map` is partially based upon spatial maps, and it exposes a number of similar operations via its API, similar changes have been made to `Map` and `IGameObject`.

The `AddEntity`, `RemoveEntity`, `SetTerrain`, `RemoveTerrain` and `RemoveTerrainAt` functions all return `void` and throw `ArgumentException` if they are called with a value that doesn't meet the necessary criteria, with an exception message detailing exactly what the issue was.  Similarly, `Map` is configured in such a way that if an object's `Position` or `IsWalkable` properties are set to something that is invalid, then an `InvalidOperationException` with a message detailing the issue will be thrown.

There are also extension methods provided for `IGameObject` that return a boolean value indicating whether it is valid to set the object's `Position` or an `IsWalkable` property a certain way.  The `CanMove` function returns whether or not an object's position can be set; `CanMoveIn` is similar, except it takes a direction and returns whether or not the object can move in that direction.  Similarly, `CanSetWalkability` and `CanToggleWalkability` return information about whether or not `IsWalkable` can be set.

# RadiusAreaProvider Functionality Moved
The functionality of `RadiusAreaProvider` was moved to `TheSadRogue.Primitives`, and merged into the `Radius` class in the form of `Radius.PositionsInRadius` functions.

# Random Number Generation Rewritten
In GoRogue 2, random number generation was handled by way of the [Troscheutz.Random](https://gitlab.com/pomma89/troschuetz-random/-/tree/main/src/Troschuetz.Random) library.  This worked reasonably, however there were a number of issues that required workarounds for some use cases of GoRogue:
- At least some tests for statistical quality find issues with many, if not most, of Troschuetz's random number generator implementations.  Some are more robust and do not fail, but these are mostly the slower generators in the library.
    - Many of the generators pass older, simpler tests like the first version of DIEHARD, but fail some tests in newer and more stringent suites like [PractRand](http://pracrand.sourceforge.net/).
    - Some generators are old enough that flaws are well-known, such as the Mersenne Twister.
    - Others have been superseded by later generators, like XorShift128+ and its improved successors xoroshiro128** and xoshiro256**.
    - Some also have dubious licensing restrictions (the three NR3 generators, only one of which passes more than a minute of quality testing)
- Troschuetz's API for generators does not expose the state of the generators, and the generators can only be serialized out of the box via select serialization methods (eg. serialization methods that support `[Serializable]`).
    - Therefore, when users needed to use a serialization method which was unsupported by Troschuetz directly, they had to simply record the seed value used to create the RNG, and then manually advance the generator state upon deserialization to get it back to what it was when the generator was serialized.
    - This method could be performance intensive, and in some cases impractically so, and also tended to make testing and debugging more difficult.
- Troschuetz's API does not directly expose functionality to generate longs, ulongs, floats, or decimals.

## ShaiRandom Overview
In an effort to provide better solutions to these issues, GoRogue 3 has moved away from Troschuetz, and instead uses a random number generation library called ShaiRandom.  In addition to implementing a completely different set of generation algorithms, it provides a number of other benefits:
- Generator states are publicly exposed, and settable.
    - This allows much more fine-tuned manipulation of generators, for both testing and serialization purposes.
- Generators all support a method of serializing to and from a string
    - This remains independent of any particular serialiation method, but still provides easy ways to handle saving and restoring generator states
- Generators support "previous" and "skip" operations where possible
- Generators support long, ulong, float, and decimal generation
- ShaiRandom can utilize more modern C# features, since it only supports back to .NET Standard 2.1
    - This allows some performance optimizations such as using spans for serialization/deserialization, as well as some convenience features

ShaiRandom also provides a number of other benefits; among them are some additional extension methods, wrappers and helpers that can assist in replicating and debugging issues with algorithms using RNG, and better run-time performance than both the Troschuetz generators and `System.Random`. for details, see [ShaiRandom's documentation](https://github.com/tommyettinger/ShaiRandom).

## Porting Guide
Despite the improvements, from a functional perspective, ShaiRandom is very similar to Troschuetz.  The most subtle differences pertain to the minimum/maximum bounds of values returned from some similarly named functions and interfaces.

Therefore, although the differences between the two libraries are non-trivial, most of any difficulties porting existing code will likely be due to differences in the bounds of parameterless generation functions, if you are using any of them.  The following sections will attempt to outline the changes most likely to be relevant when porting existing GoRogue 2 or Troschuetz-based code over to use GoRogue 3 and ShaiRandom.  Note that there is a substantial amount of ShaiRandom functionality that is not mentioned here (because it has no direct Troschuetz equivalent); feel free to look through ShaiRandom's API documentation for details on those features.  

### Interface/Function Names
In Troschuetz, `IGenerator` is the interface used to accept an arbitrary RNG.  `ShaiRandom` has a similar interface called `IEnhancedRandom`.  However, the names of equivalent functions between these two interfaces differ in some cases.  The following table lists a rough mapping of Troschuetz function names to ShaiRandom ones:

| Troschuetz Name | ShaiRandom Name |
| --------------- | --------------- |
| `Next` | `NextInt` **\*** |
| `NextUInt` | `NextUInt` **\*** |
| `NextInclusiveMaxValue` | `NextInt` **\*\*** |
| `NextUIntInclusiveMaxValue` | `NextUInt` **\*\*** |
| `NextBoolean` | `NextBool` |
| `NextBytes` | `NextBytes` |
| `NextDouble` | `NextDouble` |

**\*** The ShaiRandom functions treat bounds (and 0-parameter versions) differently than Troschuetz does; see the below sections for details.

**\*\*** As described below, the 0-parameter versions of the listed ShaiRandom functions _are_ inclusive on max value.

### Zero-Parameter Integer Function Bounds
In Troschuetz, unless otherwise specified, integer-generation functions which take 0 parameters are _exclusive_ on the type's max-value; eg. `IGenerator.Next()` can return values in range `[int.MinValue, int.MaxValue)`, `IGenerator.NextUInt()` can return values in range `[uint.MinValue, uint.MaxValue)`, and so on.

In `ShaiRandom`, the integral type generation functions which take 0 parameters are _inclusive_ on their max bound; eg. `IGenerator.NextInt()` can return values in range `[int.MinValue, int.MaxValue]`, `IGenerator.NextUInt()` can return values in range `[uint.MinValue, uint.MaxValue]`, and so on.

Note that this applies only to _integer_ generation functions; the 0-parameter floating point functions in both ShaiRandom and Troschuetz both return a value in the range 0 (inclusive) to 1.0 (exclusive).

### Bounded Generation Function Contracts
Both Troschuetz and ShaiRandom have various generator functions which take either one or two bounds, and guarantee that the number returned lies within those two bounds.  However, how those bounds are interpreted differs between the two libraries.

The biggest difference for functions taking a _single_ bound, is that Troschuetz interprets that bound as a "maximum" value, whereas ShaiRandom interprets the bound as simply an "outer" bound.  In Troschuetz, for instance, even for `Next(int)` or `NextDouble(double)` functions, the bound specified must be greater than or equal to 0 (passing it anything else results in an exception).  ShaiRandom does not have this limitation.  The following table describes the behavior for ShaiRandom's generation functions which take a _single_ bound:

| Bound Relation | Allowed Range of Returned Value |
| -------------- | ------------------------------- |
| bound > 0 | `[0, bound)` (eg. 0 (inclusive) to `bound` (exclusive)) |
| bound == 0 | `0` |
| bound < 0 | `(bound, 0]` (eg. `bound` (exclusive) to 0 (inclusive)) |

Similar logic applies for functions that take _two_ bounds.  Troschuetz asserts that the first bound is a "minimum" value, and the second a "maximum", whereas ShaiRandom interprets the first as an "inner" bound and the second as an "outer" bound.  In Troschuetz, the second bound must always be greater than or equal to the first; the bounds are not allowed to cross each other.  ShaiRandom allows this, and defines the behavior as follows:

| Bound Relation | Allowed Range of Returned Value |
| -------------- | ------------------------------- |
| inner < outer | `[inner, outer)` (eg. `inner` (inclusive) to `outer` (exclusive)) |
| inner == outer | `inner` |
| inner > outer | `(outer, inner]` (eg. `outer` (exclusive) to `inner` (inclusive)) |

Note that the inclusivity and exclusivity of the bounds as outlined above refers to the bounds for "typical" functions; eg. the bounds for `NextInt`, `NextUInt`, `NextDouble`, etc; anything that does not have `Inclusive` or `Exclusive` specifically in the function name.  ShaiRandom does provide a number of functions for generating floating point numbers that specify otherwise; for example, `NextInclusiveDouble` follows the same logic but considers both bounds as inclusive, and `NextExclusiveDouble` is similar but considers both bounds to be exclusive.  These functions will clearly note how they interpret the bounds in the API documentation.

### Seeding and Serialization
In Troschuetz, generators exposed a `Seed` property, which you could use to query the seed used to initialize a random number generator.  In ShaiRandom, the state is exposed directly, and can be accessed via either the `IEnhancedRandom.SelectState` function, or various properties specific to generator implementations.  Because of this, the API handles seeding from a single value differently than Troschuetz.

In ShaiRandom, you may call the `IEnhancedRandom.Seed(ulong)` function on any generator implementation, in order to initialize it in a deterministic way based on the given seed value.  However, once you call this function, there is no way to retrieve the value that you passed to that function; in most generator implementations, it won't be stored directly in the state fields or any property you can access.  Instead, the `Seed(ulong)` function implementation will take that single `ulong` passed to it, and use it to initialize all of the state variables of the generator (deterministically, of course).

Functionally, nothing is lost here.  In fact, ShaiRandom offers a lot of other APIs to interact with generator state directly; and so ShaiRandom is functionally _more_ robust than Troschuetz in this context.  Nevertheless, this may affect existing code in that it may change how it serializes data or initializes generators to a particular state.

There are two main ways you can go about replicating and controlling a generator's state:
1. Utilize the serialization ShaiRandom provides.  For any generator, the `StringSerialize()` function will produce a string of ASCII characters which encompasses the generators entire state.  This string can be passed to the `AbstractRandom.Deserialize()` function, which will create a new generator of the same type, with exactly the same state as the one that was serialized.
2. Initialize the generator context with the `Seed(ulong)` function, and record the seed used before you pass it, so that you may use it again in the future.  Although not as flexible as other methods, it is relatively similar to the method of recording the `Seed` property value on Troschuetz generators.

Furthermore, you can also control a generator's state directly via IEnhancedRandom's `SetState`, `SetSelectedState`, and `SelectState` functions, as well as properties specific to each generator implementation.  The `PreviousULong` and `Skip` methods may also be useful for controlling generator state.  ShaiRandom's API documentation provides more specific information on these methods.

## RNG Implementations Moved
In GoRogue 2, GoRogue had a number of random number "generator" implementations in the `GoRogue.Random` namespace.  These included `KnownSeriesRandom`, which allowed a user to specify exactly what numbers to return from the generation functions (useful for unit testing), as well as `MinRandom` and `MaxRandom`, which effectively implemented the generation functions to return the minimum and maximum possible values, respectively, given the specified bounds.

In GoRogue 3, these implementations have all been moved to ShaiRandom, and are located in the `ShaiRandom.Generators` namespace.  Furthermore, their functionality has been expanded upon significantly; all of these generators support the full suite of `IEnhancedRandom` number generation functions, and all of them are serializable using ShaiRandom's string-based serialization method.

### ArchivalWrapper
An `ArchivalWrapper` generator exists in the `ShaiRandom.Wrappers` namespace, which drastically augments the usefulness of `KnownSeriesRandom`.  `ArchivalWrapper` wraps another `IEnhancedRandom` implementation and allows you to very easily reproduce the values produced by the generator it's wrapping.  Once created, the `ArchivalWrapper` can be passed to any arbitrary algorithm taking an `IEnhancedRandom` implementation.  Then, after the algorithm uses it, the wrapper can create a `KnownSeriesRandom` that will produce _exactly_ the same sequence of numbers produced by the generator it wrapped.

This can be extremely useful for debugging and unit testing, because it requires you to have no knowledge of the inner workings of an algorithm to reliably reproduce an issue with it; and the ability to reproduce that issue is completely independent of any RNG implementation, and anything else that RNG instance is used for before/after the problematic algorithm used it.  The following highlights this with some example (pseudo) code:

[!code-csharp[](../../GoRogue.Snippets/UpgradeGuide2To3.cs#ArchivalWrapper)]

You can, of course, create a similar result by simply serializing the state of the underlying generator directly before each run of the problematic algorithm, and thus avoid using `KnownSeriesRandom` or `ArchivalWrapper` at all; however, the resulting serialized generator will only continue to reproduce the problem so long as the implementation of the RNG used to create it does not change; the `KnownSeriesRandom` approach removes the original RNG entirely from the replication process.

## Namespace and Helper Method Changes
The `GoRogue.Random.SingletonRandom` class that existed in GoRogue 2 has been renamed to `GoRogue.Random.GlobalRandom`, to more accurately reflect its purpose.  The `DefaultRNG` parameter should otherwise be the same (other than now being of type `IEnhancedRandom`), and will simply require updating the class name in any references.


### Extension Methods for Built-In C# Types
GoRogue 2 also provided an array of extension methods for built-in C# types, which pertained to RNG in some capacity.  For example, there were several extension methods defined for `List<T>`, which included `FisherYatesShuffle`, `RandomItem`, and `RandomIndex`.  Similar methods were provided for other types, including arrays.  All of these methods have been moved to ShaiRandom, and some have been renamed.  The following lists a mapping of old names to new:

| Old Name | New Name |
| -------- | -------- |
| `FisherYatesShuffle<T>` | `Shuffle<T>` |
| `RandomItem<T>` | `RandomElement<T>` |
| `RandomIndex<T>` | `RandomIndex<T>` |

Additionally, these methods are now extension methods of `IEnhancedRandom`, rather than being extension methods of the built-in C# type they interact with.  So, whereas in GoRogue 2 you would write:

```CS
myList.FisherYatesShuffle(myRNG);
```

In GoRogue 3, you would write this instead:

[!code-csharp[](../../GoRogue.Snippets/UpgradeGuide2To3.cs#ShuffleListExample)]

This helps to consolidate similar functions into a single namespace.  Note that you will need to have `using ShaiRandom;` in the code file to be able to call these methods.

### Random-Oriented Methods for Other Types
GoRogue 2 also provided methods equivalent to `RandomItem` and `RandomIndex`, which operated on map views, rectangles, areas, and other types which were created by GoRogue.  These types simply defined the function as part of the type, which made the calling syntax look very much like the extension methods for C# types:

```CS
var itemFromList = myList.RandomItem(myRNG);
var itemFromRect = myRect.RandomPosition(myRNG);
```

These extension methods still exist in GoRogue 3; however since the ones for built-in C# types are now extension methods for _generators_ instead of the container type (as explained in the above section), the methods on these GoRogue defined classes have been refactored to match.  So, given the above code in GoRogue 2, you would write it the following way in GoRogue 3:

[!code-csharp[](../../GoRogue.Snippets/UpgradeGuide2To3.cs#RNGExtensionsCustomTypes)]

Note that although these extension methods are defined by GoRogue, they are still defined within the `ShaiRandom.Generators` namespace, so you will need to have `using ShaiRandom.Generators;` in your code files to be able to access them. 

## Added PercentageCheck
Additionally, a `PercentageCheck` function has been added, also as an extension method of `IEnhancedRandom`.  This makes it much more convenient to use an RNG to perform a "percentage check" (a check with a specified percent chance to succeed) with any RNG, including `DefaultRNG`.

Note that this function is quite similar to the `NextBool(float chance)` method that ShaiRandom defines; however there are two key differences:
1. `PercentageCheck` takes its parameter as a float in range [0f, 100f], whereas `NextBool` accepts its percentage as a value between 0f and 1f.
2. `PercentageCheck` will throw an exception if an out-of-range percent is given, whereas `NextBool` will tolerate arbitrary values.

Both behaviors can be useful depending on the situation; other than these differences, the two methods perform the same function.

# Effects System Refactored
GoRogue 2 contained an "effects" system designed to help users implemented game mechanics such as damage, armor, healing, etc in an extensible way.  It consisted of an `Effect<T>` class and an `EffectTrigger<T>` class, where the `Effect<T>` class would be subclassed to implement an effect, and effect instances could then be added to an `EffectTrigger<T>` instance, which represented an event and could trigger any added effects.

In GoRogue 3, this system still exists, but has been refactored to address some usability and performance concerns.  The implementation in GoRogue v2 had a few main issues:

1. The type parameter made usage unnecessarily complex for simple use cases, and could be initially misleading to users.   Primarily, this is because the type parameters given to `Effect`, and the parameters given to an `EffectTrigger` it is added to, had to match; only effects that take the same parameter to their `Trigger()` functions can be added to a given `EffectTrigger`.  The basic implementation having this type parameter tended to encourage using it to pass parameters that were better passed as constructor parameters to the `Effect` subclass, and would get users into trouble due to type mismatches.

2. The API was confusing for those using only `Effect` that didn't need a parameter, and without an `EffectTrigger`, because the parameter was completely useless. 

3. The type used for the type parameter needed to be a reference type.  This has a number of performance implications due to the allocation of new types constantly.

These concerns have been addressed in GoRogue 3 via the following changes.

## Effects Namespace
First, all effect-related classes have been moved to the `GoRogue.Effects` namespace.  This on its own is a relatively minor change, and is mostly a side effect of there being more classes related to effects in the new implementation.

## Effect and EffectTrigger Do Not Take Type Parameters
The biggest change, is that `Effect` and `EffectTrigger` no longer take any type parameters.  Instead, `Effect.Trigger` and `Effect.OnTrigger` take an `out bool` parameter, which you set to `true` if you want to cancel a trigger.  This is the equivalent of setting `args.CancelTrigger` to `true` in the old implementation.  `EffectTrigger.TriggerEffects` doesn't take any parameter at all; instead, it creates a boolean internally and passes it as the `out bool` parameter to `Trigger`.

Additionally, there is an overload of `Effect.Trigger` which takes no parameter at all.  This is simply meant as a convenience for when you're calling `Trigger` manually for an instantaneous effect and don't care about the parameter value.

This change removes the ability for you to pass custom parameters to `Effect.OnTrigger`.  If you do need to do so, you will instead need to use the new classes introduced into `GoRogue.Effects`; `AdvancedEffect<TTriggerArgs>`, and `AdvancedEffectTrigger<TTriggerArgs>`.  These classes are identical to `Effect` and `EffectTrigger`, respectively, except that they take an additional parameter to their trigger-related functions of type `TTriggerArgs`.  This is functionally equivalent to `Effect<T>` and `EffectTrigger<T>` from GoRogue 2; except that the `TTriggerArgs` type can be of any type (value or reference type), and therefore the `out bool` parameter still exists to allow for cancellation.

Examples that use both `Effect`/`EffectTrigger` and `AdvancedEffect<TTriggerArgs>`/`AdvancedEffectTrigger<TTriggerArgs>` can be found in the [effects system how-to article](~/articles/howtos/effects-system.md).

