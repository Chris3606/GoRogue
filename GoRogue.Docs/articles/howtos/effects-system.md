---
title: Effects System
---

# Effects System
The Effects system exists to provide a class structure suitable for representing any "effect" in-game.  These could include dealing damage, mitigating damage, healing a target, area-based effects, over-time effects, or even permanent/conditional modifiers.  The system provides the capability for effects to have duration in arbitrary units, from instantaneous (immediate), to infinite (activates whenever a certain event happens, forever or until manually removed).

# Effects
At its core, the `Effect` class is an abstract class that should be subclassed in order to define what a given effect does.  It defines an abstract `OnTrigger` method that, when called, should take all needed actions to cause a particular effect.  The (non abstract) public `Trigger()` function should be called to trigger the effect, as it calls the `OnTrigger` function, as well as decrements the remaining duration on an effect (if it is not instantaneous or infinite).

## Parameters of Effects
`Effect` doesn't allow you to pass any sort of input data to the `Trigger()` function.  In many cases, this isn't an issue, because more often than not parameters that have to do with effects can instead be given to that effect as constructor properties, rather than at the time of trigger.  If you do need to pass a parameter to `Trigger()`, however, `AdvancedEffect` allows this.  `AdvancedEffect` takes a generic type parameter which indicates the type of the (single) argument that will be passed to the `Trigger` function.  It is also possible to pass multiple parameters to the `Trigger` function -- you can simply create a class/struct that wraps all the values you need to pass into one type, and use that as the type parameter when subclassing.

## Constructing Effects
Each effect takes a string parameter representing its name (for display purposes), and an integer variable representing its duration.  Duration (including infinite and instant duration effects), are covered in more depth below.

# Basic Example
For the sake of a concise code example, we will create a small code example which takes a Monster class with an HP field, and creates an effect to apply basic damage.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/EffectsSystem.cs#EffectsBasicExample)]

# Duration of Effects and EffectTrigger
The code example above may appear to be excessively large for such a simple task.  However, one of the advantages of using using `Effect` for this type of functionality is that the effects system has built-in support for durations.  `Effect` takes as a constructor parameter an integer duration.  This duration can either be an integer value in range `[1, int.MaxValue]`, or one of two special (static) constants.  These constants are either `EffectDuration.Instant`, which represents effects that simply take place whenever their `Trigger()` function is called and do not partake in the duration system, or `EffectDuration.Infinite`, which represents and effect that has an infinite duration.

The duration value is in no particular unit of measurement, other than "number of times `Trigger()` is called".  In fact, the duration value means very little by itself -- rather, any non-instant effect is explicitly meant to be used with an `EffectTrigger`.  `EffectTrigger` is, in essence, a highly augmented list of `Effect` instances.  It has a method that calls the `Trigger()` functions of all Effects in its list (which modifies the duration value for the `Effect` as appropriate), then removes any effect from the list whose durations have reached 0.  It also allows any effect in the list to "cancel" the trigger, preventing the `Trigger()` functions in subsequent effects from being called.  In this way, `EffectTrigger` provides a convenient way to manage duration-based effects.

## Adding Effects
`Effect` instances can be added to an `EffectTrigger` by calling the `Add()` function, and passing the `Effect` to add.  Such an effect will automatically have its `Trigger()` method called next time the effect trigger's [TriggerEffects](#triggering-added-effects) function is called.  If an effect with duration 0 (instant or expired duration) is added, an exception is thrown.

## Triggering Added Effects
Once effects have been added, all the effects may be triggered with a single call to the `TriggerEffects()` function.  When this function is called, all effects that have been added to the `EffectTrigger` have their `Trigger()` function called.  If any of the effects set the `cancelTrigger` boolean value they receive to true, the trigger is "cancelled", and no subsequent effects in that `EffectTrigger` will have their `Trigger()` function called.

## A Code Example
In this example, we will utilize the `Damage` effect written in the previous code example to create an `EffectTrigger` and demonstrate its support for instantaneous, damage-over-time, and infinite damage-over-time effects.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/EffectsSystem.cs#EffectTriggersAndDurationsExample)]

For reference, the output of the above code is something like this:

```console
Triggering instantaneous effect...
An effect triggered; monster now has 96 HP.

Added some duration-based effects; current effects: [Damage: 3 duration remaining, Damage: Infinite duration remaining]
Press enter to trigger round 1:
Triggering round 1....
An effect triggered; monster now has 92 HP.
An effect triggered; monster now has 86 HP.

Current Effects: [Damage: 2 duration remaining, Damage: Infinite duration remaining]
Press enter to trigger round 2:
Triggering round 2....
An effect triggered; monster now has 79 HP.
An effect triggered; monster now has 72 HP.

Current Effects: [Damage: 1 duration remaining, Damage: Infinite duration remaining]
Press enter to trigger round 3:
Triggering round 3....
An effect triggered; monster now has 69 HP.
An effect triggered; monster now has 65 HP.

Current Effects: [Damage: Infinite duration remaining]
Press enter to trigger round 4:
Triggering round 4....
An effect triggered; monster now has 59 HP.

Current Effects: [Damage: Infinite duration remaining]
```

# Conditional-Duration Effects
We can also represent effects that have arbitrary, or conditional durations, via the infinite-duration capability.

For example, consider a healing effect that heals the player, but only when there is at least one enemy within a certain radius at the beginning of a turn.  We could easily implement such an effect by giving this effect infinite duration and adding it to an `EffectTrigger` that has its `TriggerEffects()` function called at the beginning of the turn.  The `OnTrigger()` implementation could do any relevant checking as to whether or not an enemy is in range.  Furthermore, if we wanted to permanently cancel this effect as soon as there was no longer an enemy within the radius, we could simply set the effect's duration to 0 in the `OnTrigger()` implementation when it does not detect an enemy, and the effect would be automatically removed from its `EffectTrigger`.


# Passing Parameters to the Trigger Function
In the case above, we passed the damage bonus and target parameters to the effect in its constructor.  This works well for many use cases, when the parameters are part of the effect itself.  However, in other use cases, we may want to pass parameters to the `OnTrigger()` function of the effect.  This is typically the case when the effect is being used with a trigger, and the parameter is something to do with the trigger event itself, rather than the effect.

A good example of this might be an "armor" effect, that is called via a trigger which triggers whenever damage is taken.  The "armor" effect should reduce incoming damage by a fixed percentage.  For this, the effect needs to know how much damage the target is taking; and this isn't known until the effect is triggered, so it can't be specified when the effect is created.

`Effect` and `EffectTrigger` do not support this use case.  Instead, GoRogue contains `AdvancedEffect` and `AdvancedEffectTrigger` classes to support this.  These are identical to `Effect` and `EffectTrigger`, respectively, except that they take a type parameter which specifies the type of an arbitrary argument that must be provided to their `Trigger` and `TriggerEffects` functions.

The following code uses this functionality to implement an "armor" effect like we described above.

[!code-csharp[](../../../GoRogue.Snippets/HowTos/EffectsSystem.cs#AdvancedEffectsExample)]

The output of this code will look something like this:

```console
Damage effect dealt 4 damage (before reduction).
Damage taken reduced from 4 to 2 by armor.
Monster took damage; it now has 98 HP.
```

Note that the type parameter given to an `AdvancedEffectTrigger` must also be the same type parameter given to any `AdvancedEffect` added to it.