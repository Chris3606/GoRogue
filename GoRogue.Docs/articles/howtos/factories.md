---
title: Using Factories
---

# Factory Classes
One common paradigm in development is to have a "factory" whose responsibility is to produce objects of a given type.  This can be as simple as a class with static methods whose name corresponds to the name of the object it creates; for example, you could have an `EnemyFactory` class which has methods `Orc()`, `Goblin()`, etc; however implementations which conveniently allow for serialization and/or customization via user-readable data files can become more complex.  GoRogue provides a set of pre-built classes which provide one possible way of implementing this paradigm.

# Factories in Concept
The GoRogue factory implementations consists of two components; the factory class, and blueprints.  A blueprint is simply a unique identifier which denotes the type of item it creates, paired with a function which, when called, creates an object of that type.  One or more blueprints are added to a factory class.  After blueprints have been added, factories have a `Create()` function which can be passed a type of item as a parameter, and will call the appropriate blueprint's `Create()` function in order to create an item of that type and return it to you.

# Basic Usage
The simplest way to use the factory system, is to create a `Factory` which consists of `LambdaFactoryBlueprint` instances, which allow you to specify the creation function as a `Func<TProduced>`:

```CS
// Arbitrary class we want to create instances of.
public record Terrain(int Glyph, bool IsWalkable, bool IsTransparent);

// We'll identify the blueprints with strings in this instance, but this could be an enum or
// any hashable type
var factory = new Factory<string, Terrain>()
{
    new LambdaFactoryBlueprint<string, Terrain>("Floor", () => new Terrain('.', true, true)),
    new LambdaFactoryBlueprint<string, Terrain>("Wall", () => new Terrain('#', false, false))
};

var floorTile = factory.Create("Floor");
var wallTile = factory.Create("Wall");
```

You could also create a subclass of `Factory` and have the creation functions be (static or non-static) methods on that subclass, if you prefer:

```CS
public record Terrain(int Glyph, bool IsWalkable, bool IsTransparent);

public class MyFactory : Factory<string, Terrain>
{
    public MyFactory()
    {
        Add(new LambdaFactoryBlueprint<string, Terrain>("Floor", Floor));
        Add(new LambdaFactoryBlueprint<string, Terrain>("Wall", Wall));
    }

    private Terrain Floor() => new('.', true, true);
    private Terrain Wall() => new('#', false, false);
}

var factory = new MyFactory();

var floorTile = factory.Create("Floor");
var wallTile = factory.Create("Wall");
```

This may be cleaner than anonymous functions if your creation methods are more complex and entail a fair a bit of code.

`LambdaFactoryBlueprint` instances work best as your blueprint type when your blueprints have no state or wrapper code associated with them.  If you do have some state, have more advanced customization or parameterization you wish to do, or simply prefer creating subclasses for each item type, the blueprint types need only implement `IFactoryBlueprint`; so you may create your own subclass:

```CS
public record Terrain(int Glyph, bool IsWalkable, bool IsTransparent);

// A blueprint for terrain which counts the number of times each item type is instantiated.
public record TerrainBlueprint(string Id, int Glyph, bool IsWalkable, bool IsTransparent) : IFactoryBlueprint<string, Terrain>
{
    private static readonly Dictionary<string, int> s_countingDictionary = new();

    public Terrain Create()
    {
        s_countingDictionary[Id] = s_countingDictionary.GetValueOrDefault(Id, 0) + 1;
        return new Terrain(Glyph, IsWalkable, IsTransparent);
    }
}

var factory = new Factory<string, Terrain>()
{
    new TerrainBlueprint("Floor", '.', true, true),
    new TerrainBlueprint("Wall", '#', false, false)
};

var floorTile = factory.Create("Floor");
var wallTile = factory.Create("Wall");
```

In some cases, you may wish to pass some parameters and/or state to the blueprint when the `Create()` method is called, rather than when the blueprint is created.  For this, you should use `AdvancedFactory` instead of `Factory`.  `AdvancedFactory` is identical to `Factory` except that it lets you specify an additional type parameter which is the type of a parameter you pass to the factory's `Create` function.  This parameter is, in turn, passed to the blueprint.

Below is an example which aims to pass a Point to the `Create()` function which specifies the object's initial position:

```CS
public record Terrain(Point Position, int Glyph, bool IsWalkable, bool IsTransparent);

// LambdaAdvancedFactoryBlueprint is the same as LambdaFactoryBlueprint but implements
// IAdvancedFactoryBlueprint instead, which allows its creation function to take parameters.
// This is useful, for example, to create objects that require parameters to be passed to
// their constructor.
var factory = new AdvancedFactory<string, Point, Terrain>
{
    new LambdaAdvancedFactoryBlueprint<string, Point, Terrain>("Floor", pos => new Terrain(pos, '.', true, true)),
    new LambdaAdvancedFactoryBlueprint<string, Point, Terrain>("Wall", pos => new Terrain(pos, '#', false, false))
};

var floorTile = factory.Create("Floor", new Point(1, 2));
var wallTile = factory.Create("Wall", new Point(3, 4));
```

You may also implement an `AdvancedFactory` subclass if you wish, or create custom blueprints by implementing the `IAdvancedFactoryBlueprint` interface yourself, just like the above examples which use `Factory` do.