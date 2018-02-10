using System;
using System.Collections.Generic;
using System.Text;
using GoRogue.DiceNotation.Terms;

namespace GoRogue.DiceNotation
{
    public class Parser : IParser
    {
        private static Dictionary<char, int> operatorPrecedence = new Dictionary<char, int>()
        {
            ['('] = 1,
            ['+'] = 2,
            ['-'] = 2,
            ['*'] = 3,
            ['/'] = 3,
            ['k'] = 4,
            ['d'] = 5,
        };

        public IDiceExpression Parse(string diceNotation)
        {
            var postfix = toPostfix(diceNotation);
            var lastTerm = evaluatePostfix(postfix);

            return new DiceExpression(lastTerm);
        }

        // Converts dice notation to postfix notation
        private static List<string> toPostfix(string infix)
        {
            var output = new List<string>();
            var operators = new Stack<char>();

            int charIndex = 0;
            while (charIndex < infix.Length)
            {
                if (char.IsDigit(infix[charIndex])) // Is an operand
                {
                    string number = "";
                    while (charIndex < infix.Length && char.IsDigit(infix[charIndex]))
                    {
                        number += infix[charIndex];
                        charIndex++;
                    }

                    output.Add(number); // Add operand to postfix output
                }
                else // Separate so we can increment charIndex differently
                {
                    if (infix[charIndex] == '(')
                        operators.Push(infix[charIndex]);
                    else if (infix[charIndex] == ')')
                    {
                        var op = operators.Pop();
                        while (op != '(')
                        {
                            output.Add(op.ToString());
                            op = operators.Pop();
                        }
                    }
                    else if (operatorPrecedence.ContainsKey(infix[charIndex]))
                    {
                        while (operators.Count > 0 && operatorPrecedence[operators.Peek()] >= operatorPrecedence[infix[charIndex]])
                        {
                            output.Add(operators.Pop().ToString());
                        }

                        operators.Push(infix[charIndex]);
                    }

                    charIndex++;
                }
            }

            while (operators.Count != 0)
                output.Add(operators.Pop().ToString());

            return output;
        }

        // Evaluates the postfix expression, returning the final term (the one to evaluate to roll the expression)
        private static ITerm evaluatePostfix(List<string> postfix)
        {
            var operands = new Stack<ITerm>();

            foreach (var str in postfix)
            {
                if (char.IsDigit(str[0]))
                {
                    operands.Push(new ConstantTerm(int.Parse(str)));
                }
                else // Is an operator
                {
                    ITerm op1;
                    ITerm op2;

                    switch (str)
                    {
                        case "d":
                            op2 = operands.Pop();
                            op1 = operands.Pop();
                            operands.Push(new DiceTerm(op1, op2));
                            break;
                        case "*":
                            op2 = operands.Pop();
                            op1 = operands.Pop();
                            operands.Push(new MultiplyTerm(op1, op2));
                            break;
                        case "/":
                            op2 = operands.Pop();
                            op1 = operands.Pop();
                            operands.Push(new DivideTerm(op1, op2));
                            break;
                        case "+":
                            op2 = operands.Pop();
                            op1 = operands.Pop();
                            operands.Push(new AddTerm(op1, op2));
                            break;
                        case "-":
                            op2 = operands.Pop();
                            op1 = operands.Pop();
                            operands.Push(new SubtractTerm(op1, op2));
                            break;
                        case "k":
                            op2 = operands.Pop();
                            op1 = operands.Pop();
                            var diceOp = op1 as DiceTerm; // Must be preceded by a dice term
                            if (diceOp == null)
                                throw new Exceptions.InvalidSyntaxException();

                            operands.Push(new KeepTerm(op2, diceOp));
                            break;
                    }
                }
            }

            if (operands.Count != 1) // Something went awry
                throw new Exceptions.InvalidSyntaxException();

            return operands.Pop(); // Last thing is the answer!
        }
    }
}
