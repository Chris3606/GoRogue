---
title: Grid System
---

# Grid System
GoRogue includes a number of classes and methods that, together, serve to create the concept of an integer-based grid, and to assist you with operations that you might typically do based on that concept.  This article serves as a quick overview of those features, to help familiarize you with what they are and what they can do.  Since "doing operations on a grid" is a broad topic, this article _is not_ a comprehensive overview of all of the functionality of these features -- rather, it is only intended to get you familiar with each features basic purpose.  For a more comprehensive overview of these features, see the API Documentation on each of the mentioned classes -- it will provide a comprehensive description of all features it contains.  You may also click each header (or navigate to the corresponding article in the sidebar) for more details.

# [2D Points](~/articles/grid_components/2d-points.md)
The `Coord` class simply represents a 2D, integral point.  It provides methods for things like adding coordinates together as vectors, as well as other common mathematical computations that pertain to an integer-based grid.

# [Directions and Defining the Coordinate Plane](~/articles/grid_components/directions-defining-coordinate-plane.md)
The `Direction` class provides a convenient representation of directions of movement on a grid.  It also defines and allows customization of the "coordinate plane" GoRogue uses, and offers various utility functions to conveniently interact with 2D points and perform direction-related math.

# [Adjacency and Neighbors](~/articles/grid_components/adjacency-and-neighbors.md)
GoRogue provides convenient methods to calculate and retrieve neighboring grid locations via the `AdjacencyRule` class. This class also provides easy methods to get the `Direction` instances that lead to those neighbors.

# [Measuring Distance](~/articles/grid_components/measuring-distance.md)
The `Distance` class provides ways to measure the distance between two points, using various distance calculation algorithms.

# Working with Radius Shapes
Coming soon!

# Representing Rectangles
Coming soon!

# Line Creation
Coming soon!
