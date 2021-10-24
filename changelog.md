# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added
- Spatial map implementations now allow you to specify a custom point hashing algorithm to use

### Fixed
- FleeMaps now properly support goal maps where the base map has open edges (fixes #211)

### Changed
- Applied performance optimizations to A* algorithm
    - ~20% improvements to speed when generating paths on open maps
    - Performance gain varies on other test cases but should generally be measurable
- Applied performance optimizations to GoalMap and FleeMap algorithms
    - ~50% improvement on a full-update operation, using a wide-open (obstacle free) base map
    - Performance gain varies on other test cases but should generally be measurable

## [3.0.0-alpha08] - 2021-10-17

### Added
- Added the `IGameObject.WalkabilityChanging` event that is fired directly _before_ the walkability of the object is changed.
- Added the `IGameObject.TransparencyChanging` event that is fired directly _before_ the transparency of the object is changed.
- Added `IGameObject.SafelySetProperty` overload that can deal with "changing" (pre) events as well as "changed" (post) events.

### Fixed
- Fixed bug in `GameFramework.Map` that prevented setting of the walkability of map objects.

## [3.0.0-alpha07] - 2021-10-13

### Added
- GameObject now has constructors that omit the parameter for starting position

### Changed
- `MessageBus` now has improved performance
- `MessageBus` also now supports subscribers being registered while a `Send` is in progress
    - No subscribers added will be called by the in-progress `Send` call, however any subsequent `Send` calls (including nested ones) will see the new subscribers
- `ParentAwareComponentBase` now specifies old parent in `Removed` event.

## [3.0.0-alpha06] - 2021-10-02

### Added
- Added a constructor to `Region` that takes a `PolygonArea` as a parameter and avoided copies.
- Added an event that will automatically fire when the `Area` property of a `Region` is changed.

### Changed
- Comparison of `PolygonArea` now compares exclusively based off of defined corner equivalency.

### Removed
- All functions and constructors in `Region` that forwarded to corresponsding functions in `PolygonArea`.
    - Such functions and constructors are now only available by accessing `Area` property

### Fixed
- `Parallelogram` static creation method for `PolygonArea` now functions correctly for differing values of width/height (ensures correct 45 degree angles)

## [3.0.0-alpha05] - 2021-09-30

### Added
- FOV now has `CalculateAppend` functions which calculate FOV from a given source, but add that FOV into the existing one, as opposed to replacing it
- FOV now also has a `Reset` function and a `VisibilityReset` event that may be useful in synchronizing visibility with FOV state
- `PolygonArea` class that implements the `IReadOnlyArea` interface by taking in a list of corners defining a polygon, and tracking the outer boundary points of the polygon, as well as the inner points
    - Also offers transformation operations
- `Region` class (_not_ the same class as was previously named `Region`) that associates a polygon with components and provides a convenient interface for transformations
- `MultiArea` now offers a `Clear()` function to remove all sub-areas
- `MapAreaFinder` now has a function that lets you get a _single_ area based on a starting point (eg. boundary fill algorithm), rather than all areas in the map view

### Changed
- `IFOV` interface (and all implementations) now takes angles on a scale where 0 points up, and angles proceed clockwise
    - This matches better with bearing-finding functions in primitives library and correctly uses the common compass clockwise rotation scale
- `SenseSource` now takes angles on a scale where 0 points up, and angles proceed clockwise (thus matching FOV)
- `Region` has been rewritten and is replaced with `PolygonArea`
    - `PolygonArea` supports arbitrary numbers of corners and now implements `IReadOnlyArea`
    - Contains the static creation methods for rectangles and parallelograms previously on `Region`, as well as new ones for regular polygons and regular stars
    - The class called `Region` associates a `PolygonArea` with a set of components and provides convenient accesses to the area's functions/interfaces
        - This functions as a substitute for the sub-regions functionality previously on `Region`
- Significant performance optimizations applied to `MultiArea` and `MapAreaFinder`
- Updated minimum needed primitives library version to `1.1.1`

### Fixed
- `FOVBase.OnCalculate` function (all overloads) is now protected, as was intended originally
- Summary documentation for `FOVBase` is now complete
- `FOV.RecursiveShadowcastingFOV` now handles negative angle values properly
- `SenseMapping.SenseSource` now handles negative `Angle` values properly

## [3.0.0-alpha04] - 2021-06-27
### Added
- Created `GoRogue.FOV` namespace to hold everything related to FOV
- `IFOV` interface now exists in the `GoRogue.FOV` namespace which defines the public interface for a method of calculating FOV
- `FOVBase` abstract class has been added to the `GoRogue.FOV` namespace to simplify creating implementations of the `IFOV` interface


### Changed
- Attaching `IParentAwareComponent` instances to two objects at once now produces a more helpful exception message
- `FOV` renamed to `RecursiveShadowcastingFOV` and moved to the `GoRogue.FOV` namespace
- FOV classes no longer implement `IGridView<double>`; instead, access their `DoubleResultView` field for equivalent behavior
- FOV's `BooleanFOV` property renamed to `BooleanResultView`
- The `GameFramework.Map.PlayerFOV` has changed types; it is now of type `IFOV` in order to support custom implementations of FOV calculations.

### Fixed
- `ParentAwareComponentBase.Added` is no longer fired when the component is detached from an object

## [3.0.0-alpha03] - 2021-06-13

### Added
- `Map` now has parent-aware component support via an optional interface (similar to what GameObjects support)
- Added generic parent-aware component structure (objects that are aware of what they are attached to) that can support arbitrary parent types instead of just `GameObject` instances.
- Added events to `IGameObject` that must be fired when an object is added to/removed from a map (helper method provided)

### Changed
- `ComponentCollection` now handles parent-aware components intrinsically (meaning `IGameObject` implementations don't need to worry about it anymore)
- `IGameObject` implementations should now call `SafelySetCurrentMap` in their `OnMapChanged` implementation (as demonstrated in GoRogue's GameObject implementation) to safely fire all events
- `ITaggableComponentCollection` has been effectively renamed to `IComponentCollection`

### Fixed
- `ParentAwareComponentBase.IncompatibleWith` component restriction now has proper nullability on parameter types to be compatible with the `ParentAwareComponentBase.Added` event

### Removed
- Removed `GameFramework.IGameObjectComponent` interface (replaced with `IParentAwareComponent` and/or `ParentAwareComponentBase<T>`
- Removed `GameFramework.ComponentBase` and `GameFramework.ComponentBase<T>` classes (replaced with `ParentAwareComponentBase` and `ParentAwareComponentBase<T>`)
- Removed `IBasicComponentCollection` interface (contents merged into `IComponentCollection`)

## [3.0.0-alpha02] - 2021-04-04

### Added
- `MultiArea` class that implements `IReadOnlyArea` in terms of a list of "sub-areas"
- `Clear()` and `Count` functionality to component collections
- Specific functions `RandomItem` and `RandomIndex` for `Area` (since `Area` no longer exposes a list)
- Added `RegenerateMapException` which can be thrown by `GenerationStep` instances to indicate the map must be discarded and re-generated
- Added `ConfigAndGenerateSafe`/`ConfigAndGetStageEnumeratorSafe` functions that provide behavior in addition to the typical `AddStep/Generate` type sequence that can handle cases where the map must be re-generated.
- `DisjointSet` now has events that fire when areas are joined
- `DisjointSet<T>` class which automatically assigns IDs to objects

### Changed
- Updated to v1.0 of the primitives library
- Modified `Area` to implement new primitives library `IReadOnlyArea` interface
    - List of positions no longer exposed
    - `Area` has indexers that take indices and return points directly
- Modified map generation interface to support critical exceptions that require map re-generation
    - You must now call `ConfigAndGenerateSafe` to get equivalent behavior to `gen.AddSteps(...).Generate()` that will automatically handle these exceptions
- Modified `ClosestMapAreaConnection` step to minimize chances of issues that cause connections to cut through other areas
    - Uses more accurate description of areas in connection point selection
    - Uses the same `ConnectionPointSelector` used to determine points to connect for determining the distance between two areas; thus allowing more control over how distance is calculated.

### Fixed
- Incorrect nullable annotation for `Map.GoRogueComponents` (#219)
- `DungeonMazeGeneration` returning rooms with unrecorded doors (#217)
