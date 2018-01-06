using System;
using System.Collections.Generic;
using static CSharpLox.TokenType;

namespace CSharpLox
{
	public class Parser
	{
		public class ParseError : Exception
		{
		}

		private int _current = 0;

		private readonly ILogger _logger;

		public Parser(
			ILogger logger,
			List<Token> tokens)
		{
			_logger = logger;
			Tokens = tokens;
		}

		public List<Token> Tokens { get; }

		public List<Stmt> Parse()
		{
			var statements = new List<Stmt>();
			while (!IsAtEnd())
			{
				statements.Add(Declaration());
			}

			return statements;
		}

		private Stmt Declaration()
		{
			try
			{
				if (Match(VAR)) return VarDeclaration();

				return Statement();
			}
			catch (ParseError)
			{
				Synchronize();
				return null;
			}
		}

		private Stmt VarDeclaration()
		{
			var name = Consume(IDENTIFIER, "Expected variable name.");

			Expr initializer = null;
			if (Match(EQUAL))
			{
				initializer = Expression();
			}

			Consume(SEMICOLON, "Expected ';' after variable declaration.");
			return new Stmt.Var(name, initializer);
		}

		private Stmt Statement()
		{
			if (Match(IF)) return IfStatement();
			if (Match(PRINT)) return PrintStatement();
			if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

			return ExpressionStatement();
		}

		private List<Stmt> Block()
		{
			var statements = new List<Stmt>();

			while (!Check(RIGHT_BRACE) && !IsAtEnd())
			{
				statements.Add(Declaration());
			}

			Consume(RIGHT_BRACE, "Expected '}' after block.");
			return statements;
		}

		private Stmt IfStatement() {
			Consume(LEFT_PAREN, "Expected '(' after 'if'.");
			var condition = Expression();
			Consume(RIGHT_PAREN, "Expected ')' after if condition.");

			var thenBranch = Statement();
			var elseBranch = default(Stmt);

			if (Match(ELSE))
			{
				elseBranch = Statement();
			}

			return new Stmt.If(condition, thenBranch, elseBranch);
		}

		private Stmt PrintStatement()
		{
			var value = Expression();
			Consume(SEMICOLON, "Expected ';' after value.");
			return new Stmt.Print(value);
		}

		private Stmt ExpressionStatement()
		{
			var expr = Expression();
			Consume(SEMICOLON, "Expected ';' after expression.");
			return new Stmt.Expression(expr);
		}

		private Expr Expression()
		{
			return Assignment();
		}

		private Expr Assignment()
		{
			var expr = Or();

			if (Match(EQUAL))
			{
				var equals = Previous();
				var value = Assignment();

				if (expr is Expr.Variable variableExpr)
				{
					var name = variableExpr.Name;
					return new Expr.Assign(name, value);
				}

				Error(equals, "Invalid assignment target.");
			}

			return expr;
		}

		private Expr Or()
		{
			var expr = And();

			while (Match(OR))
			{
				var op = Previous();
				var right = And();
				expr = new Expr.Logical(expr, op, right);
			}

			return expr;
		}

		private Expr And()
		{
			var expr = Equality();

			while (Match(AND))
			{
				var op = Previous();
				var right = Equality();
				expr = new Expr.Logical(expr, op, right);
			}

			return expr;
		}

		private Expr Equality()
		{
			return ParseLeftAssociativeBinaryOperation(
				Comparison,
				BANG_EQUAL, EQUAL_EQUAL);
		}

		private Expr Comparison()
		{
			return ParseLeftAssociativeBinaryOperation(
				Addition,
				GREATER, GREATER_EQUAL, LESS, LESS_EQUAL);
		}

		private Expr Addition()
		{
			return ParseLeftAssociativeBinaryOperation(
				Multiplication,
				MINUS, PLUS);
		}

		private Expr Multiplication()
		{
			return ParseLeftAssociativeBinaryOperation(
				Unary,
				SLASH, STAR);
		}

		private Expr ParseLeftAssociativeBinaryOperation(
			Func<Expr> higherPrecedence,
			params TokenType[] tokenTypes)
		{
			var expr = higherPrecedence();

			while (Match(tokenTypes))
			{
				var op = Previous();
				var right = higherPrecedence();
				expr = new Expr.Binary(expr, op, right);
			}

			return expr;
		}

		private Expr Unary()
		{
			if (Match(BANG, MINUS))
			{
				var op = Previous();
				var right = Unary();
				return new Expr.Unary(op, right);
			}

			return Primary();
		}

		private Expr Primary()
		{
			if (Match(FALSE)) return new Expr.Literal(false);
			if (Match(TRUE)) return new Expr.Literal(true);
			if (Match(NIL)) return new Expr.Literal(null);

			if (Match(NUMBER, STRING))
			{
				return new Expr.Literal(Previous().Literal);
			}

			if (Match(IDENTIFIER))
			{
				return new Expr.Variable(Previous());
			}

			if (Match(LEFT_PAREN))
			{
				var expr = Expression();
				Consume(RIGHT_PAREN, "Expected ')' after expression.");
				return new Expr.Grouping(expr);
			}

			throw Error(Peek(), "Expected expression.");
		}

		private Token Consume(TokenType type, String message)
		{
			if (Check(type)) return Advance();

			throw Error(Peek(), message);
		}

		private ParseError Error(Token token, String message)
		{
			_logger.Error(token, message);
			return new ParseError();
		}

		private void Synchronize()
		{
			Advance();

			while (!IsAtEnd())
			{
				if (Previous().Type == SEMICOLON) return;

				switch (Peek().Type)
				{
					case CLASS:
					case FUN:
					case VAR:
					case FOR:
					case IF:
					case WHILE:
					case PRINT:
					case RETURN:
						return;
				}

				Advance();
			}
		}

		private bool Match(params TokenType[] types)
		{
			foreach (var type in types)
			{
				if (Check(type))
				{
					Advance();
					return true;
				}
			}

			return false;
		}

		private bool Check(TokenType tokenType)
		{
			if (IsAtEnd()) return false;
			return Peek().Type == tokenType;
		}

		private Token Advance()
		{
			if (!IsAtEnd()) _current++;
			return Previous();
		}

		private bool IsAtEnd()
		{
			return Peek().Type == EOF;
		}

		private Token Peek()
		{
			return Tokens[_current];
		}

		private Token Previous()
		{
			return Tokens[_current - 1];
		}
	}
}
