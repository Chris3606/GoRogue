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
        // Thus, we wrap these in one class so an instance may be passed to Trigger.
        class DamageArgs : EffectArgs
        {
            public Monster Target { get; }
            public int DamageBonus {get; }

            public DamageArgs(Monster target, int damageBonus)
            {
                Target = target;
                DamageBonus = damageBonus;
            }
        }

        // We inherit from Effect<T>, where T is the type of the
        // argument we want to pass to the Trigger function.
        class Damage : Effect<DamageArgs>
        {
            // Since our damage effect can be instantaneous or
            // span a duration (details on durations later),
            // we take a duration and pass it along to the base
            // class constructor.
            public Damage(int duration)
                : base("Damage", duration)
            { }

            // Our damage is 1d6, plus the damage bonus.
            protected override void OnTrigger(DamageArgs args)
            {
                // Rolls 1d6 -- see Dice Rolling documentation for details
                int damageRoll = Dice.Roll("1d6");
                int totalDamage = damageRoll + args.DamageBonus;
                args.Target.HP -= totalDamage;
            }
        }

        public static void ExampleCode()
        {
            Monster myMonster = new Monster(10);
            // Effect that triggers instantaneously -- details later
            Damage myDamage = new Damage(Damage.Instant);
            // Instant effect, so it happens whenever we call Trigger
            myDamage.Trigger(new DamageArgs(myMonster, 2));
        }
    }
    #endregion

    #region AdvancedEffectsExample
    public static class AdvancedEffectsExample
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
                HP = hp;
            }
        }

        class DamageArgs : EffectArgs
        {
            public Monster Target { get; }
            public int DamageBonus {get; }

            public DamageArgs(Monster target, int damageBonus)
            {
                Target = target;
                DamageBonus = damageBonus;
            }
        }

        class Damage : Effect<DamageArgs>
        {
            public Damage(int duration)
                : base("Damage", duration)
            { }

            protected override void OnTrigger(DamageArgs args)
            {
                int damageRoll = Dice.Roll("1d6");
                int totalDamage = damageRoll + args.DamageBonus;
                args.Target.HP -= totalDamage;
            }
        }

        public static void ExampleCode()
        {
            Monster myMonster = new Monster(40);
            // Effect that triggers instantaneously, so it happens only when we call Trigger
            // and cannot be added to any EffectTrigger
            Damage myDamage = new Damage(Damage.Instant);
            Console.WriteLine("Triggering instantaneous effect...");
            myDamage.Trigger(new DamageArgs(myMonster, 2));

            EffectTrigger<DamageArgs> trigger = new EffectTrigger<DamageArgs>();
            // We add one 3-round damage over time effect, one infinite damage effect.
            trigger.Add(new Damage(3));
            trigger.Add(new Damage(Damage.Infinite));

            Console.WriteLine($"Current Effects: {trigger}");
            for (int round = 1; round <= 4; round++)
            {
                Console.WriteLine($"Enter a character to trigger round {round}");
                Console.ReadLine();

                Console.WriteLine($"Triggering round {round}....");
                trigger.TriggerEffects(new DamageArgs(myMonster, 2));
                Console.WriteLine($"Current Effects: {trigger}");
            }

        }
    }
    #endregion
}
