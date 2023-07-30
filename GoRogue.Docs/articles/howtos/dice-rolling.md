---
title: Dice Rolling
---

# Dice Notation Parser
GoRogue contains a versatile [dice notation](https://en.wikipedia.org/wiki/Dice_notation) parser that allows you to roll virtual dice.  The parser allows you to enter complex dice notation expressions, and emulate the result of rolling them via GoRogue's random number generation framework.

# Rolling Dice Expressions
The simplest way to use the dice roller is to use the provided `Roll` method:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/DiceNotation.cs#BasicRolling)]

# Valid Expressions
The dice expression parser is robust and supports most typical dice notation syntax, which is based around the following string format:

```CSharp
int numberOfDice = 10;
int sidesOnDie = 20;
string valid = $"{numberOfDice}d{sidesOnDie}";
```

Some examples of valid expressions to roll include:
* Roll 3 six-sided dice: `3d6`
* Roll an eight-sided die and add 2 to the result: `1d8+2`
* Roll a twelve-sided die, double the result, and add 3: `d12*2+3`
* Roll a twelve-sided die, halve the result, and subtract 1: `1d12/2-1`
* Roll 10 ten-sided die, and only keep the top three: `10d10k3`
* Roll 4 six-sided die, add 1 to the entire roll, and only keep the top three: `4d6+1k3`

The parser is implemented as a fully functional recursive-descent parser, and supports the following operations, in order of precedence:
* Parenthesization (eg. `1d12+(3*2)`)
* Addition/Subtraction - Supports arbitrary terms as operands (eg `(3+2)d10` is valid)
* Multiplication/Division - Supports arbitrary terms as operands
* Keep Operation (`k<n>`) - Keeps only top `n` rolls from a dice roll result
* Dice Terms (`<num_dice>d<num_sides>`)

# Using Custom Parsers
If you need to add support for an operand that is not supported by the default parser, the `Dice.DiceParser` property is settable, and is of the abstract type `IParser`.  Whatever `IParser` implementation is set to that property is used for the `Roll` and `Parse` methods.

All of the terms for existing operators are defined as distinct classes, and the `DiceNotation.Parser` implementation, which serves as the default `IParser` implementation, is fairly straightforward.  In the future the parsing code may be broken out into more modular pieces that are more easily re-usable (without copy-paste), but for now, basing custom parsing code off of the `Parser` class is the intended approach.

# Repeating/Obtaining Attributes About Rolls
If you are repeating a roll many times, or need attributes about a roll other than its result, you may want to call the `Dice.Parse` function instead of `Dice.Roll`.  This returns you a `DiceExpression`, which you can use to roll the dice multiple times without re-parsing the dice notation string.  `DiceExpression` also allows you to retrieve useful information about the expression:

[!code-csharp[](../../../GoRogue.Snippets/HowTos/DiceNotation.cs#DiceExpression)]

You can also inspect the actual terms within the expression tree of the dice expression via the `DiceExpression.RootTerm` field.
