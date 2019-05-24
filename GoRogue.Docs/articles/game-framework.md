---
title: Game Framework
---

# Game Framework
As discussed in the [GoRogue introduction](~/articles/intro.md) page, as part of its feature set, GoRogue includes "game framework" features.  Unlike the core features, their purpose is to combine GoRogue core features into a coherent, concrete structure that can be used as a framework for your game, and build upon those features to create functionality that may apply to many use cases.  This includes providing a `GameObject` class that can be used to represent your world objects, a `Map` class that can be used to store those `GameObjects`, and a way to easily add components to your `GameObjects`.

While this game framework set of features, by design, is not as situation-agnostic as the core features, it is still designed to be versatile, and can provide at least a beneficial starting setup, if not more, for many situations.  Furthermore, even in cases where the feature set need not apply, the code can still serve as an example of what some GoRogue data structures do, and how they can be set up to interact.

# The GameObject Concept
GameObjects are designed to provide the basic functionality for objects that reside in the world.  They expose properties, functions, and events relevant to pretty much any object that is going to reside in the world.  These include things like having a position, a layer (see [The Map Concept](#the-map-concept) below for details on layers), potentially moving and colliding, and having walkability/transparency values that may affect collision and FOV.  GameObjects also implement the extremely flexible GoRogue component system, which allows you to easily add functionality to them in the form of components, if you choose to do so.

This concept is implemented via two GoRogue structures -- `GameObject`, a class that implements this functionality, and `IGameObject`, an interface that includes the entire public interface of `GameObject`.  See the usage examples below for how these can be used effectively.

# The Map Concept
The `Map` class, at its core, is simply a collection of `IGameObject` instances that reside in a world.  It provides functions for adding and removing `IGameObjects`, and functions/properties/events that help you find and interact with those `IGameObjects`.

More specifically, it is a collection of "layers" of `IGameObject` instances.  As stated above, each `IGameObject` resides on an integer "layer", which allows you to categories `IGameObjects` in ways like `Monster`, `Item`, `Terrain`, etc.  Each map has at least two layers -- a "terrain" layer, which consists of the integer layer 0, and one or more "entity" layers, which must have integer values greater than 0.  The terrain layer has some restrictions that are typically natural for terrain, namely that `IGameObjects` representing terrain cannot move, and must obviously reside on layer 0.  The number of entity (non-terrain) layers is customizable, and these layers do not have those restrictions.

While terrain layer and entity layer(s) can be treated separately, since both types of layers hold `IGameObject` instances, there are a number of functions provided by `Map` that consider the two equivalent.  For example, `GetObjects(Coord)` returns all objects at a given position, whether they be entities or terrain.

# A Starting Example
Let's see some code!  The simplest way to use the built-in `Map` system is to simply create `GameObject` instances, and add them to a map:

```CSharp
// Factory class for terrain.  Things to note here, are that both terrain GameObjects
// are placed on layer 0, and have their isStatic set to true (indicating that the object cannot move).
// If these things are incorrectly set, an exception will be raised when we try to add them to the map
// using SetTerrain later
static class TerrainFactory
{
    // Note also that both objects have the parentObject flag set to null.  This is because they have no parent,
    // as we are not inheriting from GameObject, nor are we using a GameObject instance as a backing field.  If
    // either of these things were true, the parameter would be "this" instead.
    public GameObject Wall(Coord position)
        => new GameObject(position, layer: 0, parentObject: null, isStatic: true, isWalkable: false, isTransparent: false)

    public GameObject Floor(Coord position)
        => new GameObject(position, layer: 0, parentObject: null, isStatic: true, isWalkable: true, isTransparent: true)
}

// Factory class for "entities", eg. everything not terrain
static class EntityFactory
{
    // Similar to above.  For this object, we need to make sure to set the isWalkable to false, as the player collides with other things!
    public GameObject Player(Coord position)
        => new GameObject(position, layer: 1, parentObject: null, isStatic: false, isWalkable: false, isTransparent: true);
}

class Program
{
    public static void Main(string[] args)
    {
        // We'll use GoRogue map generation to generate a simple rectangle map, with walls
        // around the edges and floor everywhere else, then translate to use our GameObjects.
        var terrainMap = new ArrayMap<bool>(80, 50);
        QuickGenerators.GenerateRectangleMap(terrainMap);

        var map = new Map(width: terrainMap.Width, height: terrainMap.Height,
                          numberOfEntityLayers: 1, distanceMeasurement: Distance.CHEBYSHEV);

        foreach (var pos in terrainMap.Positions())
            if (terrainMap[pos]) // Floor
                map.SetTerrain(TerrainFactory.Floor(pos));
            else // Wall
                map.SetTerrain(TerrainFactory.Wall(pos));

        // Create the player at position (1, 1) - just inside the outer walls
        var player = EntityFactory.Player((1, 1));
        map.AddEntity(player);
    }
}
```

And that's it, you now have a map!  For just those few lines of code, you have a map that holds your world's objects, provides some basic functions to allow you to access those objects, and provides quite a bit of other basic functionality.  

## Collision Detection
For starters, collision detection is fully implemented, based on the `isWalkable` values you set to the `GameObject` instances.  If you try to move the player to (0, 0), nothing happens, because that's a wall!

```CSharp
player.Position = (0, 0); // The player doesn't move at all here, because moving there would violate collision
player.Position = (2, 1); // This works fine, though -- the player is now at (2, 1)
player.Position += Direction.DOWN; // Since many moves are to an adjacent square, we can use directions too.
// This function returns true if the move was successful, so we know if collision blocked it or not
player.MoveIn(Direction.UP_LEFT); 
```

## FOV and Tile Exploration
Similarly, FOV is already implemented for you, based on the `isTransparent` values you set.  On top of that, exploration of tiles is tracked for you automatically as you update the FOV -- anything that comes into FOV at any point is marked as explored.

```CSharp
map.CalculateFOV(position: player.Position, radius: 10, radiusShape: Radius.SQUARE);
bool aTrue = map.FOV.BooleanFOV[position.Player]; // Player is in FOV
bool aFalse = map.FOV.BooleanFOV[70, 40]; // But, this square is not

bool exploredTrue = map.Explored[5, 5]; // Positions in FOV were automatically marked explored
bool exploredFalse = map.Explored[40, 50]; // But, positions out of FOV were not
```

## Basic A* Pathfinding
You also have basic A* pathfinding set up for you, based on the walkability values you pass in.  Pathfinding, by default, treats anything with `isWalkable` set to `false` as an obstacle to avoid, and determines whether or not to allow diagonal movement (and the cost therein) from the `Distance` calculation you passed in when you made the map.  If want to define some of this differently, you can assign a new `AStar` instance to the `Map.AStar` property.

``` CSharp
var shortestPath = map.AStar.ShortestPath(player.Position, (5, 5));
Console.WriteLine(path.Steps.ExtendToString());
```

## Functions to Interact with Objects in the Map
In addition to all the above, `Map` provides a number of functions to interact with objects in the map, data about the map (walkability and transparency values for each location, for instance), and to do other common operations like remove objects.  You are encouraged to look through the `Map` API documentation (hosted [here](xref:GoRogue.GameFramework.Map)) for these, as not even close to all of these functions are covered here, however to give you an idea of what they can do:

```CSharp
var allObjects = map.GetObjects(player.Position).ToList(); // Gets all objects, whether they be terrain or entities
var entities = map.Entities.GetItems(player.Position); // Returns only entities, not terrain.
var terrain = map.GetTerrain(player.Position);

// false if there is any non-walkable object (terrain or otherwise) at the position
bool walkabilityOfPlayerPos = map.WalkabilityView[player.Position];
```

In general, many of these functions, such as `GetObjects`and similar functions, take optional layer masks that can restrict them to returning/working with only objects on a certain layer of the map.  Layer masks may be easily generated by using the `Map.LayerMasker` property.

To access entities (as opposed to terrain or all objects) at a location, you can use the `Map.Entities` property.  This provides all the necessary functions to retrieve all entities, retrieve all entities in a layer, retrieve all entities at a location, etc.

## GameObject Components
Although this is not the only architecture in which `GameObject` can function (see below for more examples), this method of simply creating `GameObjects` and adding them to the `Map` actually provides quite a bit of flexibility.  This is because `GameObject` implements the component functionality from the GoRogue component system (see [here](~/articles/component-system.md) for details on that system).  Thus, you can add functionality simply by adding, removing, and interacting with components.

As is the case with the `GoRogue` component system itself, there is no base class or interface that is required of the components you add to `GameObject` -- they can be of any type.  As well, components with inheritance heirarchies and/or components that implement interfaces work just as you would expect -- if you have `interface IZ {}`, `class A : IZ {}`, and class `B : A {}`, and you add a component of type `B`, it qualifies as a component of type `B`, `A`, and `IZ`.

There is also some additional (optional) functionality that can be used with components that go on `GameObject` instances.  While it is not required that you do so, you may choose to have your component types implement `GoRogue.GameFramework.IGameObjectComponent`.  This interface requires a single property called `Parent`, of type `IGameObject`.  If your components implement this interface, and you add them to/remove them from a `GameObject`, the `Parent` field is automatically updated to point to the object that holds it.  As you might expect, if you were to take an instance of something implementing `IGameObjectComponent`, and try to add it as a component to two separate `GameObjects` at once, an exception would be thrown, as the `Parent` can't be updated correctly to point to two different `GameObjects`.

```CSharp
class MyComponent : IGameObjectComponent
{
    public IGameObject Parent { get; set; }

    public DoStuff() => Console.WriteLine("Do stuff!");
}

class Program
{
    public static void Main(string[] args)
    {
        var myObject = new GameObject((5, 5), layer: 1, parentObject: null, isStatic: false, isWalkable: false, isTransparent: true);
        myObject.AddComponent(new MyComponent());
        // The parent was automatically updated to point to the right object.
        bool aTrue = (myObject == myObject.GetComponent<MyComponent>().Parent);
    }
}
```

This may be useful when you need your components to know what they are attached to.

# Using Inheritance
Although the factory-function architecture used in the example above is convenient for many cases, it is not the only architecture in which `GameObject` can function.  You could easily replace the factory functions above (or combine them) with a set of classes that inherit from `GameObject`.  You get all the same functionality as demonstrated above -- the only difference is the value we set to `parentObject` when we call the `GameObject` constructor.

```CSharp
class Terrain : GameObject
{
    // Like before, the layer is set to 0 and the isStatic flag to true.  Note, however, that the parentObject
    // parameter is now "this" -- since we are inheriting from GameObject, the "parent object" is ourselves.
    public Terrain(Coord position, bool isWalkable, bool isTransparent)
        : base(position, layer: 0, parentObject: this, isStatic: true, isWalkable: isWalkable, isTransparent: isTransparent)
    { }
}

class Entity : GameObject
{
    public Entity(Coord position, int layer, bool isWalkable, bool isTransparent)
        : base(position, layer: layer, parentObject: this, isStatic: false, isWalkable: isWalkable, isTransparent: isTransparent)
    { }
}

// From here, we could either create classes that inherit from Terrain and Entity, or create factory methods
// that instantiate Terrain and Entity instances, like we did in the first example
```

## Helpful Map Methods
If you are using inheritance, it may often be useful to be able to retrieve only entities or terrain that are of a specific derived type.  `GoRogue.Map` provides methods for this, that accept some class or interface type that implements `IGameObject` as a template parameter, and return all objects that cast to this value successfully.

```CSharp
// Assuming we had a subclass of GameObject called Activatable, this would get all objects at the player's position (terrain and entity)
// that cast to the Activatable type.
myMap.GetObjects<Activatable>(player.Position);
myMap.GetTerrain<ActivatableTerrain>(player.Position); // Gets the terrain object if it casts to ActivatableTerrain
myMap.GetEntities<EntitySubclass>(player.Position); // Gets all entities (non-terrain) that casts to EntitySubclass
```

These functions can be useful for getting objects of a specific type.  However, particularly in complex cases, where you find yourself utilizing this system to check for specific functionality (like the `Activatable` example above), you may find it more convenient to use the component system (discussed [here](#gameobject-components)).

# Implementing IGameObject
In the above examples, we showed how you can either create `GameObject` instances directly, or use classes that inherit from `GameObject`.  These architectures may work well for many cases, however since C# does not allow multiple inheritance, being forced to inherit from `GameObject` can make things difficult if you are intergrating with an already existing structure, or interacting with code that requires or encourages you to inherit from a different class.  For these cases, `GoRogue` provides the `IGameObject` interface.

## A Problematic Use Case
As an example, I will use code that is heavily inspired by the ASCII display library [SadConsole](https://github.com/SadConsole/SadConsole).  A working knowledge of this library is by no means required to understand this example; the library simply provides a good practical use case.  In SadConsole, there are two classes that are the base of many of your game objects in a typical setup.  First, you have `Cell`, which is a base class that defines some basic rendering characteristics for squares on a console (usually terrain, in the context of a map).  Then, you have `Entity`, which defines similar functionality for other, mobile things.  There are many ways to utilize SadConsole's system, however a typical, basic class setup for using SadConsole might look something like:

```CSharp
class MyTerrain : Cell { }
class MyEntity : Entity { }
```

Inheriting from `GoRogue.GameObject` in this situation would propose a problem, as C# will not allow you to inherit from two different base classes.  You could create a public field of type `GameObject` in `MyTerrain` and `MyEntity`, however this gets extremely complicated because, since then GoRogue only knows about the field, not the object itself, it can become challenging to access the data you want.

## The Solution
To handle this case, GoRogue provides the `IGameObject` interface.  `IGameObject` is just an interface which includes the entirety of the public interface of the `GameObject` class.  The implementation of these functions is non-trivial, and it is the intent that you _never_ implement these manually.  Instead, you can implement this interface on your objects by using a private, backing field of type `GameObject`.  For each method/property required by `IGameObject`, you simply forward the call to the appropriate method/property in the `GameObject` backing field.  Many IDEs, including Visual Studio, can generate these forwarding functions for you automatically.

```CSharp
class MyTerrain : Cell, IGameObject
{
    private IGameObject _backingField;

    public MyTerrain(Coord position, bool isWalkable, bool isTransparent)
    {
        // Of note here is that the parentObject parameter is set to "this".  Because we are using a backing field,
        // we need to set the parentObject to the actual IGameObject that is added to the map, which is "this"
        _backingField = new GameObject(position, layer: 0, parentObject: this, isStatic: true, isWalkable: isWalkable, isTransparent: isTransparent);
    }

    // These are some examples of forwarding functions.  Many IDEs can generate these for you automatically
    bool MoveIn(Direction direction) => _backingField.MoveIn(direction);
    public bool IsStatic => _backingField.IsStatic;
    public event EventHandler<ItemMovedEventArgs<IGameObject>> Moved
    {
        add => _backingField.Moved += value;
        remove => _backingField.Moved -= value;
    }
    /* Rest of forwarding functions would go here */
}

/* MyEntity from the above SadConsole example would be implemented similarly */
```

The GoRogue `GameObject` class is specifically designed to support its use as a backing field like this, and to overcome the limitations you might traditionally encounter when doing so.  For example, in the above code, if you subscribe to the `MyTerrain.Moved` event, generally, you would want the `sender` parameter to reference the `MyTerrain` instance, not `_backingField`.  However, if `GameObject` was implemented traditionally, you might find the opposite -- that the `sender` in fact refers to the backing field.  These type of issues can cause difficult-to-find bugs, and as such `GameObject` has been written to ensure that these types of issues do not occur -- the `sender` parameter of the `MyTerrain.Moved` event, for instance, will indeed refer to the `MyTerrain` instance in the above example.  This even applies to the component-parent interface -- if you attach a component implementing `IGameObjectComponent` to an instance of your `MyTerrain` class, the `Parent` field will accurately point to the `MyTerrain` instance as well.

Furthermore, since the entirety of `Map` uses `IGameObject`, and `IGameObject` contains the entire public interface of `GameObject`, you don't lose any functionality using this method.  All the examples, from the `Map` functions to the presence of the component system, apply to your `IGameObject` implementation directly, just as they would to `GameObject`.  This makes it a very good, relatively straightforward option for using the GoRogue game framework system in cases where you are integrating with an existing structure.