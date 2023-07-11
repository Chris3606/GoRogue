---
title: Effects System
---

# Overview
The Effects system exists to provide a class structure suitable for representing any "effect" in-game.  These could include dealing damage, healing a target, area-based effects, over-time effects, or even permanent/conditional modifiers.  The system provides the capability for effects to have duration in arbitrary units, from instantaneous (immediate), to infinite (activates whenever a certain event happens, forever).

# Effects
At its core, the `Effect` class is an abstract class that should be subclassed in order to define what a given effect does.  It defines an abstract `OnTrigger` method that, when called, should take all needed actions to cause a particular effect.  The (non abstract) public `Trigger()` function should be called to trigger the effect, as it calls the `OnTrigger` function, as well as decrements the remaining duration on an effect (if it is not instantaneous or infinite).

## Parameters of Effects
Different effects may need vastly different types and numbers of parameters passed to the `Trigger()` function in order to work properly.  For this reason, `Effect` takes a generic type parameter which indicates the type of the (single) argument that will be passed to the `Trigger` function.  In order to enable some advanced functionality with [EffectTriggers](#duration-of-effects-and-effecttrigger), all types used as the value for this type parameter must inherit from the class `EffectArgs`.  It is also possible to pass multiple parameters to the `Trigger` function -- you can simply create a class/struct that wraps all the values you need to pass into one type, and use that as the type parameter when subclassing.  This will be demonstrated in a later code example.

## Constructing Effects
Each effect takes a string parameter representing its name (for display purposes), and an integer variable representing its duration.  Duration (including infinite and instant duration effects), are covered in more depth below.

## Creating a Subclass
For the sake of a concise code example, we will create a small code example which takes a Monster class with an HP field, and creates an effect to apply basic damage.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/EffectsSystem.cs#EffectsBasicExample)]

# Duration of Effects and EffectTrigger
The code example above may appear to be excessively large for such a simple task; the advantage of using `Effect` for this type of functionality lies in Effect's capability for durations.  `Effect` takes as a constructor parameter an integer duration.  This duration can either be an integer constant in range `[1, int.MaxValue]`, or one of two special (static) constants.  These constants are either `Effect<T>.Instant`, which represents effects that simply take place whenever their `Trigger()` function is called and do not partake in the duration system, or `Effect<T>.Infinite`, which represents and effect that has an infinite duration.

The duration value is in no particular unit of measurement, other than "number of times `Trigger()` is called".  In fact, the duration value means very little by itself -- rather, any non-instant effect is explicitly meant to be used with an `EffectTrigger`.  `EffectTrigger` is, in essence, a highly augmented list of `Effect` instances that all take the same parameter to their `Trigger()` function.  It has a method that calls the `Trigger()` functions of all Effects in its list (which modifies the duration value for the `Effect` as appropriate), then removes any effect from the list whose durations have reached 0.  It also allows any effect in the list to "cancel" the trigger, preventing the `Trigger()` functions in subsequent effects from being called.  In this way, `EffectTrigger` provides a convenient way to manage duration-based effects.

## Creating an EffectTrigger
When we create an `EffectTrigger`, we must specify a type parameter.  This is the same type parameter that we specified when dealing with effects -- it is the type of the argument passed to the `Trigger()` function of effects it holds, and the type used must subclass `EffectArgs`.  Only `Effect` instances taking this specified type to their `Trigger()` function may be added to that `EffectTrigger`.  For example, if you have an instance of type `EffectTrigger<DamageArgs>`, only `Effect<DamageArgs>` instances may be added to it -- eg. only `Effect` instances that take an argument of type `DamageArgs` to their `Trigger()` function. 

## Adding Effects
`Effect` instances can be added to an `EffectTrigger` by calling the `Add()` function, and passing the `Effect` to add.  Such an effect will automatically have its `Trigger()` method called next time the effect trigger's [TriggerEffects](#triggering-added-effects) function is called.  If an effect with duration 0 (instant or expired duration) is added, an exception is thrown.

## Triggering Added Effects
Once effects have been added, all the effects may be triggered with a single call to the `TriggerEffects()` function.  When this function is called, all effects that have been added to the `EffectTrigger` have their `Trigger()` function called.  If any of the effects set the `CancelTrigger` field of their argument to true, the trigger is "cancelled", and no subsequent effects will have their `Trigger()` function called.

## A Code Example
In this example, we will utilize the `Damage` effect written in the previous code example to create and demonstrate instantaneous, damage-over-time, and infinite damage-over-time effects.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/EffectsSystem.cs#EffectsAdvancedExample)]

# Conditional-Duration Effects
We can also represent effects that have arbitrary, or conditional durations, via the infinite-duration capability.

For example, consider a healing effect that heals the player, but only when there is at least one enemy within a certain radius at the beginning of a turn.  We could easily implement such an effect by giving this effect infinite duration and adding it to an `EffectTrigger` that has its `TriggerEffects()` function called at the beginning of the turn.  The `OnTrigger()` implementation could do any relevant checking as to whether or not an enemy is in range.  Furthermore, if we wanted to permanently cancel this effect as soon as there was no longer an enemy within the radius, we could simply set the effect's duration to 0 in the `OnTrigger()` implementation when it does not detect an enemy, and the effect would be automatically removed from its `EffectTrigger`.