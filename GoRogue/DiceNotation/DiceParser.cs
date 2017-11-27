﻿using System;
using System.Text.RegularExpressions;

namespace GoRogue.DiceNotation
{
    /// <summary>
    /// Used to parse a string into a DiceExpression.
    /// </summary>
    public class DiceParser : IDiceParser
    {
        private readonly Regex whitespacePattern;

        /// <summary>
        /// Constructs a new instance of the DiceParser class.
        /// </summary>
        public DiceParser()
        {
            whitespacePattern = new Regex(@"\s+");
        }

        /// <summary>
        /// Create a new DiceExpression by parsing the specified string.
        /// </summary>
        /// <param name="expression">A dice notation string expression. Ex. 3d6+3.</param>
        /// <returns>A DiceExpression parsed from the specified string.</returns>
        /// <exception cref="ArgumentException">Invalid dice notation supplied.</exception>
        public DiceExpression Parse(string expression)
        {
            if (string.IsNullOrEmpty(expression))
            {
                throw new ArgumentException("A dice notation expression must be supplied.", nameof(expression));
            }

            string cleanExpression = whitespacePattern.Replace(expression.ToLower(), "");
            cleanExpression = cleanExpression.Replace("+-", "-");

            var parseValues = new ParseValues().Init();

            var dice = new DiceExpression();

            for (int i = 0; i < cleanExpression.Length; ++i)
            {
                char c = cleanExpression[i];

                if (char.IsDigit(c))
                {
                    parseValues.Constant += c;
                }
                else if (c == '*')
                {
                    parseValues.Scalar *= int.Parse(parseValues.Constant);
                    parseValues.Constant = "";
                }
                else if (c == 'd')
                {
                    if (parseValues.Constant.Length == 0)
                    {
                        parseValues.Constant = "1";
                    }
                    parseValues.Multiplicity = int.Parse(parseValues.Constant);
                    parseValues.Constant = "";
                }
                else if (c == 'k')
                {
                    string chooseAccum = "";
                    while (i + 1 < cleanExpression.Length && char.IsDigit(cleanExpression[i + 1]))
                    {
                        chooseAccum += cleanExpression[i + 1];
                        ++i;
                    }
                    parseValues.Choose = int.Parse(chooseAccum);
                }
                else if (c == '+')
                {
                    append(dice, parseValues);
                    parseValues = new ParseValues().Init();
                }
                else if (c == '-')
                {
                    append(dice, parseValues);
                    parseValues = new ParseValues().Init();
                    parseValues.Scalar = -1;
                }
                else
                {
                    throw new ArgumentException("Invalid character in dice expression", nameof(expression));
                }
            }
            append(dice, parseValues);

            return dice;
        }

        private static void append(DiceExpression dice, ParseValues parseValues)
        {
            int constant = int.Parse(parseValues.Constant);
            if (parseValues.Multiplicity == 0)
            {
                dice.Constant(parseValues.Scalar * constant);
            }
            else
            {
                dice.Dice(parseValues.Multiplicity, constant, parseValues.Scalar, parseValues.Choose);
            }
        }

        private struct ParseValues
        {
            public string Constant;
            public int Scalar;
            public int Multiplicity;
            public int? Choose;

            public ParseValues Init()
            {
                Scalar = 1;
                Constant = "";
                return this;
            }
        }
    }
}