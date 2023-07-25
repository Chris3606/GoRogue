---
title: Grid View Concepts
---

# Grid Views Concept
GoRogue offers many different algorithms that either help to generate or operate on grids -- pathfinding, map generation, FOV, sense mapping, and many other algorithms all need to either generate data about, or extract some sort of data from, a grid, in order to function.  However, the data about the grid that these algorithms need (eg. how a given algorithm "views" a grid) may differ in all of these cases.

For example, in the case of A* pathfinding, the algorithm might view a grid as a single boolean value per location, saying whether or not that location is passable -- this is the only data about a grid that the algorithm needs to function. By contrast, however, goal-map pathfinding might view each location in the grid as one of three values; clear space, obstacle space, or goal.  Sense mapping is different still; it might view the grid as a double value per location, which gives its resistance to spreading values. While the data in these examples may be inter-related, each algorithm views the grid as a different set of values.

GoRogue provides implementations of all the algorithms above (and more), and is designed to be a highly portable library that doesn't require the use of any particular code architecture or arrangement of data to function.  So, its algorithms must view the grid in the appropriate way without any real knowledge of or guarantees about how the data is stored or acquired. For example, in the case of A*, its boolean data described above could be stored in a simple 2D array of boolean values. It could also be a boolean value out of some `Tile` or `GameObject` class which is stored in an array or spatial map; or, it could be acquired as the result of a complex operation operation such as ray-casting.  One of the goals of GoRogue is to allow all these options (and any other that a user might create) to be valid, without necessitating code modifications, and without being difficult to use and requiring you to duplicate data in order to specify it in a particular format.  To accomplish this, GoRogue algorithms view the map via an abstraction called a "grid view".

>[!TIP]
>Although a relatively simple concept, the "grid view" abstraction is considered to be foundational knowledge for using GoRogue.  All features of the library that implement an algorithm of some sort on a grid will ask for grid views as input, produce them as output, or both.  We recommend that you, at minimum, familiarize yourself with the concept and some of the built-in implementations, before digging too deep into other features of the library like pathfinding and FOV.

## Important Knowledge and Disclaimers
Before digging into the grid view concept, it is worthy to note a few bits of background knowledge.

First, **the concept of "grid views" is not defined by GoRogue**; it is defined by [TheSadRogue.Primitives](https://github.com/thesadrogue/TheSadRogue.Primitives), also known as the "primitives" library, which is a dependency of GoRogue.  This library is open-source and maintained in large part by the same author as GoRogue.  However, this article will focus on the grid view concept, and some common scenarios users may encounter when using GoRogue.  It is **not** designed to be a comprehensive guide on grid views; there will be implementations provided by TheSadRogue.Primitives which are not mentioned here.  If a built-in implementation is not mentioned here, it does **not** mean you won't find it useful; just that it wasn't necessary to understand the concept, and was not picked as one of the examples used to demonstrate those concepts.  Therefore, you may find it very beneficial to look though the [API documentation for grid view implementations](https://thesadrogue.github.io/TheSadRogue.Primitives/api/SadRogue.Primitives.GridViews.html) to get a feel for what implementations are already provided.

Similarly, **this article is not meant as a tutorial on how to use specific GoRogue features**, such as pathfinding or FOV.  Specific features will have their own articles dedicated to them.  This article is **only** intended to introduce basic knowledge of what grid views are, and a few ways GoRogue commonly uses them.  So, when this article mentions other features, it is doing so only as a use case/example, and it will gloss over specifics of those features.  If you're interested in those features, you should check out their dedicated articles; however, keep in mind that those dedicated articles won't spend much time explaining what a grid view is and how to create one; so our recommendation is to familiarize yourself with grid views _first_ (via this article), then check out specific articles for features that use them.

# The Abstraction
The "grid view" concept utilizes a couple of core interfaces to create an abstraction over the concept of a grid with values associated with each position, which are described below.

## IGridView
The [IGridView&lt;T&gt;](xref:SadRogue.Primitives.GridViews.IGridView`1) interface is the basis of the grid view concept, and represents a bounded grid which contains "values" for each position within it.  An implementation of `IGridView<T>` uses [indexers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/indexers/using-indexers) to take in a Point, and return the value of type T corresponding to that location.  It also provides an overload of its indexer that take two integers (an x and a y value) and produce an equivalent result, as well as an overload that takes only a single integer.  The single-integer overload expects an "index" of a 2D position encoded via the formula `Y * Width + X`.  This is a fairly common formula to use for encoding a statically sized 2D grid of values into a 1D array, and support for this is built into the `Point` type; see [Point.ToIndex](xref:SadRogue.Primitives.Point.ToIndex(System.Int32)) and [Point.FromIndex](xref:SadRogue.Primitives.Point.FromIndex(System.Int32,System.Int32)) for details.

This interface also defines `Width` and `Height` properties, which is how the "bounded" portion of the abstraction is represented.  A `Count` property is also defined which should always be equal to `Width * Height`; this is largely to support C# [indices](https://learn.microsoft.com/en-us/dotnet/csharp/tutorials/ranges-indexes#type-support-for-indices-and-ranges).

This simple abstraction solves the problem of grid representation for algorithms which read data from a grid.  This allows, for example, an A* pathfinding algorithm to receive its grid representation as a parameter of type `IGridView<bool>`, and a sense mapping algorithm to take its grid representation as a parameter of type `IGridView<double>`.  This way, GoRogue requires no knowledge of how the data is actually stored or retrieved.  A user need only supply an appropriate grid view implementation that retrieves the proper information from their actual data structures.  A user can do this either by using a built-in implementation of `IGridView<T>`, or creating their own; both will be discussed later.

>[!WARNING]
>Although this abstraction allows GoRogue to not concern itself with how your data is stored or retrieved, the actual data structures you are retrieving data from are still very important!  In many cases, the performance of a grid view implementation will drastically affect the performance of a GoRogue algorithm using it.  This abstraction simply leaves those performance considerations up to you, the user.

## ISettableGridView
`IGridView<T>` works very well with algorithms such as A* pathfinding, where the algorithm does not modify the grid. However, some algorithms (like ones that pertain to map generation, for instance) will need to modify values in the grid as well.  To support abstraction in these cases, [ISettableGridView&lt;T&gt;](xref:SadRogue.Primitives.GridViews.ISettableGridView`1) is provided. This interface is exactly like `IGridView<T>` (in fact, it itself implements `IGridView<T>`), however its indexers must also define `set` functions. These functions take a value of type `T`, and apply whatever changes are necessary to underlying data structures to implement that change.

`ISettableGridView<T>` also defines a `Clear()` function and a `Fill(T value)` function, which can be used to clear the grid to a default value or fill it with a given value, respectively.

One key concept to emphasize, is that **`ISettableGridView<T>` implements `IGridView<T>`**; you should think of `ISettableGridView` as an extension of `IGridView` that simply adds the functionality to set values to locations.  This is important because any built-in grid view implementations that _can_ implement `ISettableGridView<T>` generally _will_; and this does not preclude them from being used as `IGridView<T>` instances if needed.

# Provided Interface Implementations
There are a number of grid view interface implementations provided by the "primitives" library, that will fit a wide variety of use cases.  **Not all of them will be covered here**, as previously discussed; we'll only discuss a few examples that are very commonly used with GoRogue or have caveats you should be aware of.  You are encouraged to look through the [grid view API documentation](https://thesadrogue.github.io/TheSadRogue.Primitives/api/SadRogue.Primitives.GridViews.html) to see what options are provided.  There are also some additional grid view implementations provided by GoRogue in the [GoRogue.GridViews](xref:GoRogue.GridViews) namespace; however these are somewhat niche and aren't discussed here either.

Some use cases may also require you to make your own implementations; this will be discussed later.

## ArrayView&lt;T&gt;
`ArrayView<T>` is one of the simplest concrete grid views provided.  It implements `ISettableGridView<T>` by simply wrapping an array of type `T`.  When it is created, you can either give it give it a width and a height, in which case it will create a new array of type `T[]` and expose that array to you via its indexer functions, or you can give it an existing array and a width, and it will calculate a height and use the array you gave it (without copying).  `ArrayView<T>` also implicitly converts to `T[]` (again, without copying), so if you need to pass an array to some function or other library, you can easily do so.

### Common Uses
`ArrayView<T>` is extremely useful and has a few common use cases in GoRogue:

1. It implements the concept of accessing a 1D array which is indexed such that there is one value per position.  Since it is implicitly convertible to an array, you can use it as a substitute for doing the position-to-index math yourself, even if you're not working with code that explicitly takes grid views.

2. If data you need to pass to a GoRogue algorithm is already in an array, you can simply wrap that array using an `ArrayView<T>` and pass that grid view to the algorithm.

3. If your data is _not_ stored in an array, but you would like to keep track of the data that some algorithm needs in a "cached" array rather than having the algorithm retrieve your data every time it needs it, you can create an `ArrayView<T>` and use it to cache the data.  Although creating and managing your own cache like this is not necessary or convenient in some cases, you may find that you want to do it for performance reasons since accessing data from an array is very fast compared to virtually anything else.

>[!TIP]
>If you are thinking of using an `ArrayView<bool>` and you are _not_ trying to wrap an existing array, you should consider using `BitArrayView` instead.  It is based on a C# BitArray, so it may take up to 8x less memory than an `ArrayView<bool>`, and typically doesn't have much difference in raw performance.

## ArrayView2D&lt;T&gt;
`ArrayView2D<T>` is the same as `ArrayView<T>`, except that it wraps a two dimensional array (eg an array of type `T[,]`).  Typically, this is useful only if you have an existing array of type `T[,]` that you want to wrap and present to an algorithm that takes a grid view.  A 1D array like the one used by `ArrayView<T>` is preferable in most other circumstances.

## BitArrayView
`BitArrayView` is another provided grid view implementation.  It implements `ISettableGridView<bool>`, and is based on C#'s `BitArray`, which represents an series of boolean values using 1 bit for each value.  Like `ArrayView<T>`, when you create one you can either give it give it a width and a height, in which case it will create a new `BitArray` and expose that array to you via its indexer functions, or you can give it an existing `BitArray` and a width, and it will calculate a height and use the `BitArray` you gave it (without copying).  It is also implicitly convertible to `BitArray` (without copying).

It uses up to 8x less memory than an `ArrayView<bool>`, but otherwise is typically the same in terms of raw performance.  Therefore, it is useful for the same cases where `ArrayView<bool>` is ([discussed here](#common-uses)).  If those reasons fit your use case, and you do _not_ need to wrap some already existing `bool[]`, you can probably use `BitArrayView` instead and save yourself some memory without a notable performance loss.

## LambdaGridView&lt;T&gt; and LambdaSettableGridView&lt;T&gt;

`LambdaGridView<T>` is a simple implementation of `IGridView<T>` which implements the indexers by taking a `Func<Point, T>` in its constructor, and calling the function to retrieve a value every time an indexer is used.

There is also a `LambdaSettableGridView<T>`, which is the same except for it implements `ISettableGridView<T>` and takes an extra parameter; an `Action<Point, T>` which is called to "assign" a value to a location.

For GoRogue algorithms, the bulk of the use for these will involve `LambdaGridView<T>`, because it is a convenient implementation to use in cases where retrieving a value appropriate for a grid view is a simple matter of calling an existing function, or accessing a property from a simple structure or class.

>[!NOTE]
>Although `LambdaGridView<T>` can be convenient, be aware that it may have performance implications.  Generally, GoRogue algorithms which take grid views _by design_ do not introduce any sort of intermediate caching between the grid view you provide and the algorithm.  So, if you specify a `LambdaGridView<T>`, _every time_ an algorithm needs to retrieve a value, it will call your function; so the faster your function, the faster the algorithm, and conversely the slower your function the slower the algorithm.  Particularly since some algorithms can access grid views many times during their operations, you may find that a `LambdaGridView<T>` results in a much slower algorithm compared to something like an `ArrayView<T>`.  GoRogue leaves this performance determination up to you; if you find that a `LambdaGridView<T>` is fast enough, you can use it; if not, you can create your own cache via `ArrayView<T>` or some other structure and maintain it.  This gives you the most overall control over the performance characteristics and tradeoffs. 

### Code Example
As an example, we'll consider a relatively common case where a user might have an array of `Tile` structures which represent their terrain, and they need to generate data for a grid view by accessing a field in the proper tile structure.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/GridViewConcepts.cs#LambdaGridViewExample)]

## Translation Grid Views
The primitives library also implements a number of "translation views", which are grid views designed to map values of one grid view to values in a grid view of a different type.  The most common translation grid view is `LambdaTranslationGridView<T1, T2>`, which works very similarly to the above `LambdaGridView<T>`; it implements `ISettableGridView<T2>`, and you specify a function at construction which, given a position and value, translates that value to the new type `T2`.  There is also `LambdaSettableTranslationView<T1, T2>`, which is the same except it implements `ISettableGridView<T2>` and you specify a "set" function.

### Basic Code Example
The following code example re-implements the above translation from `Tile` instances, but uses a `LambdaTranslationGridView` instead.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/GridViewConcepts.cs#LambdaTranslationGridViewExample)]

### Subclasses
If you want to implement a translation grid view via a subclass, rather than by specifying a function, the "primitives" library also provides `TranslationGridView<T1, T2>`.  This class specifies two protected, virtual functions; `T2 TranslationGet(T1 value)` and `T2 TranslationGet(Point position, T1 value)`.  You should override exactly _one_ of these to implement the translation from one value to another.  It does not matter which one you override; the functions are configured such that whichever of the two functions you override will be called.

There is also a `LambdaSettableTranslationGridView<T1, T2>` class which, again, implements the same concept but there is a pair of functions `TranslateSet` which you may override as well.

### Subclass Code Example
The following example implements the same translation the above one does, but uses the subclass method instead.  There is no real need to use a subclass here, other than for the sake of an example and/or personal preference; the functionality is exactly the same as before:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/GridViewConcepts.cs#SubclassTranslationGridViewExample)]

## Viewport
The "primitives" library also provides grid views designed to represent "sub-areas" of other grid views.  This can be useful for representing very large maps to algorithms, where you only want an algorithm to consider a small portion of the map.  The primary reason you might want to do this, is performance.  Consider `GoalMap`, for instance, which is a fairly expensive algorithm.  On a very large map, you may only need to calculate the dijkstra map for a portion of the map.  A viewport can help facilitate this.

>[!WARNING]
>The viewport is a very basic concept which may leave some to be desired in certain situations.  Note that `Viewport` _only_ implements indexers that take coordinates in that viewport's "coordinate space", and most algorithms have no concept of if they're operating on a viewport or some other grid view implementation.  So if you give an algorithm a viewport, it will expect coordinates to be given to it in the _viewport_ coordinate scheme; and similarly, it will produce its output in that viewport's coordinate space.  It remains the user's responsibility to do any translation as it pertains to translating between "actual" positions and viewport positions.  An example will be specified below which demonstrates this.

Note that a more advanced "coordinate space translation" abstraction is planned for the primitives library in the feature, which may take the place of this for many use cases.  You can track its progress on [this issue](https://github.com/Chris3606/GoRogue/issues/281).

There are also two other viewport-related grid views provided.  The first is `SettableViewport<T>`, which is identical to `Viewport<T>` except for it implements `ISettableGridView<T>` and therefore allows you to set values via the viewport.  The second is `UnboundedViewport<T>`.  `UnboundedViewport<T>` is similar to `Viewport<T>`, but instead of throwing an exception if you access elements outside of the current viewing area, it will instead return a default value.

### Code Example
The following code example uses a viewport to create a `GoalMap` which is based on a much larger grid view.  The semantics of how a goal map work are unimportant; goal maps are covered in their own article; for the purposes of this example, just note that the result of a goal map is a map where the value of each square is its distance from the nearest "goal".  The main intent is to demonstrate the semantics of a viewport and its limitations.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/GridViewConcepts.cs#ViewportDemonstration)]

# Creating Custom Grid Views
In some cases, you may want to create a custom implementation of `IGridView<T>` or `ISettableGridView<T>`.  In these cases, you may simply implement these interfaces; however, there is some repetitive code involved.  As outlined above, the `IGridView<T>` interface involves defining 3 indexers; one which takes a `Point`, one which takes a position as an x and y value, and one that takes an index and uses it as if it is encoded using the formula `Y * Width + X`.  In most cases, these indexers can be defined in terms of each other; an index and an X and Y value can both be converted to a `Point`, for example, given a grid view with known width.  Furthermore, the interface has you define a `Count` property which should be equal to `Width * Height`.

In order to alleviate this type of repetitive boilerplate code, some base classes are provided that you can inherit from instead of implementing the interface directly.  These classes implement `IGridView<T>` by having you define only the width, height, and a single indexer; and the remaining properties are defined in terms of those.

The first such class is `GridViewBase<T>`.  It has you define an indexer taking a `Point`, and the other indexers are defined in terms of that one.  The following is a code example which defines a simple custom grid view using this class as the base.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/GridViewConcepts.cs#GridViewBaseExample)]

There is also a `GridView1DIndexBase<T>` class.  This is identical to `GridViewBase<T>`, but instead of implementing an indexer which takes a `Point`, you implement an indexer which takes a 1D index.  This may be more useful if you are wrapping an array or some structure that presents itself like an array.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/GridViewConcepts.cs#GridView1DIndexBaseExample)]

There are also equivalents of these classes for implementing `ISettableGridView<T>`; these are `SettableGridViewBase<T>` and `SettableGridView1DIndexBase<T>`, respectively.

# Useful Extension Methods
There are many useful and well-optimized extension methods provided for grid views; most of which are quite simple.  We will not cover them all individual here; you should look through the API documentation for [IGridView](xref:SadRogue.Primitives.GridViews.IGridView`1) and [ISettableGridView](xref:SadRogue.Primitives.GridViews.ISettableGridView`1) for details.  Particularly useful methods include `Positions()`, `Bounds()` and `ApplyOverlay()`.  There are also extensions to ShaiRandom's `IEnhancedRandom` interface, [which GoRogue provides](xref:ShaiRandom.Generators.GoRogueEnhancedRandomExtensions).  Although many extensions are provided, the grid-view related functions include various overloads of `RandomPosition()`, which focus on selection of random positions from within the bounds of the grid view (which may optionally be required to meet certain parameters).