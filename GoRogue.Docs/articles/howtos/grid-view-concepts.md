---
title: Grid View Concepts
---

# Grid Views Concept
GoRogue offers many different algorithms that either help to generate or operate on grids -- pathfinding, map generation, FOV, sense mapping, and many other algorithms all need to either generate data about, or extract some sort of data from, a grid, in order to function.  However, the data about the grid that these algorithms need (eg. how a given algorithm "views" a grid) may differ in all of these cases.

For example, in the case of A* pathfinding, the algorithm might view a grid as a single boolean value per location, saying whether or not that location is passable -- this is the only data about a grid that the algorithm needs to function. By contrast, however, goal-map pathfinding might view each location in the grid as one of three values; clear space, obstacle space, or goal.  Sense mapping is different still; it might view the grid as a double value per location, which gives its resistance to spreading values. While the data in these examples may be inter-related, each algorithm views the grid as a different set of values.

GoRogue provides implementations of all the algorithms above (and more), and is designed to be a highly portable library that doesn't require the use of any particular code architecture or arrangement of data to function.  So, its algorithms must view the grid in the appropriate way without any real knowledge of or guarantees about how the data is stored or acquired. For example, in the case of A*, its boolean data described above could be stored in a simple 2D array of boolean values. It could also be a boolean value out of some Tile or GameObject class which is stored in an array or spatial map; or, it could be acquired as the result of a complex operation operation such as ray-casting.  One of the goals of GoRogue is to allow all these options (and any other that a user might create) to be valid, without necessitating code modifications, and without being difficult to use and requiring you to specify data in a particular format.  To accomplish this, GoRogue algorithms view the map via an abstraction called a "grid view".

>[!TIP]
>Although a relatively simple concept, the "grid view" abstraction is considered to be foundational knowledge for using GoRogue.  All features of the library that implement an algorithm of some sort on a grid will ask for grid views as input, produce them as output, or both.  We recommend that you, at minimum, familiarize yourself with the concept and some of the built-in implementations, before digging too deep into other features of the library like pathfinding and FOV.

## Important Knowledge and Disclaimers
Before digging into the grid view concept, it is worthy to note a few bits of background knowledge.

First, **the concept of "grid views" is not defined by GoRogue**; it is defined by [TheSadRogue.Primitives](https://github.com/thesadrogue/TheSadRogue.Primitives), also known as the "primitives" library, which is a dependency of GoRogue.  This library is open-source and maintained in large part by the same author as GoRogue.  However, this article will focus on the grid view concept, and some common scenarios users may encounter when using GoRogue.  It is **not** designed to be a comprehensive guide on grid views; there will be implementations provided by TheSadRogue.Primitives which are not mentioned here.  If a built-in implementation is not mentioned here, it does **not** mean you won't find it useful; just that it wasn't necessary to understand the concept, and was not picked as one of the examples used to demonstrate those concepts.  Therefore, you may find it very beneficial to dig though the [API documentation for grid view implementations](https://thesadrogue.github.io/TheSadRogue.Primitives/api/SadRogue.Primitives.GridViews.html) to get a feel for what implementations are already provided.

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

It uses up to 8x less memory than an `ArrayView<bool>`, but otherwise is typically the same in terms of raw performance.  Therefore, it is useful for the same cases where `ArrayView<bool>` is ([discussed here](#common-uses)).  If those reasons fit your use case, and you do _not_ need to wrap some already existing `bool[]`, you can probably use `BitArray` instead and save yourself some memory without a notable performance loss.

# LambdaGridView&lt;T&gt;
TODO: Here
