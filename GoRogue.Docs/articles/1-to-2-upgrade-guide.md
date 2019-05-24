---
title: 1.x to 2.0 Upgrade Guide
---

# 1.x To 2.0 Upgrade Guide
This article is intended to be a useful resource for those that are familiar with GoRogue 1.8.2 (the last 1.x release), and are looking to upgrade to 2.0.

# Breaking Changes
The following are changes to the way things function that may break existing code.  Completely new features, unless they pertain to the discussion, are not covered in this section.

## Coord is a Value Type
`Coord` has been changed from a reference type (C# class) to a value type (C# struct).  That produces the following breaking changes:
1. The `Coord.Get` function no longer exists; you should just use `new`, eg. `Coord c = new Coord(1, 2)`.  There are some syntactic shortcuts for this covered in the New Features section.
2. `Coord` is no longer nullable, so if you need to assign a "None" value, you'll need to use `Coord?`, or use the new `Coord.NONE` static field, which is set to (x, y) values `(int.MinValue, int.MinValue)`.  This also means a `Coord` with those coordinates is technically indistinguishable from `NONE` to GoRogue functions.  Basically, replace `Coord c = null` with `Coord c = Coord.NONE`, or `Coord? c = null` if you absolutely need nullable types.
3. Generally minor: If you are exposing `Coord` instances as properties (not fields), the change to value type means there is very likely some unintended copying going on when you access that property.  Consider making the property of type `ref Coord` or `ref readonly Coord` if that copy absolutely matters.  If you're not sure about that C# feature set, or don't know whether its necessary, generally speaking just leave it -- the type is so small it really doesn't matter most of the time, though it _can_ have performance implications in that 1% of cases.

## GameObject/Map Changes
This system has changed its structure significantly.
1. `GameObject` and `Map` no longer take template parameters.  A GoRogue 1.0 implementation of:
```CSharp
class MyGameObject : GameObject<MyGameObject> { ... }
// Stored in instance of Map<MyGameObject>
```
in 2.0 becomes simply:
```CSharp
class MyGameObject : GameObject { ... }
// Store in instance of Map
```

This also means that all functions of `Map` that return objects from the map (`GetObjects`, etc) now return objects of base type `IGameObject` (this is functionally equivalent to `GameObject` in interface; see number 2 below).  If you need them as your derived type, each function that returns an object also provides a templated overload that automatically performs the cast, and returns the object if and only if it is castable to the type you give it.  So, if you have `foreach (var mObject in myMap.GetObjects(myPos))` in 1.0, and you are OK with `mObject` being of type `IGameObject` instead of `WhateverMyDerivedTypeIs`, it can stay as it is.  If you need access to things from the derived type, this line becomes `foreach (var mObject in myMap.GetObjects<WhateverMyDerivedTypeIs>(myPos))`.  This also has the advantage that this will work even if not _all_ the objects at `myPos` are of type `WhateverMyDerivedTypeIs`.  If that's the case, you get only the ones that can successfully cast to `WhateverMyDerivedTypeIs`.

2. `GameObject` now has its entire interface implemented as `IGameObject`, and `Map` holds and returns `IGameObject` instances from functions.  Because every bit of the public interface was pulled from `GameObject` to `IGameObject`, this shouldn't break code in itself, except for explicit type declarations, eg. `foreach (GameObject myObj in myMap.GetObjects(myPos))` must now be `foreach (IGameObject myObj in myMap.GetObjects(myPos))`.  This interface change also may make it _much_, _much_ easier to implement the `GameObject`/`Map` system in an environment where your game's basic map object type needs to inherit from some type other than `GameObject`.  This is covered in detail in the New Features section.

3. Finally, `GameObject` now takes an extra constructor argument -- its "parent" object.  This argument will be `this`, in cases where you are either inheriting from `GameObject`, or using a `GameObject` as a private backing field to implement `IGameObject` (as is suggested in the New Features section).  If you are creating plain old base `GameObject` instances and adding those to a `Map`, then the parameter will be `null`.  If this argument is configured improperly, an `Exception` will be raised as soon as you attempt to add the `GameObject` to a `Map`.

## FOV Accepts Only Boolean Inputs
Previously, `FOV` constructors accepted either `IMapView<bool>`, where `true` indicated a tile was see-through and `false` indicated a sight-blocking tile, or an `IMapView<double>` where the definitions of values were the same as a resistance map for `SenseMap`.  In 2.0, `FOV` accepts only `IMapView<bool>`.  Because the `SenseMap`'s definition of a resistance map has been expanded (see new New Features section for details), it no longer makes sense to provide this interpretation in `FOV`, and in most cases was excessive anyway.  If you absolutely must use GoRogue 1.0-style double values for the input data, you can hand `FOV` a `LambdaTranslationMap` that performs the translation:
```CSharp
// This is something you could hand FOV before, but now can't
var oldResistanceMap = new ArrayMap<double>(10, 10);

var fovMap = new LambdaTranslationMap<double, bool>(oldResistanceMap, d => d < 1.0 : true : false);
var fov = new FOV(fovMap);
```

## Rectangle Creation/Modification Functions Refactored
A number of `Rectangle` functions changed names:
```
Rectangle.CreateWithExtents => Rectangle.WithExtents
Rectangle.CreateWithRadius => Rectangle.WithRadius
Rectangle.CreateWithSize => Rectangle.WithPositionAndSize
Rectangle.CenterOn => Rectangle.WithCenter
Rectangle.Move => Rectangle.WithPosition
Rectangle.MoveX => Rectangle.WithX
Rectangle.MoveY => Rectangle.WithY
Rectangle.SetHeight => Rectangle.WithHeight
Rectangle.SetMaxExtent => Rectangle.WithMaxExtent
Rectangle.SetMaxExtentX => Rectangle.WithMaxExtentX
Rectangle.SetMaxExtentY = Rectangle.WithMaxExtentY
Rectangle.SetMinExtent => Rectangle.WithMinExtent
Rectangle.SetMaxExtentX => Rectangle.WithMinExtentX
Rectangle.SetMaxExtentY = Rectangle.WithMinExtentY
Rectangle.SetSize => Rectangle.WithSize
Rectangle.SetWidth => Rectangle.WithWidth
Rectangle.MoveIn => Rectangle.Translate
```

## BasicRoomsGenerator Returns Rectangles Representing Interior of Rooms
This one is straightforward -- `BasicRoomsGenerator` and `QuickGenerators.GenerateRandomRoomsMap` used to return an `IEnumerable<Rectangle>` where the `Rectangle` instances represented the rooms created, including their walls all around the outside.  The `Rectangle` instances returned now represent only the floor area of each room.  This means it now matches the convention used by `RoomsGenerator`, for maximum compatibility.  For code accessing these return values, if you need the room including walls, simply expand the rectangle by one on each side -- there's a built in function for that.  For example: `var roomIncludingWalls = returnedRect.Expand(1, 1);`.

## IMapView Now Requires Implementation of 1D Indexer
This is also straightforward -- `IMapView` and `ISettableMapView` now require the implementation of a `this` indexer that takes a 1D array index and returns the value.  Thus, you will need to add such a function to any custom `IMapView` or `ISettableMapView` implementation you have. 
 Because GoRogue offers functions to convert 2D to 1D array indexes, this should be straightforward:
```CSharp
class MyMapView : IMapView<bool> {
    /* Existing functions from implementation using GoRogue 1.0 here */
    public bool this[int array1DIndex] => this[Coord.ToCoord(array1DIndex, Width)];
}
```
Unfortunately, since default interface implementations do not exist in C# until C# 8.0, and extension methods cannot be indexers, this functionality cannot be provided automatically.  However, all built in objects implementing `IMapView` (`FOV`, `SenseMap`, along with everything in the `GoRogue.MapViews` namespace) now possess this indexer, and can thus be accessed like both 1D arrays and 2D arrays.

# New Features
The following are completely new features in GoRogue 2.0.0.  They may change the preferred way of doing things, but in and of themselves will not break existing code that uses GoRogue 1.8.2 (the last 1.x release).

## Coord Syntactic Sugar
Since `Coord` is a value type now, GoRogue now provides a bunch of syntactic sugar to make them easier to deal with:
1. `Coords` are implicitly convertible to value-tuples of two ints, and tuples are implicitly convertible to `Coord` instances.  So, when the type is obvious, say because of an explicit type declaration or a function definition, we can omit `new Coord` part of the syntax involved in creating one:
```CSharp
Coord c = (1, 2);
// Assuming I have a function Process(Coord position) that I can call
Process((5, 6));
```

2. `Coords` also offer a deconstructor now, which can simplify code that intends to use a `Coord`'s x/y values as local parameters:
```CSharp
// Assuming myEntity has a Position field that is of type Coord
var (x, y) = myEntity.Position;
// Do processing on x and y as local variables.  Example is printing to keep it succinct
Console.WriteLine("Printing X: " + x); 
```

## Rectangle Syntactic Sugar
Similarly to `Coord`, `Rectangle` also now offers implicit conversions from tuple to `Rectangle` and back, and a deconstructor.

1. We can omit the `new Rectangle` part of creating a rectangle when the type is obvious:
```CSharp
Rectangle rect = (1, 2, 5, 10); // Rectangle with pos (1, 2), width/height of 5/10
// Assuming we have a function Process(Rectangle rect) we can call
Process((1, 2, 10, 15));

// Currently we can't do Rectangle rect = (minExtent, maxExtent), nor
// Rectangle rect = (center, horizontalRadius, verticalRadius).
// These will be added in later releases.
```

2. We can also deconstruct into local variables, as we can with `Coord`:
```CSharp
// Assuming myMap as a property BoundingBox of type Rectangle
var (x, y, width, height) = myMap.BoundingBox;
// Do processing on x, y, width, and height as local variables.  Example is printing to keep it succinct
Console.WriteLine("Printing X: " + x); 
```

## Primitive-Type Implicit Conversion to/from other Common Libraries
GoRogue now allows its primitive types to be implicitly converted to and from equivalent types from (some) other libraries.  Currently, GoRogue `Coord`/`Rectangle` are implicitly convertible to and from both MonoGame's equivalent types, and equivalent types from the `System.Drawing` namespace.  Long story short, if you have a function or variable, either in your own code or in some other library that you're using, that takes MonoGame's `Point`, `System.Drawing.Point`, or `System.Drawing.Size`, feel free to pass it a GoRogue `Coord` and it will just work (and vice versa!).  The same goes for `GoRogue.Rectangle` and MonoGame's `Rectangle`, or System.Drawing's `Rectangle`.

In the case of `System.Drawing`, you can also pass `GoRogue.Coord` to a function or variable taking `System.Drawing.PointF`, but NOT the other way around (since the floating point values would be truncated).  The same goes for `GoRogue.Rectangle` and `System.Drawing.RectangleF`.

Similarly, you may add values of type `Coord` to values of Monogame's `Point` type, or `System.Drawing.Point` (and vice versa).  As well, you may add a `GoRogue.Direction` instance to an instance of MonoGame's `Point` or `System.Drawing.Point`, just like you could with `Coord`.

Even in the case of the `MonoGame` conversions/operators, GoRogue 2.0 DOES NOT depend on MonoGame in any way.  The conversions simply work if any appropriate `MonoGame` assembly is present, and affect nothing if no such assembly is present in a project.

### A Code Example
If using GoRogue in a MonoGame project, we can convert implicitly to/from MonoGame Point/Rectangle:
```CSharp
Microsoft.Xna.Point p = new Point(1, 2);
p = new GoRogue.Coord(1, 5); // This works fine.
p += new GoRogue.Coord(1, 1); // As does this, where we are adding a value of type Coord to a value of type Point
// Assuming we have a function Process(Microsoft.Xna.Point p) we can call
Process(new GoRogue.Coord(1, 2));

// Assuming myEntity has a Position property of type Microsoft.Xna.Point.
myEntity.Position = new GoRogue.Coord(3, 5);
myEntity.Position += GoRogue.Direction.UP;

ArrayMap<bool> map = new ArrayMap(10, 10);
map[new Microsoft.Xna.Point(1, 3)] = true; // This indexer takes Coord, but we can give it Point
```

## SourceLink and Debugging Symbols Now Available
Starting in GoRogue 2.0, the library supports [SourceLink](https://github.com/dotnet/sourcelink).  This functionality allows you to step into the source of GoRogue while using the debugger, just as if it was your own code.  As part of this functionality, GoRogue now also distributes debugging symbols packages with each release, in the form of _.snuget_ packages.

The use of these packages requires Visual Studio 2017 version 15.9 or greater, and also requries that you add a symbols source to your Visual Studio debugging settings.  See the "Consume snupkg from NuGet.org in Visual Studio" section of [this webpage](https://blog.nuget.org/20181116/Improved-debugging-experience-with-the-NuGet-org-symbol-server-and-snupkg.html) for instructions on this process.  As well, ensure that in your debugging options (Tools->Options->Debugging) you have disabled "Just My Code", and enabled "SourceLink Support".  Once these steps have been completed, the appropriate symbols and source files will be downloaded automatically as soon as you try to step into a GoRogue method.

## Debug Build of GoRogue Now Available
The addition of SourceLink and symbols package support in GoRogue 2.0 can make debugging code much easier.  However, since the default GoRogue package for each version is still a "Release" build, it can still be challenging to debug code involving GoRogue function calls, as optimizations that occur during the release build process can limit the usefulness of debugging symbols.  Thus, starting with GoRogue 2.0, with each version of GoRogue, a "Debug" build is also provided. The debug build is categorized as a prerelease by nuget, so you will need to enable prereleases to see it.  Once you do so, if you look at versions of GoRogue available, you will see two listings for each version -- x.y.z, which is the release build and x.y.z-debug, which is categorized as a prerelease, and is the debug build.  If you need to perform debugging involving stepping into GoRogue code, simply switch your package version to the "-debug" version corresponding to the GoRogue version you are using.  Then, you can switch back to the regular version when debugging is complete.

## IMapView and ISettableMapView Implementations and 1D Indexers
All `IMapView` and `ISettableMapView` implementations now have 1D-array style indexers, in addition to the 2d ones present previously.  This actually requires the addition of this indexer in custom implementations; see the appropriate portion of the Breaking Changes section for details.  However, this also means that all built in implementations also have these indexers:
```CSharp
ArrayMap<bool> boolMap = new ArrayMap<bool>(10, 10);
int index = Coord.ToIndex(2, 3, boolMap.Width); // 1D index for position (2, 3)
Console.WriteLine(boolMap[index]);

// This indexer is present in all IMapViews and ISettableMapViews, not just ArrayMap
LambdaTranslationMap<bool, int> intMap = new LambdaTranslationMap(boolMap, b => b ? 1 : 0);
Console.WriteLine(intMap[index]);
```

Built-in objects implementing `IMapView`/`ISettableMapView` include `FOV`, `SenseMap`, and everything in the `GoRogue.MapViews` namespace.

## IGameObject Interface
The `Map` class now holds objects of type `IGameObject`, rather than `GameObject`, while `GameObject` implements `IGameObject`.  This, along with the lack of the template parameters to `Map` and `GameObject`, creates breaking changes; see the appropriate portion of the Breaking Changes section for details.  However, while it doesn't force you to change an existing structure where you inherit from `GameObject`, this feature can make it much easier to integrate with object structures that make inheriting from `GameObject` difficult.

Before continuing, is worthy of note that, although it is possible, the intent is for you _never_ to need to create the code for implementing `IGameObject` functionality manually, as the necessary code can be non-trivial.  Instead, if you implement `IGameObject` on your object, use a private backing field of type `GameObject` to provide the actual implementations, and simply write (or let your IDE generate for you) functions that forward appropriate calls to that field's methods, as is shown in the example below.  `GameObject` is specifically designed to support this, and is implemented in such a way that it avoids many of the traditional issues you might encounter with implementations like this, namely references that point to the backing field when you expect them to point to the object.

Consider a case where you want to create a base map object type for your game, but you cannot or do not want to inherit from `GameObject`.  This could occur, for instance, because you need to inherit from some other class, as C# does not allow multiple inheritance.  SadConsole's `Entity` class, for instance, is a good example of this, or any other class you may want to create or use from an existing structure.  In this case, to use the `GameObject`/`Map` system in GoRogue 1.0, you would need to have either `GameObject`, or the other class that you wanted to inherit from, be a public "component" of your base object.  However, then you need to have a way to get back to the "parent" object from the component, and your architecture can get unnecessarily complicated.

In GoRogue 2.0, we can simply implement `IGameObject` instead, which allows you to inherit from whatever other _class_ you need to.  Then, to avoid providing your own implementations of `IGameObject` functions, you can simply implement `IGameObject` by forwarding its functions/properties to those of a (private) backing field of type `GameObject` -- something most IDEs can do for you automatically:

```CSharp
class MyBaseObject : ClassINeedToInheritFrom, IGameObject
{
    private IGameObject _backingField;

    public MyBaseObject(Coord pos, int layer, bool isStatic, bool isWalkable, bool isTransparent)
    {
        _backingField = new GameObject(pos, layer, this, isStatic, isWalkable, isTransparent)
    }
    /* Implements IGameObject by forwarding the function calls to the corresponding function in _backingField.
     * Many IDEs can generate these automatically. */
}
```

## Component System
GoRogue 2.0 now offers an extremely flexible, type-safe system that allows you to efficiently attach components to your objects.  It offers functions for attaching and removing components, retrieving components, and checking whether a given object has one or more components.  `GameObject` is set up to use this system automatically, however the components system itself can be easily interfaced and used with any object.

### Overview
The bulk of the code relevant to the component system is in the `GameFramework.ComponentContainer` class.  This class is a container for components, and contains all the functions for adding, removing, querying, and retrieving components.  To implement the component system in your own structure, you may simply give your object a public field of type `ComponentContainer`, and access that field whenever you do anything related to components.  However, if you wish to avoid having to access that field each time, you may also choose to inherit from `ComponentContainer`.

If you cannot inherit from `ComponentContainer`, the entire public interface of `ComponentContainer` is contained within the `IHasComponents` interface.  Thus, you may have your object implement `IHasComponents` via a private backing field of type `ComponentContainer`, exactly as can would do with `IGameObject` and `GameObject`.  These "forwarding functions" can be generated automatically by many IDEs, and in this way, your object has the component-related functions as direct members, so you don't have to access them through a field every time you want to use them.

```CSharp
class MyObject : IHasComponents
{
    private ComponentContainer _backingField;

    public MyObject() { _backingField = new ComponentContainer(); }
    
     /* Implements IHasComponents by forwarding the function calls to the corresponding function in _backingField.
     * Many IDEs can generate these automatically. */
}
```

The API documentation for `ComponentContainer` is relatively complete, and most of the functions are self-explanatory.  The system is extremely robust, as it requires no base class for components, and is capable of dealing with components with inheritance hierarchies and components that implement interfaces effectively.  For example, suppose you have the following class structure:
```CSharp
interface IZ {}
class A : IZ {}
class B : A {}
```
After you add a component of type `B` to an object `myObj`, `myObj.GetComponent<A>()`, `myObj.GetComponent<B>()`, and `myObj.GetComponent<IZ>()` all return the instance of `B` that you added.  The same applies to the `HasComponent` and `HasComponents` functions -- in the case of the previous example, `myObj.HasComponent(typeof(A))`, `myObj.HasComponents(typeof(A), typeof(B))`, and `myObj.HasComponent<IZ>()` also return true.

### Implementation in `GameObject`
The component system is already baked into a setup based on `IGameObject`/`GameObject`, as both `IGameObject` and `GameObject` implement `IHasComponent`.  Therefore, if you either inherit from `GameObject`, or use a private backing field of type `GameObject` to implement `IGameObject` on your own objects, your object will have the functions from `ComponentContainer` directly as class members (`AddComponent`, `RemoveComponent`, etc), just as if you had inherited from `ComponentContainer` directly.

Although these functions, like the ones that `ComponentContainer` possesses, will accept an object of any type as a component, you may optionally implement `IGameObjectComponent` on your component classes.  This interface contains a single field, `Parent`.  This field should not be assigned to manually.  Instead, when you call the `GameObject.AddComponent` or `GameObject.RemoveComponent(s)` functions, and pass them an object that implements `IGameObjectComponent`, this `Parent` field is updated automatically to point to the object it was added as a component to.  This applies even if you are using `GameObject` as a backing field to implement `IGameObject` on your own objects -- the `Parent` field will point to your object, not the backing field.  This can be useful if you need a component to know what it is attached to.

## Map Has Virtual CalculateFOV Functions
In GoRogue 1.0, the `Map` class had non-virtual `CalculateFOV` methods.  This combined with the fact that the `FOV` class itself does not have any events or overridable functions allowing you to respond to `FOV` calculation made it cumbersome to add functionality.  In GoRogue 2.0, the `Map.CalculateFOV` functions are virtual, allowing you to easily override their implementation and add functionality on top of the existing functions.

Only the `CalculateFOV` functions that accept `int x, int y` to indicate the starting position have been made virtual, not the ones that accept `Coord` as the starting position.  This is because the non-virtual functions call the virtual ones under the hood, so overriding all virtual `CalculateFOV` functions is sufficient to change the functionality of all of them.

## Map Creation Method
In GoRogue 1.0, `Map` provided two different constructors for map creation -- one that simply took width/height (along with the other optional parameters), and one that took `ISettableMapView<GameObject>`.  These constructors still exist in GoRogue 2.0, although the parameter of the latter has changed to `ISettableMapView<IGameObject>`.  That much said, the fact that the constructor must take `ISettableMapView<IGameObject>`, rather than `ISettableMapView<T>` where `T` is any type that _implements_ `IGameObject`, can be limiting and difficult to work around.  GoRogue 2.0's implicit conversion of `ArrayMap<T>` to `T[]` helps a little, because this type of polymorphism _is_ allowed with arrays, however this still has issues, and obviously this implicit conversion only applies if you are using `ArrayMap`.

Thus, to solve this issue, in GoRogue 2.0 a method is provided that takes `ISettableMapView<T>`, where `T` can be any type that implements `IGameObject`. This can be very useful for integrating with an inheritance tree, or other code that requires that your map objects inherit from a given class:

```CSharp
class BaseClass
{
    public int BaseInt;
    public BaseClass(int baseInt) { BaseInt = baseInt; }
}

class DerivedClass : BaseClass, IGameObject
{
    public int DerivedInt;
    public DerivedClass(int baseInt, int derivedInt) : base(baseInt) { DerivedInt = derivedInt; }
}

class Program
{
    public static void Main(string[] args)
    {
        ArrayMap<DerivedClass> terrain = new ArrayMap<DerivedClass>(10, 10);
        // We can't pass terrain to anything accepting `ISettableMapView<IGameObject>`, so we can't do
        // Map map = new Map(terrain).  But, since DerivedClass implements `IGameObject`, we can use 
        // the static CreateMap function:
        Map map = Map.CreateMap(terrain);
    }
}
```

This function is implemented in such a way that the `Map` returned gets the _actual_ data you gave it.  The data isn't first copied into a separate array.  in the example above, this means that if, after you create the `Map`, you assign some value in `terrain` a new value, eg. `terrain[1, 2] = new Tile(1, 2);`, that change is automatically reflected in the `Terrain` property of the `Map`, and vice versa.

Note that this functionality, like the functionality allowed by the constructor taking `ISettableMapView`, can be dangerous.  It is recommended that you _do not_ set the value of the original map view after creation, but rather use the `Map`'s `SetTerrain` function.  This function is responsible for ensuring the `ObjectAdded` and `ObjectRemoved` events are fired for you, so if you do not use the `SetTerrain` function these events will not be fired.  In addition, `SetTerrain` performs a number of checks for you, ensuring that you do not place a terrain tile in a way that violates walkability for example, or that you do not place a tile that violates the principal that all terrain must have a `Layer` value of 0.  If you set values to the `terrain` view you gave the `Map` after creation, these invariants aren't guaranteed, and it can lead to unexpected behavior.  The functionality is nonetheless allowed for maximum compatibility, and if you are careful to not violate the invariants enforced by `SetTerrain`, then it is functional.

In addition, if you use `SetTerrain` to set an `IGameObject` that is not castable to the type `T` of the map view you gave to `CreateMap`, then an `InvalidCastException` will be thrown.

## SenseSource now has Configurable (Arbitrary) Starting Intensity and Resistance Map Values
In GoRogue 1.0, sense maps had all sources starting at a value of 1.0 at their origin.  The resistance map values also varied between `0.0` and `1.0`.  In the case of sources using `RIPPLE` and variations, values between 0.0 and 1.0 mattered -- it indicated "partial" resistance to the source spreading.  When using `SHADOW` sources, any value less than 1.0 didn't block, and a value of 1.0 did block (since shadowcasting must use on-off blocking).

In GoRogue 2.0, you can think of this functionality as exactly the same, except the maximum "source intensity" of 1.0 can now be an arbitrary value.  `SenseSource` instances now have an `Intensity` property, which defaults to 1.0 if unspecified, that determines the starting intensity.  The resistance maps, then, can have any positive double value for a given square.

In the case of sources that use `RIPPLE` and variations, the resistance value is, like before, subtracted from the source value as it spreads.  So,  a source with an `Intensity` of 2.5 can potentially go through two cells where the resistance is 1.0 (although its intensity, and thus its spreading distance in that direction, would be significantly reduced), but a third cell with that resistance value would block it completely.

In the case of sources using `SHADOW`, any cell with a resistance value greater than or equal to the source's starting intensity will block the source, and any value less than the source's starting intensity will not block it.

## ArrayMap Implicit Conversion to 1D Array
In order to increase ease of use with libraries that use traditional data structures, GoRogue's `ArrayMap` is now implictly convertible to a 1-dimensional array.  The values are not copied into a new array when this is done -- any modification of the array will also modify the `ArrayMap`.  If you already have a 1D array, because the width is needed to convert it to an array map, it will not implicitly convert.  However, there is a constructor of `ArrayMap` that takes a 1D array.

```CSharp
ArrayMap<bool> map = new ArrayMap<bool>(10, 10);
bool[] array = map;

// Assuming we have a function Process(bool[] array1D, int width)
Process(map, map.Width);

bool[] otherArray = new bool[10*15];
ArrayMap<bool> otherMap = new ArrayMap<bool>(otherArray, 10);
```

## Addition of ArrayMap2D
`ArrayMap` was sufficient for all cases where you needed an array-like structure in GoRogue 1.0.  In GoRogue 2.0, however, `ArrayMap` implicitly converts to a 1D array.  This is useful for cases where you want to interact with code that expects 1D arrays, however it may also be useful to have the capability for implicit conversions to 2D arrays.  Unfortunately, it is not possible to have `ArrayMap` convert to both 1D array and 2D array, and still guarantee that changes made to the resulting arrays also show up in the `ArrayMap`.  Thus, for cases where you need to interact with 2D arrays, `ArrayMap2D` has been added.  It is identical to `ArrayMap`, except that it implicitly converts to _2D_ arrays, and the overload of its constructor that takes an array takes a _2D_ array:

```CSharp
ArrayMap2D<bool> map = new ArrayMap2D<bool>(10, 10);
bool[,] array = map;

// Assuming we have a function Process(bool[,] array)
Process(map);

bool[,] otherArray = new bool[10, 10];
ArrayMap2D<bool> otherMap = new ArrayMap2D<bool>(otherArray);
```

There is no difference between `ArrayMap` and `ArrayMap2D`, other than the underlying data structure used and thus the types it implicitly converts to.  Thus, if you aren't interacting with code that expects any sort of actual array, feel free to just use `ArrayMap`.

## Addition of UnboundedViewport
In GoRogue 1.0, there was the `Viewport` class, which is an `IMapView` implementation that exposed a section of the underlying map view.  The area of the map exposed was automatically bounded to the edges of the underlying map, so that no part of the exposed rectangle could ever be "off the map".

This is useful in many situations, however in other cases the bounding behavior is undesireable, and you would rather it just return a default value for any position out of range of the underlying map.  This is precisely what `UnboundedViewport` does:

```CSharp
ArrayMap<bool> map = new ArrayMap<bool>(100, 100);
map[0, 0] = true;
// False will be returned for any value accessed that's outside the actual map.
// If this value isn't specified, it defaults to default(T)
UnboundedViewport<bool> viewport = new UnboundedViewport<bool>(map, new Rectangle(-4, -4, 10, 10), false);

// Accesses (-4, -4) of the underlying map, which doesn't exist, so we get false
Console.WriteLine(viewport[0, 0]);
// Accesses 0, 0 of the underlying map, so we get true
Console.WriteLine(viewport[4, 4]);
```

This might be useful, for instance, if you are creating a system wherein you have one `FOV` instance per monster, and want to minimize the amount of memory used for each instance.  You would simply hand each `FOV` instance an `UnboundedViewport<bool>` where the area is restricted to a fov-radius sized-area around the corresponding monster.  If you did this with `Viewport`, the `FOV`s wouldn't follow their respective monsters properly to the edge of the map.