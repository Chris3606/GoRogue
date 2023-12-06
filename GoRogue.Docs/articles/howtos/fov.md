---
title: Field of View
---

# Field of View
GoRogue offers algorithms for calculating [field of view](https://www.roguebasin.com/index.php/Field_of_Vision) (also known as field of vision or FOV).  GoRogue provides complete implementations of algorithms which allow you to calculate this, and also provides an abstraction which allows you to implement your own algorithms.

FOV algorithms, like most algorithms in GoRogue, depend on [grid views](~/articles/howtos/grid-view-concepts.md) to take input and produce output.  Grid views are an abstraction over a 2D grid which is explained in detail at the article linked above; it is recommended that you review this article first if you are unfamiliar with grid views.  The code examples here will gloss over the details of grid views, and will instead focus on FOV itself.

Also note that the code examples in this article will assume the following "using" statements are within scope:
[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#Usings)]

>[!NOTE]
> If you are using classes from the `GoRogue.GameFramework` namespace, please note that `Map` has a built-in property called `PlayerFOV`.  This field uses the FOV abstraction discussed here, and will by default be set to an FOV implementation which gets its grid data based on the objects on the map.  When using `GoRogue.GameFramework`, this article will be helpful in terms of teaching you how FOV works, but note that you do not have to create the instance yourself unless you want to use a different algorithm.

## Using Included FOV Algorithms
The simplest way to use FOV in GoRogue is to use one of the built-in algorithms provided in the [`GoRogue.FOV` namespace](xref:GoRogue.FOV).  At construction, FOV algorithms typically take, as input, an `IGridView<bool>`.  This grid view represents the "map" to the algorithm.  Each location in the grid view should have a value of "true" if the field of view algorithm should consider it transparent, and a value of "false" if that location is opaque.

Basic usage after creation involves calling the `Calculate()` function with the appropriate parameters.  There are then a number of ways to access to results.

>[!TIP]
>Generally, you should avoid creating a new instance of an FOV algorithm each time you need to re-calculate it.  When created, FOV algorithms (and many GoRogue algorithms in general) tend to allocate a fairly substantial amount of memory.  This is by design; GoRogue allocates this memory as the instance is created, but in exchange, the amount of memory allocated when the `Calculate` function is called is minimized.  Therefore, to achieve the intended performance, **you should re-use the same instance where possible**.

What you provide as the grid view will vary based on your actual map structure.  The [grid view documentation](~/articles/howtos/grid-view-concepts.md) discusses grid views in detail; so you will need to select and/or implement an appropriate grid view as outlined there.  For these examples, we will assume a very simple map structure, with an array of "Tile" structures representing the attributes of each piece of terrain.  We will base our FOV grid view on attributes of the Tile structure appropriate for the given location.  The following code example creates simple class structure which implements the above concept and uses a `LambdaTranslationGridView` to provide an appropriate grid view to an FOV instance.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#MapStructure)]

This code will use a [RecursiveShadowcastingFOV](xref:GoRogue.FOV.RecursiveShadowcastingFOV) instance as the FOV implementation.  This class implements a [recursive shadow-casting algorithm](https://www.roguebasin.com/index.php/FOV_using_recursive_shadowcasting) which is very fast and offers a wide range of features.  There are a couple notable features that this algorithm does _not_ support, however:

1. The resulting FOV is not guaranteed to be "symmetrical".  There is an issue open to both add a symmetrical option to this algorithm, as well as some implementations of other symmetric algorithms [here](https://github.com/Chris3606/GoRogue/issues/195).

2. The algorithm will consider single-cell thick diagonal walls as opaque.  Other algorithms will be implemented to mitigate this (tracking issue [here](https://github.com/Chris3606/GoRogue/issues/229)).

>[!NOTE]
>For information on FOV algorithms included with GoRogue, you should look at the [`GoRogue.FOV` API documentation](xref:GoRogue.FOV); the documentation for each class will describe the differences in the implementations.  Examples in this article will continue to use `RecursiveShadowcastingFOV`, but the API will be the same regardless of which implementation you choose.

Now, we will create a `Map` instance which represents a simple map with walls (opaque, non-walkable cells) on all the outer edges, and floors (transparent, walkable ones) everywhere else:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#MapCreation)]

Don't worry about understanding the map generation code; we're just using it to create a non-trivial Map structure to demonstrate FOV with.  The map generation system is covered in more detail in [its own article](~/articles/howtos/map-generation.md).  For reference, the output printed from the above is:

```
Map:
# # # # # # # # # # #
# . . . . . . . . . #
# . . . . . . . . . #
# . . . . . . . . . #
# . . . . . . . . . #
# . . . . . . . . . #
# . . . . . . . . . #
# . . . . . . . . . #
# . . . . . . . . . #
# . . . . . . . . . #
# # # # # # # # # # #
```

Now, we can use the FOV algorithm to calculate the field of view.  We specify the origin point of the FOV, the maximum radius, and the shape to use:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#CalculateFOV)]

### Accessing Results as Boolean Values
From here, there are several ways in which we can access the results of the calculation.  The easiest is by using the [BooleanResultView](xref:GoRogue.FOV.IReadOnlyFOV.BooleanResultView) property.  This property is a grid view which has a value of "true" for a given location if that location is within the visible field of view, and false otherwise.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#BooleanResultView)]

This code produces the following output:

```
BooleanResultView FOV:
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
```

Note that all values are "true" in the result; this is because all of the walls are visible since there is nothing between them and the origin point.

Now, let's introduce a location in the middle that blocks line of sight, and re-calculate the FOV:
[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#IntroduceBlockingAndRecalc)]

In the output this time, we can see that cells behind the location which is opaque are no longer visible:

```
FOV with blocking object:
* * - - - - - - - * *
* * * - - - - - * * *
* * * * - - - * * * *
* * * * * - * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
* * * * * * * * * * *
```

### Restricting FOV to a Cone
GoRogue's FOV algorithms also support restricting the calculated field of view to a cone:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#AngleRestrictedFOV)]

Here, we can see that only cells which are visible and within the cone return true:

```k
FOV Restricted to Cone:
- - - - - - - - - - -
- - - - - - - - - - -
- - - - - - - - - - -
- - - - - - - - - * *
- - - - - - - * * * *
- - - - - * * * * * *
- - - - - - - * * * *
- - - - - - - - - * *
- - - - - - - - - - -
- - - - - - - - - - -
- - - - - - - - - - -
```

### Other Ways of Accessing Results
There are also several other forms of data about the result which are accessible.

First, there is a [DoubleResultView](xref:GoRogue.FOV.IReadOnlyFOV.DoubleResultView) property.  This is a grid view of double values, which returns a value of 0.0 for locations outside the field of view.  For locations inside FOV, it returns 1.0 for the origin, and a value in the range (0.0, 1.0) for other locations, which is greater for values close to the origin, and smaller for values farther away.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#DoubleResultView)]

```k
DoubleResultView FOV:
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.33 0.17
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.67 0.50 0.33 0.17
0.00 0.00 0.00 0.00 0.00 1.00 0.83 0.67 0.50 0.33 0.17
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.67 0.50 0.33 0.17
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.33 0.17
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00
0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00 0.00
```

This can be useful, for example, if you wish to "dim" cells which are further away from the origin; you can simply use this value as a brightness multiplier for a cell's true color.

There are also a number of very useful "sets" of points which represent data about the result.  These include [NewlySeen](xref:GoRogue.FOV.IReadOnlyFOV.NewlySeen) and [NewlyUnseen](xref:GoRogue.FOV.IReadOnlyFOV.NewlyUnseen).  `NewlySeen` contains any cells which are visible currently but were _not_ visible after the previous call to `Calculate()` had completed.   Conversely, `NewlyUnseen` contains cells that are _not_ visible now which _were_ visible when the previous call to `Calculate()` finished:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#NewlySeenAndUnseen)]

As you can see, `NewlySeen` contains cells which face downward, and `NewlyUnseen` contains cells facing to the right; cells which have not changed (including the FOV origin point of (5, 5)) are not in either set.

```k
FOV After recalculate:
- - - - - - - - - - -
- - - - - - - - - - -
- - - - - - - - - - -
- - - - - - - - - - -
- - - - - - - - - - -
- - - - - * - - - - -
- - - - - * - - - - -
- - - - * * * - - - -
- - - - * * * - - - -
- - - * * * * * - - -
- - - * * * * * - - -

Newly seen: [(5,6), (6,7), (5,7), (6,8), (5,8), (7,9), (6,9), (5,9), (7,10), (6,10), (5,10), (4,7), (4,8), (3,9), (4,9), (3,10), (4,10)]
Newly unseen: [(6,5), (7,4), (7,5), (8,4), (8,5), (9,3), (9,4), (9,5), (10,3), (10,4), (10,5), (7,6), (8,6), (9,7), (9,6), (10,7), (10,6)]
```

These properties are useful for updating the visibility or attributes of only cells which have changed state.  For example, if you wanted to ensure that cells outside an FOV were invisible and cells inside were visible, you might iterate over `NewlyUnseen` and set the visibility to `false`, then iterate over `NewlySeen` and set the visibility to true.

There is also the [CurrentFOV](xref:GoRogue.FOV.IReadOnlyFOV.CurrentFOV) property, which simply enumerates all of the points currently in FOV:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#CurrentFOV)]

In this case, it includes all cells in the downward-facing cone, including the origin:

```k
Current FOV: [(5,5), (5,6), (6,7), (5,7), (6,8), (5,8), (7,9), (6,9), (5,9), (7,10), (6,10), (5,10), (4,7), (4,8), (3,9), (4,9), (3,10), (4,10)]
```

### Appending Multiple FOV Calculations
The code above uses the `Calculate()` function to calculate the FOV.  As shown, when `Calculate()` is called, the current result data is overridden with the results of the new calculation.  For some use cases, however, you may want to add onto the existing result data, rather than overriding it.  One use case for this might be to implement something that gives the player temporary vision on a part of the map which they don't currently occupy.  To support this, GoRogue's FOV interface provides the `CalculateAppend()` methods.

>[!TIP]
>If you are trying to implement something akin to multiple "light sources", [sense maps](~/articles/howtos/sense-maps.md) may be a better choice than using an FOV algorithm and CalculateAppend.

The `CalculateAppend()` methods work exactly like the `Calculate()` methods, except for the current result data is not erased before the calculation is performed:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/FOV.cs#CalculateAppend)]

This code produces the following output.  Note that the FOV includes cells seen by the call to Calculate in the previous section (the downward facing cone), as well as the cells seen by the call to `CalculateAppend` above (the leftward-facing cone).

```k
FOV After append:
- - - - - - - - - - -
- - - - - - - - - - -
- - - - - - - - - - -
* * - - - - - - - - -
* * * * - - - - - - -
* * * * * * - - - - -
* * * * - * - - - - -
* * - - * * * - - - -
- - - - * * * - - - -
- - - * * * * * - - -
- - - * * * * * - - -
Newly seen: [(5,6), (6,7), (5,7), (6,8), (5,8), (7,9), (6,9), (5,9), (7,10), (6,10), (5,10), (4,7), (4,8), (3,9), (
4,9), (3,10), (4,10), (4,5), (3,4), (3,5), (2,4), (2,5), (1,3), (1,4), (1,5), (0,3), (0,4), (0,5), (3,6), (2,6), (1
,7), (1,6), (0,7), (0,6)]
Newly unseen: [(6,5), (7,4), (7,5), (8,4), (8,5), (9,3), (9,4), (9,5), (10,3), (10,4), (10,5), (7,6), (8,6), (9,7),
 (9,6), (10,7), (10,6)]
```

### Resetting FOV Data
Note again in the output above that the `NewlySeen` property reflects locations newly seen via the last call to `Calculate` made in the previous example, _as well_ as any cells newly seen in the call to `CalculateAppend`.  More precisely, `NewlySeen` (and `NewlyUnseen`) reflect state changes since the last time `Reset()` was called.  We haven't talked about the `Reset()` function explicitly yet; but you can simply think of it as "resetting" the result data such that all locations are considered _outside_ the FOV (eg setting it such that nothing is visible).

The `Calculate()` function, therefore, can really be thought of as a function that first calls `Reset()`, then calls `CalculateAppend()` with the parameters it receives.  You are free to call `Reset()` yourself if you need to; however remember that `Calculate()` by its definition, calls `Reset()`; so if you are using the `Calculate()` function, you probably will not need to manually call `Reset()`.

### Other Useful Functionality
There are a few other useful properties and events which have not been mentioned.   Notably, FOV implementations have a [Recalculated](xref:GoRogue.FOV.IFOV.Recalculated) event, which is fired whenever a calculation (appended or otherwise) is performed.  This provides an easy way to automatically react when the FOV changes.  There is also a [VisibilityReset](xref:GoRogue.FOV.IFOV.VisibilityReset) event, which fires whenever `Reset()` (or `Calculate()`) is called; eg. whenever the current FOV data is erased.

FOV implementations also contain a [CalculationsPerformed](xref:GoRogue.FOV.IReadOnlyFOV.CalculationsPerformed) property.  This is a list of structures which each contain the parameters passed to a `Calculate` or `CalculateAppend` call after the last time `Reset()` was called.  Functionally, this acts as a record of the data regarding the FOV calculations which have been performed since the last reset.  This can be useful, for example, if you would like to know the center point of each FOV.

## Creating Custom FOV Algorithms
In some cases, you may want to implement your own field of view algorithms.  GoRogue allows you to do this by implementing the interfaces which constitute GoRogue's FOV abstraction.  Although you can always implement a field of view algorithm independent of GoRogue's abstraction, implementing GoRogue's interfaces will allow you switch GoRogue's algorithms out for your own without changing the API, and will also allow you to use your algorithm with things like `GoRogue.GameFramework.Map` which have properties that utilize the interfaces.

Implementing GoRogue's FOV abstraction simply entails implementing the [`IFOV` interface](xref:GoRogue.FOV.IFOV).  Its properties and methods are well-documented, and largely consist of the properties outlined in previous sections.  However, there are also two a number of abstract classes you can inherit from which implement some of the common boilerplate used in `IFOV` implementations.

### FOVBase
The most basic abstract class is [`FOVBase`](xref:GoRogue.FOV.FOVBase).  This class implements the calculation function overloads of the `IFOV` interface in terms of a minimal set of functions which the user must implement.  The actual calculation is implemented via two functions: `OnCalculate` and `OnReset`.  `OnCalculate` is a function that should append an FOV with the given parameters onto the current result views; `OnReset` should reset the result views to their "nothing visible" value.  It is highly recommended that you read both the "Remarks" section of the `FOVBase` class documentation, as well as the documentation for the abstract functions; it describes what each function should do in detail.

### Other Base Classes
`FOVBase` is generally the most useful for advanced cases where you need a large amount of control over property implementations.  For straightforward algorithms, there are two other abstract classes provided which may be of use.  These are [`DoubleBasedFOVBase`](xref:GoRogue.FOV.DoubleBasedFOVBase) and [`BooleanBasedFOVBase`](xref:GoRogue.FOV.BooleanBasedFOVBase).  These classes inherit from `FOVBase` and make it easier to implement the `DoubleResultView` and `BooleanResultView` properties.  Each one of these classes has a protected `ResultView` property, which is an `ISettableGridView` representing the actual data the algorithm works with; the `DoubleResultView` and `BooleanResultView` properties are automatically implemented based on the `ResultView` property.

In the case of `DoubleBasedFOVBase`, [`ResultView`](xref:GoRogue.FOV.DoubleBasedFOVBase.ResultView) is an `ISettableGridView<double>`, where the values should be set by the same definition as `DoubleResultView`.  As you might expect, `DoubleResultView` will simply be defined by exposing `ResultView`; `BooleanResultView` is defined as `true` for any location in the `ResultView` whose value is greater than 0.

`BooleanBasedFOVBase` instead defines [`ResultView`](xref:GoRogue.FOV.BooleanBasedFOVBase.ResultView) as an `ISettableGridView<bool>`, where the values should be set by the same definition as `BooleanResultView`.  In this case, `BooleanResultView` will simply be defined by exposing `ResultView`.  `DoubleResultView` is defined as 0 for any locations for which `ResultView` is `false`; for locations for which `ResultView` is `true`, a value fitting the definition is calculated based on the location's distance from the origin points of calculations which have happened since the last call to `Reset()`.

### Use Cases for Base Classes
Despite appearing more complicated, `DoubleBasedFOVBase` is more often than not a good "default" choice for base class when creating new algorithms.  Many FOV algorithms will do some sort of distance calculation during propagation anyway to implement FOV radius; so it's easy to just record the proper double value in the `ResultView` based on that distance calculation.  The formula is relatively straightforward; given an FOV maximum radius, the decay of the double values per unit of distance from the center can be calculated as follows:

```CS
var decayPerUnit = 1.0 / (radius + 1);
```

Therefore, given a cell which is known to be within the FOV, and the center point and radius of that FOV, the proper double value could be calculated like this:

```CS
var doubleValue = 1.0 - decayPerUnit * distanceMethod.Calculate(center, currentLocation);
```

`BooleanBasedFOVBase` should generally be considered more niche.  Although it is true that traditional views of FOV algorithms typically consider each location as having a boolean value, it is much more difficult for `BooleanBasedFOVBase` to implement `DoubleResultView` than it is for `DoubleBasedFOVBase` to implement `BooleanResultView`.  Keep in mind that GoRogue's FOV abstraction supports appending multiple FOV calculations onto a single result view; and that those FOVs, in theory, could overlap.  Therefore, in the case of locations where the `ResultView` value is `true`, `BooleanBasedFOVBase` has to iterate through all recorded FOV calculations in order to retrieve a double value which is guaranteed to account for the closest FOV.  Although `BooleanBasedFOVBase` takes care of this implementation for you, it comes with a performance penalty.

There are some benefits to using `BooleanResultView`, however.  The first is that the memory consumption of the class instance is lower.  `BooleanBasedFOVBase.ResultView` can be implemented as `BitArrayView`, which is much smaller than an array of doubles.  Additionally, accessing values of `BooleanResultView` is also naturally faster when using `BooleanBasedFOVBase`.  Therefore, use cases where `BooleanBasedFOVBase` might be a good choice include cases where usages of the algorithm typically revolve around exclusively using `BooleanResultView`, or where `CalculateAppend` is not used and therefore there is only a single FOV (which makes accessing values in `DoubleResultView` much faster).

Algorithms provided by GoRogue typically provide one implementation based on `DoubleBasedFOVBase` which is designed for general use, and a separate implementation of the same algorithm based on `BooleanBasedFOVBase`.  A good example is [RecursiveShadowcastingFOV](xref:GoRogue.FOV.RecursiveShadowcastingFOV), which is a recursive shadowcasting implementation based on `DoubleBasedFOVBase`, and [RecursiveShadowcastingBooleanBasedFOV](xref:GoRogue.FOV.RecursiveShadowcastingBooleanBasedFOV), which is a version of the same algorithm based on `BooleanBasedFOVBase`.  There is no reason you cannot also do so for your own algorithms; however GoRogue does so largely to ensure that it offers maximum flexibility, so with a particular use case in mind, you probably need to create only one version.

If you wish to implement the result views in a different way, you can always inherit from `FOVBase` directly, which allows you complete control over the underlying data structures.  You can also implement `IFOV` directly; however this is probably not ideal for most use cases because some of the properties and functionality of `IFOV` is non-trivial to implement correctly.

### Other Common Paradigms
One other component to implementing custom FOV algorithms which has not yet been discussed is how to implement `NewlySeen`, `NewlyUnseen`, and `CurrentFOV`.  The properties are of type `IEnumerable<Point>`, so there are many possible ways to implement them.  However, a common way to do this efficiently is to store the current cells inside the FOV in a `HashSet<Point>` which you add the points to as you find them during the calculation.  `CurrentFOV` can then just expose the hash set:

```CS
private HashSet<Point> _currentFOV;
public IEnumerable<Point> CurrentFOV => _currentFOV;
```

To implement `NewlySeen` and `NewlyUnseen`, you can simply store the _previous_ calculation's `HashSet` as well, and take the set difference of them.  Assuming you are using `FOVBase` or one of the other provided base class, an implementation might look like this:

```CS
private HashSet<Point> _currentFOV = new HashSet<Point>();
private HashSet<Point> _previousFOV = new HashSet<Point>();
public IEnumerable<Point> CurrentFOV => _currentFOV;

// We can now implement NewlySeen and NewlyUnseen using set difference
public override IEnumerable<Point> NewlySeen
    => _currentFOV.Where(pos => !_previousFOV.Contains(pos));

public override IEnumerable<Point> NewlyUnseen
    => _previousFOV.Where(pos => !_currentFOV.Contains(pos));

protected override void OnReset()
{
    // Reset result views however is needed

    // Store previous FOV so we can calculate NewlySeen and NewlyUnseen
    (_previousFOV, _currentFOV) = (_currentFOV, _previousFOV);
    _currentFOV.Clear();
}
```

It is also possible to implement these properties using the same type of logic with any set-type structure.  For example, the "primitives" library which GoRogue depends on [currently has an issue open](https://github.com/thesadrogue/TheSadRogue.Primitives/issues/131) to add set structures specifically designed for sets of points, which are more efficient for some use cases than `HashSet`; these types of structures would also work.

You can find examples of this and other common paradigms in the built-in GoRogue implementations.