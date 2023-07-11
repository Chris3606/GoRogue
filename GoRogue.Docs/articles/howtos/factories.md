---
title: Factories
---

# Factory Classes
One common paradigm in development is to have a "factory" whose responsibility is to produce objects of a given type.  This can be as simple as a class with static methods whose name corresponds to the name of the object it creates; for example, you could have an `EnemyFactory` class which has methods called `Orc()`, `Goblin()`, etc; however implementations which conveniently allow for serialization and/or customization via user-readable data files can become more complex.  GoRogue provides a set of pre-built classes which provide one possible way of implementing this paradigm.

# Factories in Concept
The GoRogue factory implementations consist of two components; the factory class, and blueprints.  A blueprint is simply a unique identifier which denotes the type of item it creates, paired with a function which, when called, creates an object of that type.  One or more blueprints are added to a factory class.  After blueprints have been added, you call the `Create()` function on the factory.  This function is  passed a type of item as a parameter (eg. the identifier of a blueprint), and will call the appropriate blueprint's `Create()` function in order to create an item of that type and return it to you.

# Basic Usage
The simplest way to use the factory system, is to create a `Factory` which consists of `LambdaFactoryBlueprint` instances, which allow you to specify the creation function as a `Func<TProduced>`:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/Factories/Factory.cs#BasicExample)]

You could also create a subclass of `Factory` and have the creation functions be (static or non-static) methods on that subclass, if you prefer:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/Factories/Factory.cs#SubclassExample)]

This may be cleaner than anonymous functions if your creation methods are more complex and entail a fair a bit of code.

`LambdaFactoryBlueprint` instances work best as your blueprint type when your blueprints have no state or wrapper code associated with them.  If you do have some state, have more advanced customization or parameterization you wish to do, or simply prefer creating subclasses for each item type, the blueprint types need only implement `IFactoryBlueprint`; so you may create your own subclass:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/Factories/Factory.cs#CustomBlueprintExample)]

In some cases, you may wish to pass some parameters and/or state to the blueprint when the `Create()` method is called, rather than when the blueprint is created.  For this, you should use `AdvancedFactory` instead of `Factory`.  `AdvancedFactory` is identical to `Factory` except that it lets you specify an additional type parameter which is the type of a parameter you pass to the factory's `Create` function.  This parameter is, in turn, passed to the blueprint.

Below is an example which aims to pass a Point to the `Create()` function which specifies the object's initial position:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/Factories/AdvancedFactory.cs#AdvancedFactoryExample)]

You may also implement an `AdvancedFactory` subclass if you wish, or create custom blueprints by implementing the `IAdvancedFactoryBlueprint` interface yourself, just like the above examples which use `Factory` do.