# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## [Unreleased]

### Added
- FOV now has `CalculateAppend` functions which calculate FOV from a given source, but add that FOV into the existing one, as opposed to replacing it
- FOV now also has a `Reset` function and a `VisibilityReset` event that may be useful in synchronizing visibility with FOV state

### Changed
- `IFOV` interface (and all implementations) now takes angles on a scale where 0 points up, and angles proceed counter-clockwise
    - This matches better with bearing-finding functions in primitives library and correctly uses the common compass clockwise rotation scale

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
