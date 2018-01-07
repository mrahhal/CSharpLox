using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpLox
{
	public class AstPrinter : Expr.IVisitor<string>, Stmt.IVisitor<string>
	{
		public string Print(Expr expr)
		{
			return expr.Accept(this);
		}

		public string Print(Stmt stmt)
		{
			return stmt.Accept(this);
		}

		public string VisitAssignExpr(Expr.Assign expr)
		{
			return Parenthesize2("=", expr.Name.Lexeme, expr.Value);
		}

		public string VisitBinaryExpr(Expr.Binary expr)
		{
			return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
		}

		public string VisitBlockStmt(Stmt.Block stmt)
		{
			var builder = new StringBuilder();
			builder.Append("(block ");

			foreach (var statement in stmt.Statements)
			{
				builder.Append(statement.Accept(this));
			}

			builder.Append(")");
			return builder.ToString();
		}

		public string VisitCallExpr(Expr.Call expr)
		{
			return Parenthesize2("call", expr.Callee, expr.Arguments);
		}

		public string VisitClassStmt(Stmt.Class stmt)
		{
			var builder = new StringBuilder();
			builder.Append("(class " + stmt.Name.Lexeme);
			//> Inheritance omit

			if (stmt.Superclass != null)
			{
				builder.Append(" < " + Print(stmt.Superclass));
			}
			//< Inheritance omit

			foreach (var method in stmt.Methods)
			{
				builder.Append(" " + Print(method));
			}

			builder.Append(")");
			return builder.ToString();
		}

		public string VisitExpressionStmt(Stmt.Expression stmt)
		{
			return Parenthesize(";", stmt.InnerExpression);
		}

		public string VisitFunctionStmt(Stmt.Function stmt)
		{
			var builder = new StringBuilder();
			builder.Append("(fun " + stmt.Name.Lexeme + "(");

			foreach (var param in stmt.Parameters)
			{
				if (param != stmt.Parameters[0]) builder.Append(" ");
				builder.Append(param.Lexeme);
			}

			builder.Append(") ");

			foreach (var body in stmt.Body)
			{
				builder.Append(body.Accept(this));
			}

			builder.Append(")");
			return builder.ToString();
		}

		public string VisitGetExpr(Expr.Get expr)
		{
			return Parenthesize2(".", expr.Object, expr.Name.Lexeme);
		}

		public string VisitGroupingExpr(Expr.Grouping expr)
		{
			return Parenthesize("group", expr.Expression);
		}

		public string VisitIfStmt(Stmt.If stmt)
		{
			if (stmt.ElseBranch == null)
			{
				return Parenthesize2("if", stmt.Condition, stmt.ThenBranch);
			}

			return Parenthesize2("if-else", stmt.Condition, stmt.ThenBranch,
				stmt.ElseBranch);
		}

		public string VisitLiteralExpr(Expr.Literal expr)
		{
			if (expr.Value == null) return "nil";
			return expr.Value.ToString();
		}

		public string VisitLogicalExpr(Expr.Logical expr)
		{
			return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
		}

		public string VisitPrintStmt(Stmt.Print stmt)
		{
			return Parenthesize("print", stmt.InnerExpression);
		}

		public string VisitReturnStmt(Stmt.Return stmt)
		{
			if (stmt.Value == null) return "(return)";
			return Parenthesize("return", stmt.Value);
		}

		public string VisitSetExpr(Expr.Set expr)
		{
			return Parenthesize2("=", expr.Object, expr.Name.Lexeme, expr.Value);
		}

		public string VisitSuperExpr(Expr.Super expr)
		{
			return Parenthesize2("super", expr.Method);
		}

		public string VisitThisExpr(Expr.This expr)
		{
			return "this";
		}

		public string VisitUnaryExpr(Expr.Unary expr)
		{
			return Parenthesize(expr.Operator.Lexeme, expr.Right);
		}

		public string VisitVariableExpr(Expr.Variable expr)
		{
			return expr.Name.Lexeme;
		}

		public string VisitVarStmt(Stmt.Var stmt)
		{
			if (stmt.Initializer == null)
			{
				return Parenthesize2("var", stmt.Name);
			}

			return Parenthesize2("var", stmt.Name, "=", stmt.Initializer);
		}

		public string VisitWhileStmt(Stmt.While stmt)
		{
			return Parenthesize2("while", stmt.Condition, stmt.Body);
		}

		private string Parenthesize(string name, params Expr[] exprs)
		{
			var builder = new StringBuilder();

			builder.Append("(").Append(name);

			foreach (var expr in exprs)
			{
				builder.Append(" ");
				builder.Append(expr.Accept(this));
			}

			builder.Append(")");

			return builder.ToString();
		}

		private string Parenthesize2(string name, params object[] parts)
		{
			var builder = new StringBuilder();

			builder.Append("(").Append(name);

			foreach (var part in parts)
			{
				builder.Append(" ");

				switch (part)
				{
					case Expr expr:
						builder.Append(expr.Accept(this));
						break;

					case Stmt stmt:
						builder.Append(stmt.Accept(this));
						break;

					case Token token:
						builder.Append(token.Lexeme);
						break;

					case IEnumerable<Expr> expressions:
						if (expressions.Any())
						{
							builder.Append("[");
							foreach (var expr in expressions)
							{
								if (expr != expressions.First())
								{
									builder.Append(", ");
								}
								builder.Append(expr.Accept(this));
							}
							builder.Append("]");
						}
						break;

					default:
						builder.Append(part.ToString());
						break;
				}
			}

			builder.Append(")");

			return builder.ToString();
		}
	}
}
