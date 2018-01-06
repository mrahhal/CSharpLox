using System;
using System.Collections.Generic;
using static CSharpLox.TokenType;

namespace CSharpLox
{
	public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
	{
		private EnvironmentScope _environment = new EnvironmentScope();

		private readonly ILogger _logger;

		public Interpreter(
			ILogger logger)
		{
			_logger = logger;
		}

		public void Interpret(List<Stmt> statements)
		{
			try
			{
				foreach (var statement in statements)
				{
					Execute(statement);
				}
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
			var value = Evaluate(expr.Value);

			_environment.Assign(expr.Name, value);
			return value;
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
			var left = Evaluate(expr.Left);

			if (expr.Operator.Type == OR)
			{
				if (IsTruthy(left)) return left;
			}
			else
			{
				if (!IsTruthy(left)) return left;
			}

			return Evaluate(expr.Right);
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
			return _environment.Get(expr.Name);
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

		public object VisitBlockStmt(Stmt.Block stmt)
		{
			ExecuteBlock(stmt.Statements, new EnvironmentScope(_environment));
			return null;
		}

		private void ExecuteBlock(List<Stmt> statements, EnvironmentScope environment)
		{
			var previous = _environment;
			try
			{
				_environment = environment;

				foreach (var statement in statements)
				{
					Execute(statement);
				}
			}
			finally
			{
				_environment = previous;
			}
		}

		public object VisitClassStmt(Stmt.Class stmt)
		{
			throw new NotImplementedException();
		}

		public object VisitExpressionStmt(Stmt.Expression stmt)
		{
			Evaluate(stmt.InnerExpression);
			return null;
		}

		public object VisitFunctionStmt(Stmt.Function stmt)
		{
			throw new NotImplementedException();
		}

		public object VisitIfStmt(Stmt.If stmt)
		{
			if (IsTruthy(Evaluate(stmt.Condition)))
			{
				Execute(stmt.ThenBranch);
			}
			else if (stmt.ElseBranch != null)
			{
				Execute(stmt.ElseBranch);
			}
			return null;
		}

		public object VisitPrintStmt(Stmt.Print stmt)
		{
			var value = Evaluate(stmt.InnerExpression);
			Console.WriteLine(Stringify(value));
			return null;
		}

		public object VisitReturnStmt(Stmt.Return stmt)
		{
			throw new NotImplementedException();
		}

		public object VisitVarStmt(Stmt.Var stmt)
		{
			object value = null;
			if (stmt.Initializer != null)
			{
				value = Evaluate(stmt.Initializer);
			}

			_environment.Define(stmt.Name.Lexeme, value);
			return null;
		}

		public object VisitWhileStmt(Stmt.While stmt)
		{
			throw new NotImplementedException();
		}

		private void Execute(Stmt stmt)
		{
			stmt.Accept(this);
		}
	}
}
