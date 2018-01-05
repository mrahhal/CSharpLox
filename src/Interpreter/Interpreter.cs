using System;
using static CSharpLox.TokenType;

namespace CSharpLox
{
	public class Interpreter : Expr.IVisitor<object>
	{
		private readonly ILogger _logger;

		public Interpreter(
			ILogger logger)
		{
			_logger = logger;
		}

		public void Interpret(Expr expression)
		{
			try
			{
				var value = Evaluate(expression);

				// TODO: Return this instead?
				Console.WriteLine(Stringify(value));
			}
			catch (RuntimeError error)
			{
				_logger.RuntimeError(error);
			}
		}

		private String Stringify(object obj)
		{
			if (obj == null) return "nil";

			if (obj is double)
			{
				var text = obj.ToString();
				if (text.EndsWith(".0"))
				{
					text = text.Substring(0, text.Length - 2);
				}
				return text;
			}

			return obj.ToString();
		}

		public object VisitAssignExpr(Expr.Assign expr)
		{
			throw new NotImplementedException();
		}

		public object VisitBinaryExpr(Expr.Binary expr)
		{
			var left = Evaluate(expr.Left);
			var right = Evaluate(expr.Right);

			switch (expr.Operator.Type)
			{
				case GREATER:
					CheckNumberOperands(expr.Operator, left, right);
					return (double)left > (double)right;

				case GREATER_EQUAL:
					CheckNumberOperands(expr.Operator, left, right);
					return (double)left >= (double)right;

				case LESS:
					CheckNumberOperands(expr.Operator, left, right);
					return (double)left < (double)right;

				case LESS_EQUAL:
					CheckNumberOperands(expr.Operator, left, right);
					return (double)left <= (double)right;

				case BANG_EQUAL: return !IsEqual(left, right);
				case EQUAL_EQUAL: return IsEqual(left, right);

				case MINUS:
					CheckNumberOperands(expr.Operator, left, right);
					return (double)left - (double)right;

				case PLUS:
					if (left is double && right is double)
					{
						return (double)left + (double)right;
					}

					if (left is string && right is string)
					{
						return (string)left + (string)right;
					}

					throw new RuntimeError(expr.Operator, "Operands must be two numbers or two strings.");

				case SLASH:
					CheckNumberOperands(expr.Operator, left, right);
					return (double)left / (double)right;

				case STAR:
					CheckNumberOperands(expr.Operator, left, right);
					return (double)left * (double)right;
			}

			// Unreachable.
			return null;
		}

		public object VisitCallExpr(Expr.Call expr)
		{
			throw new NotImplementedException();
		}

		public object VisitGetExpr(Expr.Get expr)
		{
			throw new NotImplementedException();
		}

		public object VisitGroupingExpr(Expr.Grouping expr)
		{
			return Evaluate(expr.Expression);
		}

		public object VisitLiteralExpr(Expr.Literal expr)
		{
			return expr.Value;
		}

		public object VisitLogicalExpr(Expr.Logical expr)
		{
			throw new NotImplementedException();
		}

		public object VisitSetExpr(Expr.Set expr)
		{
			throw new NotImplementedException();
		}

		public object VisitSuperExpr(Expr.Super expr)
		{
			throw new NotImplementedException();
		}

		public object VisitThisExpr(Expr.This expr)
		{
			throw new NotImplementedException();
		}

		public object VisitUnaryExpr(Expr.Unary expr)
		{
			var right = Evaluate(expr.Right);

			switch (expr.Operator.Type)
			{
				case BANG:
					return !IsTruthy(right);

				case MINUS:
					return -(double)right;
			}

			// Unreachable.
			return null;
		}

		public object VisitVariableExpr(Expr.Variable expr)
		{
			throw new NotImplementedException();
		}

		private Object Evaluate(Expr expr)
		{
			return expr.Accept(this);
		}

		private bool IsTruthy(object obj)
		{
			switch (obj)
			{
				case null: return false;
				case bool boolean: return boolean;
				default: return true;
			}
		}

		private bool IsEqual(object a, object b)
		{
			// nil is only equal to nil.
			if (a == null && b == null) return true;
			if (a == null) return false;

			return a.Equals(b);
		}

		private void CheckNumberOperand(Token op, object operand)
		{
			if (operand is double) return;
			throw new RuntimeError(op, "Operand must be a number.");
		}

		private void CheckNumberOperands(Token op, object left, object right)
		{
			if (left is double && right is Double) return;
			throw new RuntimeError(op, "Operands must be numbers.");
		}
	}
}
