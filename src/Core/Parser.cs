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

		public Expr Parse()
		{
			try
			{
				return Expression();
			}
			catch (ParseError)
			{
				return null;
			}
		}

		private Expr Expression()
		{
			return Equality();
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
