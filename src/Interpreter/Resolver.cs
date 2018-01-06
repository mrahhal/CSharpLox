using System;
using System.Collections.Generic;

namespace CSharpLox
{
	public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
	{
		private readonly Interpreter _interpreter;
		private readonly ILogger _logger;
		private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();

		public Resolver(
			Interpreter interpreter,
			ILogger logger)
		{
			_interpreter = interpreter;
			_logger = logger;
		}

		public void Resolve(List<Stmt> statements)
		{
			foreach (var statement in statements)
			{
				Resolve(statement);
			}
		}

		public object VisitAssignExpr(Expr.Assign expr)
		{
			Resolve(expr.Value);
			ResolveLocal(expr, expr.Name);
			return null;
		}

		public object VisitBinaryExpr(Expr.Binary expr)
		{
			Resolve(expr.Left);
			Resolve(expr.Right);
			return null;
		}

		public object VisitBlockStmt(Stmt.Block stmt)
		{
			BeginScope();
			Resolve(stmt.Statements);
			EndScope();
			return null;
		}

		public object VisitCallExpr(Expr.Call expr)
		{
			Resolve(expr.Callee);

			foreach (var argument in expr.Arguments)
			{
				Resolve(argument);
			}

			return null;
		}

		public object VisitClassStmt(Stmt.Class stmt)
		{
			throw new NotImplementedException();
		}

		public object VisitExpressionStmt(Stmt.Expression stmt)
		{
			Resolve(stmt.InnerExpression);
			return null;
		}

		public object VisitFunctionStmt(Stmt.Function stmt)
		{
			Declare(stmt.Name);
			Define(stmt.Name);

			ResolveFunction(stmt);
			return null;
		}

		public object VisitGetExpr(Expr.Get expr)
		{
			throw new NotImplementedException();
		}

		public object VisitGroupingExpr(Expr.Grouping expr)
		{
			Resolve(expr.Expression);
			return null;
		}

		public object VisitIfStmt(Stmt.If stmt)
		{
			Resolve(stmt.Condition);
			Resolve(stmt.ThenBranch);
			if (stmt.ElseBranch != null) Resolve(stmt.ElseBranch);
			return null;
		}

		public object VisitLiteralExpr(Expr.Literal expr)
		{
			return null;
		}

		public object VisitLogicalExpr(Expr.Logical expr)
		{
			Resolve(expr.Left);
			Resolve(expr.Right);
			return null;
		}

		public object VisitPrintStmt(Stmt.Print stmt)
		{
			Resolve(stmt.InnerExpression);
			return null;
		}

		public object VisitReturnStmt(Stmt.Return stmt)
		{
			if (stmt.Value != null)
			{
				Resolve(stmt.Value);
			}

			return null;
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
			Resolve(expr.Right);
			return null;
		}

		public object VisitVariableExpr(Expr.Variable expr)
		{
			if (_scopes.Count != 0 && _scopes.Peek()[expr.Name.Lexeme] == false)
			{
				_logger.Error(expr.Name,
					"Cannot read local variable in its own initializer.");
			}

			ResolveLocal(expr, expr.Name);
			return null;
		}

		public object VisitVarStmt(Stmt.Var stmt)
		{
			Declare(stmt.Name);
			if (stmt.Initializer != null)
			{
				Resolve(stmt.Initializer);
			}
			Define(stmt.Name);
			return null;
		}

		public object VisitWhileStmt(Stmt.While stmt)
		{
			Resolve(stmt.Condition);
			Resolve(stmt.Body);
			return null;
		}

		private void BeginScope()
		{
			_scopes.Push(new Dictionary<string, bool>());
		}

		private void EndScope()
		{
			_scopes.Pop();
		}

		private void Declare(Token name)
		{
			if (_scopes.Count == 0) return;

			var scope = _scopes.Peek();
			scope[name.Lexeme] = false;
		}

		private void Define(Token name)
		{
			if (_scopes.Count == 0) return;
			_scopes.Peek()[name.Lexeme] = true;
		}

		private void ResolveLocal(Expr expr, Token name)
		{
			var scopes = _scopes.ToArray();
			for (var i = scopes.Length - 1; i >= 0; i--)
			{
				if (scopes[i].ContainsKey(name.Lexeme))
				{
					_interpreter.Resolve(expr, scopes.Length - 1 - i);
					return;
				}
			}

			// Not found. Assume it is global.
		}

		private void ResolveFunction(Stmt.Function function)
		{
			BeginScope();
			foreach (var param in function.Parameters)
			{
				Declare(param);
				Define(param);
			}
			Resolve(function.Body);
			EndScope();
		}

		private void Resolve(Stmt stmt)
		{
			stmt.Accept(this);
		}

		private void Resolve(Expr expr)
		{
			expr.Accept(this);
		}
	}
}
