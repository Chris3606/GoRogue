using GoRogue.DiceNotation.Terms;
using System.Collections.Generic;

namespace GoRogue.DiceNotation
{
	/// <summary>
	/// Default class for parsing a string representing a dice expression into an <see cref="IDiceExpression"/> instance.
	/// </summary>
	public class Parser : IParser
	{
		// Defines operator priorities
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

		/// <summary>
		/// Parses the dice expression spcified into an <see cref="IDiceExpression"/> instance.
		/// </summary>
		/// <remarks>
		/// Breaks the dice expression into postfix form, and evaluates the postfix expression to the
		/// degree necessary to produce the appropriate chain of <see cref="ITerm"/> instances.
		/// </remarks>
		/// <param name="expression">The expression to parse.</param>
		/// <returns>
		/// An <see cref="IDiceExpression"/> representing the given expression, that can "roll" the expression on command.
		/// </returns>
		public IDiceExpression Parse(string expression)
		{
			var postfix = toPostfix(expression);
			var lastTerm = evaluatePostfix(postfix);

			return new DiceExpression(lastTerm);
		}

		// Evaluates the postfix expression, returning the final term (the one to evaluate to roll
		// the expression)
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
	}
}