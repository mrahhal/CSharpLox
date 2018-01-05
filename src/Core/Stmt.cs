using System.Collections.Generic;

namespace CSharpLox
{
	public abstract class Stmt
	{
		public interface IVisitor<R>
		{
			R VisitBlockStmt(Block stmt);

			R VisitClassStmt(Class stmt);

			R VisitExpressionStmt(Expression stmt);

			R VisitFunctionStmt(Function stmt);

			R VisitIfStmt(If stmt);

			R VisitPrintStmt(Print stmt);

			R VisitReturnStmt(Return stmt);

			R VisitVarStmt(Var stmt);

			R VisitWhileStmt(While stmt);
		}

		public abstract R Accept<R>(IVisitor<R> visitor);

		public class Block : Stmt
		{
			public Block(List<Stmt> statements)
			{
				Statements = statements;
			}

			public List<Stmt> Statements { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitBlockStmt(this);
			}
		}

		public class Class : Stmt
		{
			public Class(Token name, Expr superclass, List<Stmt.Function> methods)
			{
				Name = name;
				Superclass = superclass;
				Methods = methods;
			}

			public Token Name { get; }
			public Expr Superclass { get; }
			public List<Stmt.Function> Methods { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitClassStmt(this);
			}
		}

		public class Expression : Stmt
		{
			public Expression(Expr expression)
			{
				InnerExpression = expression;
			}

			public Expr InnerExpression { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitExpressionStmt(this);
			}
		}

		public class Function : Stmt
		{
			public Function(Token name, List<Token> parameters, List<Stmt> body)
			{
				Name = name;
				Parameters = parameters;
				Body = body;
			}

			public Token Name { get; }
			public List<Token> Parameters { get; }
			public List<Stmt> Body { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitFunctionStmt(this);
			}
		}

		public class If : Stmt
		{
			public If(Expr condition, Stmt thenBranch, Stmt elseBranch)
			{
				Condition = condition;
				ThenBranch = thenBranch;
				ElseBranch = elseBranch;
			}

			public Expr Condition { get; }
			public Stmt ThenBranch { get; }
			public Stmt ElseBranch { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitIfStmt(this);
			}
		}

		public class Print : Stmt
		{
			public Print(Expr expression)
			{
				InnerExpression = expression;
			}

			public Expr InnerExpression { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitPrintStmt(this);
			}
		}

		public class Return : Stmt
		{
			public Return(Token keyword, Expr value)
			{
				Keyword = keyword;
				Value = value;
			}

			public Token Keyword { get; }
			public Expr Value { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitReturnStmt(this);
			}
		}

		public class Var : Stmt
		{
			public Var(Token name, Expr initializer)
			{
				Name = name;
				Initializer = initializer;
			}

			public Token Name { get; }
			public Expr Initializer { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitVarStmt(this);
			}
		}

		public class While : Stmt
		{
			public While(Expr condition, Stmt body)
			{
				Condition = condition;
				Body = body;
			}

			public Expr Condition { get; }
			public Stmt Body { get; }

			public override R Accept<R>(IVisitor<R> visitor)
			{
				return visitor.VisitWhileStmt(this);
			}
		}
	}
}
