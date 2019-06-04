---
title: 2D Points
---

# 2D Points
The `Coord` class simply represents a 2D, integral point.  It provides methods for things like adding coordinates together as vectors, as well as other common mathematical computations that pertain to an integer-based grid.

## Creating/Accessing Values
Coord instances can be created, like any other object, by simply using `new`, eg. `Coord c = new Coord(1, 2);`, and their values may be accessed through their `X` and `Y` fields.  For the sake of performance and safety, however, Coords are immutable value types, which means their `X` and `Y` fields cannot be modified -- all operations that modify a `Coord` must create a new one.  However, Coord provides a lot of methods, operation, and "syntactic sugar" to help make utilizing them quick and easy.

### Conversion to Tuples
`Coords` are implicitly convertible to value-tuples of two `ints`, and such tuples are implicitly convertible to `Coord` instances.  This is useful particularly in C# because the syntax for creating a value tuple is simply parenthesized values (no `new` required).  Thus, when the type is obvious, say because of an explicit type declaration or a function definition, we can effectively omit `new Coord` part of the syntax involved in creating one, and instead just create a tuple and let it be implicitly converted:
```CSharp
Coord c = (1, 2);
// Assuming I have a function Process(Coord position) that I can call
Process((5, 6));
```

> [!WARNING]
> This syntax works well with explicit type declaration, but it is recommended to avoid using `var` with it.  The code `var c = (1, 2)` will compile, but `c` will be of type `ValueTuple`, not of type `Coord`.  This could create unexpected behavior when `c` is used.

### Deconstruction
`Coords` also support C# Deconstruction syntax, which can be very useful when using a `Coord`'s values as local parameters:
```CSharp
// Assuming myEntity has a Position field that is of type Coord
var (x, y) = myEntity.Position;
// Do processing on x and y as local variables.  Example is printing to keep it succinct
Console.WriteLine("Printing X: " + x); 
```

## Operations
Coords offer a number of operators that allow them to be added/subtracted from each other, as well as operators that allow you to add, subtract, multiply, and divide `Coords` by constants.  The following sample is not a comprehensive look at all available operators, but demonstrates the syntax.  Most of the operators are self-explanatory, and the API Documentation for `Coord` covers them in-depth.

```CSharp
Coord c1 = (1, 2);
Coord c2 = (5, 6);

System.Console.WriteLine(c1 + c2); // (6, 8)
System.Console.WriteLine(c1 - c2); // (-4, -4)

System.Console.WriteLine(c1 * 2); // 2, 4
```

## 1D Indices
When the size of a 2D grid is known, any position on that grid may be mathematically converted to a 1-dimensional array index value unique to that location.  Similarly, given a 1-dimensional index and a width of a grid, it is possible to retrieve the 2d point represented.  This may be particularly useful since some storing a grid as a 1-dimensional array can yield increased performance.  `Coord` provides a set of functions to convert to 1-dimsnional indices and back:
```CSharp
int mapWidth = 10;
int mapHeight = 10;

Coord myCoord = (2, 3);
int index = myCoord.ToIndex(mapWidth); // Get 1D index
Console.WriteLine(Coord.ToIndex(5, 6, mapWidth)) // Get 1D index for position (5, 6)

Coord originalPos = Coord.ToCoord(index, mapWidth); // Get 2D position from index
```

## Utility/Grid Math
`Coord` also provides a number of static functions to assist in common mathematical operations involving points.  These are covered in the API documentation, however include functions such as `BearingOfLine` to get the degree heading of a line between two points, `Midpoint` which simply implements the midpoint formula, and `EuclideanDistanceMagnitude` for quickly comparing euclidean distances.