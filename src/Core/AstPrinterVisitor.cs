using System.Text;

namespace CSharpLox
{
	public class AstPrinterVisitor : Expr.IVisitor<string>
	{
		public string Print(Expr expr)
		{
			return expr.Accept(this);
		}

		public string VisitAssignExpr(Expr.Assign expr)
		{
			return Parenthesize2("=", expr.Name.Lexeme, expr.Value);
		}

		public string VisitBinaryExpr(Expr.Binary expr)
		{
			return Parenthesize(expr.Operator.Lexeme, expr.Left, expr.Right);
		}

		public string VisitCallExpr(Expr.Call expr)
		{
			return Parenthesize2("call", expr.Callee, expr.Arguments);
		}

		public string VisitGetExpr(Expr.Get expr)
		{
			return Parenthesize2(".", expr.Object, expr.Name.Lexeme);
		}

		public string VisitGroupingExpr(Expr.Grouping expr)
		{
			return Parenthesize("group", expr.Expression);
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

				if (part is Expr expr)
				{
					builder.Append(expr.Accept(this));
				}
				//else if (part is Stmt)
				//{
				//	builder.append(((Stmt)part).accept(this));
				//}
				else if (part is Token token)
				{
					builder.Append(token.Lexeme);
				}
				else
				{
					builder.Append(part);
				}
			}

			builder.Append(")");

			return builder.ToString();
		}
	}
}
