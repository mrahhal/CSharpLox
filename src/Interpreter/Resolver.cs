using System.Collections.Generic;

namespace CSharpLox
{
	public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
	{
		private enum FunctionType
		{
			NONE,
			FUNCTION,
			METHOD,
			INITIALIZER
		}

		private enum ClassType
		{
			NONE,
			CLASS,
			SUBCLASS
		}

		private readonly Interpreter _interpreter;
		private readonly ILogger _logger;
		private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();
		private FunctionType _currentFunction = FunctionType.NONE;
		private ClassType _currentClass = ClassType.NONE;

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
			Declare(stmt.Name);
			Define(stmt.Name);

			var enclosingClass = _currentClass;
			_currentClass = ClassType.CLASS;

			if (stmt.Superclass != null)
			{
				_currentClass = ClassType.SUBCLASS;
				Resolve(stmt.Superclass);
				BeginScope();
				_scopes.Peek()["super"] = true;
			}

			BeginScope();
			_scopes.Peek()["this"] = true;

			foreach (var method in stmt.Methods)
			{
				var declaration = FunctionType.METHOD;
				if (method.Name.Lexeme == "init")
				{
					declaration = FunctionType.INITIALIZER;
				}
				ResolveFunction(method, declaration);
			}

			EndScope();

			if (stmt.Superclass != null) EndScope();

			_currentClass = enclosingClass;
			return null;
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

			ResolveFunction(stmt, FunctionType.FUNCTION);
			return null;
		}

		public object VisitGetExpr(Expr.Get expr)
		{
			Resolve(expr.Object);
			return null;
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
			if (_currentFunction == FunctionType.NONE)
			{
				_logger.Error(stmt.Keyword, "Cannot return from top-level code.");
			}

			if (stmt.Value != null)
			{
				if (_currentFunction == FunctionType.INITIALIZER)
				{
					_logger.Error(stmt.Keyword, "Cannot return a value from an initializer.");
				}
				Resolve(stmt.Value);
			}

			return null;
		}

		public object VisitSetExpr(Expr.Set expr)
		{
			Resolve(expr.Value);
			Resolve(expr.Object);
			return null;
		}

		public object VisitSuperExpr(Expr.Super expr)
		{
			if (_currentClass == ClassType.NONE)
			{
				_logger.Error(expr.Keyword, "Cannot use 'super' outside of a class.");
			}
			else if (_currentClass != ClassType.SUBCLASS)
			{
				_logger.Error(expr.Keyword, "Cannot use 'super' in a class with no superclass.");
			}

			ResolveLocal(expr, expr.Keyword);
			return null;
		}

		public object VisitThisExpr(Expr.This expr)
		{
			if (_currentClass == ClassType.NONE)
			{
				_logger.Error(expr.Keyword, "Cannot use 'this' outside of a class.");
				return null;
			}

			ResolveLocal(expr, expr.Keyword);
			return null;
		}

		public object VisitUnaryExpr(Expr.Unary expr)
		{
			Resolve(expr.Right);
			return null;
		}

		public object VisitVariableExpr(Expr.Variable expr)
		{
			if (IsDeclaredExact(expr.Name.Lexeme, false) == true)
			{
				_logger.Error(expr.Name,
					"Cannot read local variable in its own initializer.");
			}

			ResolveLocal(expr, expr.Name);
			return null;
		}

		private bool? IsDeclaredExact(string name, bool value)
		{
			if (_scopes.TryPeek(out var scope))
			{
				if (scope.TryGetValue(name, out var v))
				{
					return v == value;
				}
				else
				{
					return null;
				}
			}
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
			if (scope.ContainsKey(name.Lexeme))
			{
				_logger.Error(name,
					"Variable with this name already declared in this scope.");
			}

			scope[name.Lexeme] = false;
		}

		private void Define(Token name)
		{
			if (_scopes.Count == 0) return;
			_scopes.Peek()[name.Lexeme] = true;
		}

		private void ResolveLocal(Expr expr, Token name)
		{
			// Stack.ToArray returns a reversed array. The first pushed item will be the last in the array.
			var scopes = _scopes.ToArray();

			for (var i = 0; i < scopes.Length; i++)
			{
				if (scopes[i].ContainsKey(name.Lexeme))
				{
					_interpreter.Resolve(expr, i);
					return;
				}
			}

			// Not found. Assume it is global.
		}

		private void ResolveFunction(Stmt.Function function, FunctionType type)
		{
			var enclosingFunction = _currentFunction;
			_currentFunction = type;

			BeginScope();
			foreach (var param in function.Parameters)
			{
				Declare(param);
				Define(param);
			}
			Resolve(function.Body);
			EndScope();

			_currentFunction = enclosingFunction;
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
