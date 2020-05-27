---
title: GoRogue v3 Map Generation
---

# GoRogue v3 Map Generation
The map generation framework is undergoing a complete redesign in GoRogue 3.0, as compared to GoRogue 2.x.  The new map generation framework is designed to drastically increase the flexibility of the system by making it easier to design/use modular algorithms that perform various elements of map generation.  It aims to maintain an API that is easy to get started with, but also to provide users with the ability to easily use the built-in algorithms in a custom order, and even allow users to create their own map generation methods, all within the provided sytem.

# Getting Started
Getting started with GoRogue map generation requires only a few lines of code.  GoRogue provides built-in generation algorithms to generate various types of maps in its [DefaultAlgorithms](xref:GoRogue.MapGeneration.DefaultAlgorithms) static class.  Each pre-built algorithm is a function you can call that retrives a list of [GenerationSteps](xref:GoRogue.MapGeneration.GenerationStep) that you simply pass to a generator and use to generate your map.

## Generating the Map
For example, to generate a basic roguelike dungeon with rectangular rooms connected by a maze, you would write the following code:
```CSharp
// The map will have a width of 40 and height of 50
var generator = new Generator(40, 50);
// Add the steps to generate a map using the DungeonMazeMap built-in algorithm
generator.AddSteps(DefaultAlgorithms.DungeonMazeMap());
// Generate the map
generator.Generate();
```

The functions in `DefaultAlgorithms`, including `DungeonMazeMap()`, have a number of optional arguments that allow you to control the parameters of the map generated.  Those functions' API documentation will explain exactly how the algorithms generate their maps, and what each parameter does.  You can substitute the call to `DungeonMazeMap` above for any of the functions in `DefaultAlgorithms` to generate that type of map instead.

## Accessing the Map
After the map has been generated, the data that describes the map is stored within the `generator.Context` field.  This field is effectively a collection of components that each have data pertaining to the map.  The components can vary widely based on the generation algorithm used, but all built-in generation algorithms leave a component of `ISettableMapView<bool>` on the context, where a value of `true` for a location describes a floor (walkable) tile of the map, and a value of `false` describes a wall (non-walkable) tile of the map.

So, after `generator.Generate()` above, we can access the simplest data of the map like this:
```CSharp
var wallFloorValues = generator.Context.GetComponent<ISettableMapView<bool>>();
foreach (var pos in wallFloorValues.Positions())
{
    if (wallFloorValues[pos])
        Console.WriteLine($"{pos} is a floor");
    else
        Console.WriteLine($"{pos} is a wall.");
}
```

Other data about the map, such as the locations of doors, rectangles representing rooms generated, and more can be found in other components on the context.  The components present will depend on the map generation algorithm, so refer to the `DefaultAlgorithms` function documentation, which will list the components that will be placed on the context and the data they store.

## More Customized Map Generation
Each function in `DefaultAlgorithms` provides parameters that allow you to control the parameters of the map it generates, however eventually, you may want to create more customized maps using different algorithms for one or more steps.  For example, perhaps you would like to generate mazes in a custom way, but use the `DungeonMazeMap` algorithm's method of placing rooms.  Or, perhaps you would like to create your own map generation algorithm entirely.  The map generation system allows you to execute such concepts within the framework as well -- although you'll need a more in-depth understanding of how the system works -- read on!

# Overview of the System
GoRogue's map generation system, in essence, provides 3 things:
1. **A framework**, in which you can develop arbitrary algorithms for map generation.  There are virtually no restrictions on what these algorithms can do or even what data they must work on, and thus you can develop arbitrary algorithms within the framework.
2. **Built-in map generation steps** that perform portions of typical map generation, like creating/placing rooms, cellular automata, generating tunnels/mazes, etc.  These steps are all developed within the same framework exposed to the user.  Map generation is incredibly broad, and GoRogue cannot (and does not intend to) provide an implementation of every possible way of performing an element of generation.  However, these steps may be building blocks for you to experiment with, and some may prove useful (if nothing else as a means to get started).
3. **Built-in, complete map generation algorithms**.  These are contained as functions within the `DefaultAlgorithms` class, and are nothing more than pre-built sets of built-in map generation steps.  These are designed to cover basic use cases, as well as provide a quick and easy way of getting started that does not require a full understanding of the framework.

# The Framework
Creating a framework designed to accomodate arbitrary map generation algorithms is challenging.  There are incredibly few assumptions about the types of data an algorithm will work with that apply to all, or even a significant subset of all, map generation algorithms.  In addition, if the approach becomes too convoluted or difficult to comprehend, then it isn't useful.  Therefore, the aim of the framework is to allow a flexible approach to map data, while avoiding typical traps of data flexibility like massive collections of fields, or the need to up-cast and/or assume data is present when it may not be.

## Overview
GoRogue's map generation framework takes a component-based approach to map data.  GoRogue already posesses a flexible, type-safe, and efficient system for dealing with components on objects via its [ComponentContainer](xref:GoRogue.ComponentContainer).  This system allows components to be added to an object, optionally associated with a "tag" string.  Additionally, it allows retrieval of these components by type (eg, retrieve the component(s) on this object that are of type `T`), as well as optionally by associated tag (eg. retrieve the component on this object that is of type `T` and has the given tag associated with it).  This existing system is leveraged as the core of the map generation framework.

The framework consists of a [GenerationContext](xref:GoRogue.MapGeneration.GenerationContext), which is effectively just an object that will have one or more components representing map data attached to it.

[GenerationSteps](xref:GoRogue.MapGeneration.GenerationStep), then, are self-contained steps that perform some algorithm that pertains to map generation.  Built-in steps perform functions such as placing rooms or creting tunnels, but the concept can be whatever it needs to be for a given generation method.  Generation steps take a generation context to operate on, expect 0 or more components (that contain the initial data they need to operate) to be present on that context, and then in their `OnPerform` method, add additional components to the context and/or modify existing ones to perform their operations and record their results.

A [Generator](xref:GoRogue.MapGeneration.Generator) is the final component, and is simply a wrapper around a generation context and a set of steps to apply to that context.  You create the generator (which in turn creates a context) and add one or more generation steps to that generator.  When you call `generator.Generate()`, it simply loops through all the generation steps that have been added to it (in the order they were added), and performs them.

# Default Algorithms
Usage of the default algorithms is nearly completely covered in the [getting-started](#getting-started) section.  You create a `Generator`, then call one of the functions in `DefaultAlgorithms`, which produces a set of generation steps, configured with the values passed to the function, that create a map a certain way.  You then pass the result to `Generator.AddSteps`, and call `Generator.Generate()` to perform the steps.  Each function documents the procedure used to create the map and what each parameter controls.

# Built-in Map Generation Steps
GoRogue provides each element of the default map generation algorithms as an independent `GenerationStep`, in the `GoRogue.MapGeneration.Steps` namespace.  Each step documents the components it requires to be present on the generation context it is given, and the components it modifies/adds to the context.  Using this as a guide, you can combine the steps in an arbitrary order, or combine them with your own custom generation steps (see below), to produce a map.

Most of the functions of a `Generator`, including `AddStep`, return the `this` instance, so you can chain calls of them together.  This makes it syntactically convenient to add many steps to a generator.  The following example creates a simple map generation algorithm that simply generates rooms, then draws tunnels between rooms closest to each other to connect them.

```CSharp
var generator = new Generator(mapWidth, mapHeight);

generator
    .AddStep
    (
        // Sets custom values for some parameters, leaves others at their default
        new RoomsGenerationStep()
        {
            MinRooms = 2,
            MaxRooms = 8,
            RoomMinSize = 3,
            RoomMaxSize = 9
        }
    )
    // According to the documentation, RoomsGenerationStep records the rooms it creates in an
    // ItemList<Room> component, with the tag "Rooms" (unless changed via constructor parameter).
    // However, the area connection algorithm we want to run operates on an ItemList<Area>, with
    // the tag "Areas", by default.  So, we have a "translation step" that creates areas from rectangles,
    // and adds them to a new component.  We specify the tags of components to input from and output to,
    // to match what the previous generation step creates the the next one expects
    .AddStep(new RectanglesToAreas("Rooms", "Areas"))
    // Connects areas together.  This component by default uses the component with the tag "Areas" for areas to connect,
    // so since we haven't changed it,  it will connect the areas representing our rooms.
    .AddStep(new ClosestMapAreaConnection());

// Execute the steps we added to generate the map
generator.Generate();
```

## Tags for Components
The steps that are built-in to GoRogue can all operate on components with a specific tag.  They'll have reasonable defaults, but allow you to customize them via constructor parameter (as we did with `RectanglesToAreas` in the above example).  In most cases, `null` can be specified as the tag to indicate that any object of the correct type should qualify (although doing this is not recommended and this may be removed before the end of alpha).

This allows you to "chain" steps together in order to get one step to operate on the output of another step.  You simply specify the tag that a previous step used as output, and that step will operate on the component with that tag.  In the example above, we use this to get our `RectanglesToAreas` step to translate the rectangles created by `RoomsGeneration`.

## Required Component Management
Since the built-in generation steps must often require that one or more components be present on the context they are given (to allow them to take input data), one challenge is how to document this well and produce useful help messages if an error is made.

Each generation step will have in its class API documentation a list of components that it requires.  Similarly, it will document what component(s) it creates and/or uses as output.  This documentation should present an easy way to determine what the inputs and outputs are.

If a step cannot find one of its required components, a `MissingContextComponentException` is raised, with a detailed exception message that tells you exactly what step required what component that it could not find.  This exception will be raised as soon as `generator.Generate()` is called.  The built-in steps also avoid very tricky scenarios like components that are "conditionally" required -- a component is either required or it is not.  These features should make debugging algorithms composed of these steps much easier.

[!NOTE]
> The framework allowing these messages is exposed to the user, and can be very useful when creating your own generation steps.  This will be covered in a later section.

## Configuration of Steps
Each built-in generation step may take in some configuration parameters that dictate how it operates.  For example, `RoomsGenerator`, which is used in the above example, takes in a number of these such as `MinRooms` and `MaxRooms`.  To avoid massive sets of constructor parameters, initial values for these fields are not passed to a generation step's constructor unless absolutely necessary (eg, no functional default can be provided).  Instad, built-in steps will expose these as public fields on the generation step class, and the field's documentation will state what the field controls.  The documentation will also note the field's default if no custom value is set.

In the example above, we use C#'s convenient initializer list syntax to pass custom values for some of the configuration parameters of `RoomGeneration`.  This provides an easy way to configure generation steps without requiring that you create a massive function call or code block to configure only a few parameters.

If an improper value is given to a field, as soon as `generator.Generate` is called an `InvalidConfigurationException` will be raised.  This exception will have a detailed exception messages that specifies exactly what value of what generation step was invalid, and what the restrictions on that value are.

[!NOTE]
> The exception constructor allowing these exception messages is exposed to the user, and can be very useful when creating your own generation steps.

## Translation Steps
Particularly when using steps in customized arrangements, or creating your own steps, it can be necessary to "translate" data from one form to another to allow steps to operate with each other.  For example, you might want to join two lists together, or translate `Rectangles` to `Areas` like we did in the example above.  For these steps, a few different "translation steps" are provided in the `GoRogue.MapGeneration.Steps.Translation` namespace.  These are steps whose sole purpose is to translate data in this way.

In the example above, we use the `RectanglesToAreas` translation step to take the `ItemList<Rectangle>` component created by the `RoomsGeneration` step, and translate it to an `ItemList<Area>` that the `ClosestMapAreaConnector` can understand.

[!NOTE]
> This particular example (involving translating `Rectangles` to `Areas` may be alleviated later by changing the output of room-generating components to some more generic interface such as area).  In any case, however, there will still be times where data translation has to take place: `DefaultAlgorithms.DungeonMazeMap` is a good example of this.

# Creating Custom Generation Algorithms
Creating completely custom map generation algorithms involves simply implementing one or more `GenerationSteps` that represent the steps to take to generate the map.  This simply involves creating a class that inherits from `GenerationStep` and implements its required method `OnPerform` to actually perform the generation step and add its results to the context.  However, there are a number of paradigms that are common when doing this, and as such there are a number of features and helper methods in `GenerationContext` and `GenerationStep` to help with this.

## Requiring Components to be Present on the Generation Context
When creating custom generation steps, it is common to require certain data as input.  For example, a generation step that connects areas of a map to each other might need to take as input a list of areas to connect and/or a map view representing tiles as wall/floor.  In GoRogue's map generation framework, this requirement is represented by requiring the generation context to have a component that contains the data a step needs when it is called upon to perform its work.

This method could lead to a lot of repetitive code in generation steps to the effect of `if(!context.HasComponent<Type1>() || !context.HasComponent<Type2>()) throw Exception("message")` in generation steps, and as well can lead to not-helpful exceptions (`NullReferenceExcpeption`, potentially) if the checks are forgotten.  To alleviate this, `GenerationStep` implements functionality that allows you to express this pattern of requiring components easily.  You simply pass the types (and, optionally, tags) of the components that are required to the constructor, and it will check for them automatically when `GenerationStep.Perform` is called.  If a component is missing, a detailed exception showing which component is missing is raised.  Since all of this happens before the virtual method `GenerationStep.OnPerform` (where you implement the step) is called, this method can safely assume that the components it wants are present.

The following is an example generation step that utilizes this functionality:
```CSharp
public class MyGenerationStep : GenerationStep
{
    public MyGenerationStep()
        // This generation step is requiring the context to have a component of type RequiredComponentType1, that has the tag
        // "RequiredComponentType1".  Additionally, it is required to have a component of type "RequiredComponentType2", with no particular tag
        // (any object of that type, with or without a tag, will work)
        : base((typeof(RequiredComponentType1), "RequiredComponent1Tag"), (typeof(RequiredComponent2), null) { }

    public void OnPerform(GenerationContext context)
    {
        // Both of these are guaranteed to not fail/return null, because the components were specified in the constructor as required
        var requiredComponent1 = context.GetComponent<RequiredComponentType1>("RequiredComponent1Tag");
        var requiredComponent2 = context.GetComponent<RequiredComponentType2>();

        // Do generation
    }
}
```

## Creating New Components Only if Not Present
Some generation steps might want to use an existing component on the context, if an appropriate one is present, but if it isn't there, then add a new one.  For example, an algorithm that creates and places rooms on a map might require a map view it can set walkable tiles to, and a list of rooms to add the rooms it generates to.  However, instead of outright requiring those components to be present, since it doesn't actually _need_ any of the data for them for inputs (it's just for storing results), it's more convenient if the algorithm simply creates and adds those components if they're not already there.  Since this again leads to repetitive code, the helper method `GenerationContext.GetComponentOrNew` exists for this.  It is exactly like `GenerationContext.GetComponent`, except it also takes a function that returns a new object of the proper type, and if it can't find the component it calls the function, and adds the component it returns to the context, then returns that component.

The following example uses this functionality to "require" two components:
```CSharp
public class MyGenerationStep : GenerationStep
{
    public void OnPerform(GenerationContext context)
    {
        // Get an ItemList<Room> component with the tag "Room" if it exists, otherwise create a new one, add it, and return that
        var roomsList = context.GetComponentOrNew(() => new ItemList<Room>(), "Rooms");
        // Get an ISettableMapView<bool> component with the tag "WallFloor" if it exists, otherwise create a new ArrayMap of the appropriate size
        // (which is a subclass of ISettableMapView), add it, and return that
        var wallFloor = context.GetComponentOrNew<ISettableMapView<bool>>(() => new ArrayMap<bool>(context.Width, context.Height), "WallFloor");

        // Do generation
    }
}
```

## Lists of Items
Basic map views might form relatively common components for generation contexts, but it is also common to want to record a list of items.  For example, a generation step that creates/places rooms in a map might want to add a list of `Rectangles` representing the rooms that it creates.  A simple `List<Rectangle>` component would suffice, but it can also be useful to record which generation step added which item in the list.  For this, the framework provides generation step names and the [ItemList<T>](xref:GoRogue.MapGeneration.ContextComponents.ItemList{T}) class.

Each generation step has a `Name` field.  If not given a value via the constructor, it defaults to the subclass's name.  For example, in the case of a class `public class MyGenerationStep : GenerationStep`, if a name was not given to the `base` constructor, the step's `Name` field would be initialized to `"MyGenerationStep"`.  Most built-in generation steps, like the base class, will optionally take a `Name` value as a parameter and simply pass it along if given.  Although this is not required, it can be helpful as it allows a user of the step to specify a custom name for it.

This `Name` field is used by `ItemList`.  An `ItemList<T>` is basically just an augmented `List<T>`, that takes the `Name` of the generation step adding items to it as a parameter.  It keeps a normal list of items but also keeps track of which step added which item, and allows you to retrieve this information later.