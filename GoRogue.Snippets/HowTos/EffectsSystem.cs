using GoRogue.DiceNotation;
using GoRogue.Effects;

namespace GoRogue.Snippets.HowTos
{
    #region EffectsBasicExample
    public static class EffectsBasicExample
    {
        class Monster
        {
            public int HP { get; set; }

            public Monster(int hp)
            {
                HP = hp;
            }
        }

        // Our Damage effect will need two parameters to function -- who is taking
        // the damage, eg. the target, and a damage bonus to apply to the roll.
        //
        // We don't pass any parameters to the Trigger function, however; we instead
        // pass them all as constructor parameters to the class.  This also allows us
        // to inherit from Effect instead of AdvancedEffect.
        class Damage : Effect
        {
            public readonly Monster Target;
            public readonly int DamageBonus;

            // Since our damage effect can be instantaneous or span a duration
            // (details on durations later), we take a duration and pass it along to
            // the base class constructor, as well as the parameters we need to deal
            // damage.
            public Damage(Monster target, int damageBonus, int duration)
                : base("Damage", duration)
            {
                Target = target;
                DamageBonus = damageBonus;
            }

            // Our damage is 1d6, plus the damage bonus.
            protected override void OnTrigger(out bool cancelTrigger)
            {
                // Since the parameter is an "out" parameter, we must set it to something.
                // We don't want to cancel a trigger that triggered this effect, so we
                // set it to false. In this example, we're not using the effect with
                // EffectTriggers anyway, so this parameter doesn't have any effect
                // either way.
                cancelTrigger = false;

                // Rolls 1d6 -- see Dice Rolling documentation for details
                int damageRoll = Dice.Roll("1d6");
                int totalDamage = damageRoll + DamageBonus;
                Target.HP -= totalDamage;
            }
        }

        public static void ExampleCode()
        {
            Monster myMonster = new Monster(10);

            // We'll make this an instant effect, so it happens
            // whenever (and only whenever) we call Trigger().
            Damage myDamage = new Damage(myMonster, 2, EffectDuration.Instant);
            myDamage.Trigger();
        }
    }
    #endregion

    #region EffectTriggersAndDurationsExample
    public static class EffectTriggersAndDurationsExample
    {
        class Monster
        {
            private int _hp;

            public int HP
            {
                get => _hp;
                set
                {
                    _hp = value;
                    Console.WriteLine($"An effect triggered; monster now has {_hp} HP.");
                }
            }

            public Monster(int hp)
            {
                _hp = hp;
            }
        }

        class Damage : Effect
        {
            public readonly Monster Target;
            public readonly int DamageBonus;

            public Damage(Monster target, int damageBonus, int duration)
                : base("Damage", duration)
            {
                Target = target;
                DamageBonus = damageBonus;
            }

            protected override void OnTrigger(out bool cancelTrigger)
            {
                cancelTrigger = false;

                int damageRoll = Dice.Roll("1d6");
                int totalDamage = damageRoll + DamageBonus;
                Target.HP -= totalDamage;
            }
        }

        public static void ExampleCode()
        {
            Monster myMonster = new Monster(100);
            // Effect that triggers instantaneously, so it happens only when we call Trigger
            // and cannot be added to any EffectTrigger
            Damage myDamage = new Damage(myMonster, 2, EffectDuration.Instant);
            Console.WriteLine("Triggering instantaneous effect...");
            myDamage.Trigger();

            EffectTrigger trigger = new EffectTrigger();
            // We add one 3-round damage over time effect, and one infinite damage effect.
            trigger.Add(new Damage(myMonster, 2, 3));
            trigger.Add(new Damage(myMonster, 2, EffectDuration.Infinite));

            Console.WriteLine($"\nAdded some duration-based effects; current effects: {trigger}");
            for (int round = 1; round <= 4; round++)
            {
                Console.Write($"Press enter to trigger round {round}: ");
                Console.ReadLine();

                Console.WriteLine($"Triggering round {round}....");
                trigger.TriggerEffects();
                Console.WriteLine($"\nCurrent Effects: {trigger}");
            }

        }
    }
    #endregion

    #region AdvancedEffectsExample
    public static class AdvancedEffectsExample
    {
        // This will be the class we use to pass information to our effect's Trigger function.
        // It could be a struct, but we'll use a class here so we can easily modify the
        // DamageTaken value.
        class DamageArgs
        {
            public int DamageTaken;

            public DamageArgs(int damageTaken)
            {
                DamageTaken = damageTaken;
            }
        }

        class Monster
        {
            private int _hp;

            public int HP
            {
                get => _hp;
                set
                {
                    _hp = value;
                    Console.WriteLine($"Monster took damage; it now has {_hp} HP.");
                }
            }

            public readonly AdvancedEffectTrigger<DamageArgs> DamageTrigger;

            public Monster(int hp)
            {
                _hp = hp;
                DamageTrigger = new AdvancedEffectTrigger<DamageArgs>();
            }

            public void TakeDamage(int damage)
            {
                // Create the DamageArgs object to pass to the trigger and pass it
                // to the TriggerEffects function.
                Console.WriteLine($"Damage effect dealt {damage} damage (before reduction).");
                DamageArgs args = new DamageArgs(damage);
                DamageTrigger.TriggerEffects(args);

                // Use whatever damage we get back as the damage to take
                HP -= args.DamageTaken;
            }
        }

        class DamageEffect : Effect
        {
            public readonly Monster Target;
            public readonly int DamageBonus;

            public DamageEffect(Monster target, int damageBonus, int duration)
                : base("Damage", duration)
            {
                Target = target;
                DamageBonus = damageBonus;
            }

            protected override void OnTrigger(out bool cancelTrigger)
            {
                cancelTrigger = false;

                int damageRoll = Dice.Roll("1d6");
                int totalDamage = damageRoll + DamageBonus;

                // Unlike the previous example, we'll take damage with the TakeDamage
                // function so that our DamageTrigger can trigger.
                Target.TakeDamage(totalDamage);
            }
        }

        // Note that this effect inherits from AdvancedEffect, and takes whatever
        // type we specify as an additional argument to its Trigger and OnTrigger
        // functions.
        class ArmorEffect : AdvancedEffect<DamageArgs>
        {
            public readonly float ReductionPct;

            public ArmorEffect(float reductionPct, int startingDuration)
                : base("ArmorEffect", startingDuration)
            {
                ReductionPct = reductionPct;
            }

            protected override void OnTrigger(out bool cancelTrigger, DamageArgs args)
            {
                cancelTrigger = false;

                int originalDamage = args.DamageTaken;
                args.DamageTaken = (int)(args.DamageTaken * (1f - ReductionPct));
                Console.WriteLine($"Damage taken reduced from {originalDamage} to {args.DamageTaken} by armor.");
            }
        }

        public static void ExampleCode()
        {
            Monster myMonster = new Monster(100);

            // Add a 50% armor effect
            myMonster.DamageTrigger.Add(new ArmorEffect(0.5f, EffectDuration.Infinite));

            // Trigger an effect to deal some damage
            var myDamage = new DamageEffect(myMonster, 2, EffectDuration.Instant);
            myDamage.Trigger();
        }
    }
    #endregion
}
