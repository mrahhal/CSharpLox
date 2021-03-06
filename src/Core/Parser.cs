﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using static CSharpLox.TokenType;

namespace CSharpLox
{
	public class Parser
	{
		private class ParseError : Exception
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
				if (Match(CLASS)) return ClassDeclaration();
				if (Match(FUN)) return Function("function");
				if (Match(VAR)) return VarDeclaration();

				return Statement();
			}
			catch (ParseError)
			{
				Synchronize();
				return null;
			}
		}

		private Stmt ClassDeclaration()
		{
			var name = Consume(IDENTIFIER, "Expected class name.");

			Expr superclass = null;
			if (Match(LESS))
			{
				Consume(IDENTIFIER, "Expected superclass.");
				superclass = new Expr.Variable(Previous());
			}

			Consume(LEFT_BRACE, "Expected '{' before class body.");

			var methods = new List<Stmt.Function>();
			while (!Check(RIGHT_BRACE) && !IsAtEnd())
			{
				methods.Add(Function("method"));
			}

			Consume(RIGHT_BRACE, "Expected '}' after class body.");

			return new Stmt.Class(name, superclass, methods);
		}

		private Stmt.Function Function(string kind)
		{
			var name = Consume(IDENTIFIER, $"Expected {kind} name.");
			Consume(LEFT_PAREN, $"Expected '(' after {kind} name.");
			var parameters = new List<Token>();
			if (!Check(RIGHT_PAREN))
			{
				do
				{
					if (parameters.Count >= 8)
					{
						Error(Peek(), "Cannot have more than 8 parameters.");
					}

					parameters.Add(Consume(IDENTIFIER, "Expected parameter name."));
				} while (Match(COMMA));
			}
			Consume(RIGHT_PAREN, "Expected ')' after parameters.");
			Consume(LEFT_BRACE, "Expected '{' before " + kind + " body.");

			var body = Block();

			return new Stmt.Function(name, parameters, body);
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
			if (Match(FOR)) return ForStatement();
			if (Match(IF)) return IfStatement();
			if (Match(PRINT)) return PrintStatement();
			if (Match(RETURN)) return ReturnStatement();
			if (Match(WHILE)) return WhileStatement();
			if (Match(LEFT_BRACE)) return new Stmt.Block(Block());

			return ExpressionStatement();
		}

		private Stmt ForStatement()
		{
			Consume(LEFT_PAREN, "Expected '(' after 'for'.");

			Stmt initializer;
			if (Match(SEMICOLON))
			{
				initializer = null;
			}
			else if (Match(VAR))
			{
				initializer = VarDeclaration();
			}
			else
			{
				initializer = ExpressionStatement();
			}

			Expr condition = null;
			if (!Check(SEMICOLON))
			{
				condition = Expression();
			}
			Consume(SEMICOLON, "Expected ';' after loop condition.");

			Expr increment = null;
			if (!Check(RIGHT_PAREN))
			{
				increment = Expression();
			}
			Consume(RIGHT_PAREN, "Expected ')' after for clauses.");

			var body = Statement();

			if (increment != null)
			{
				body = new Stmt.Block(new List<Stmt>
				{
					body,
					new Stmt.Expression(increment)
				});
			}

			if (condition == null)
			{
				condition = new Expr.Literal(true);
			}
			body = new Stmt.While(condition, body);

			if (initializer != null)
			{
				body = new Stmt.Block(new List<Stmt> { initializer, body });
			}

			return body;
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

		private Stmt IfStatement()
		{
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

		private Stmt ReturnStatement()
		{
			var keyword = Previous();
			Expr value = null;
			if (!Check(SEMICOLON))
			{
				value = Expression();
			}

			Consume(SEMICOLON, "Expected ';' after return value.");
			return new Stmt.Return(keyword, value);
		}

		private Stmt WhileStatement()
		{
			Consume(LEFT_PAREN, "Expected '(' after 'while'.");
			var condition = Expression();
			Consume(RIGHT_PAREN, "Expected ')' after condition.");
			var body = Statement();

			return new Stmt.While(condition, body);
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
				else if (expr is Expr.Get getExpr)
				{
					return new Expr.Set(getExpr.Object, getExpr.Name, value);
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

			return Call();
		}

		private Expr Call()
		{
			var expr = Primary();

			while (true)
			{
				if (Match(LEFT_PAREN))
				{
					expr = FinishCall(expr);
				}
				else if (Match(DOT))
				{
					var name = Consume(IDENTIFIER, "Expected property name after '.'.");
					expr = new Expr.Get(expr, name);
				}
				else
				{
					break;
				}
			}

			return expr;
		}

		private Expr FinishCall(Expr callee)
		{
			var arguments = new List<Expr>();
			if (!Check(RIGHT_PAREN))
			{
				do
				{
					if (arguments.Count >= 8)
					{
						Error(Peek(), "Cannot have more than 8 arguments.");
					}
					arguments.Add(Expression());
				} while (Match(COMMA));
			}

			var paren = Consume(RIGHT_PAREN, "Expected ')' after arguments.");

			return new Expr.Call(callee, paren, arguments);
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

			if (Match(SUPER))
			{
				var keyword = Previous();
				Consume(DOT, "Expected '.' after 'super'.");
				var method = Consume(IDENTIFIER, "Expected superclass method name.");
				return new Expr.Super(keyword, method);
			}

			if (Match(THIS)) return new Expr.This(Previous());

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

		[DebuggerStepThrough]
		private Token Consume(TokenType type, String message)
		{
			if (Check(type)) return Advance();

			throw Error(Peek(), message);
		}

		[DebuggerStepThrough]
		private ParseError Error(Token token, String message)
		{
			_logger.Error(token, message);
			return new ParseError();
		}

		[DebuggerStepThrough]
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

		[DebuggerStepThrough]
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

		[DebuggerStepThrough]
		private bool Check(TokenType tokenType)
		{
			if (IsAtEnd()) return false;
			return Peek().Type == tokenType;
		}

		[DebuggerStepThrough]
		private Token Advance()
		{
			if (!IsAtEnd()) _current++;
			return Previous();
		}

		[DebuggerStepThrough]
		private bool IsAtEnd()
		{
			return Peek().Type == EOF;
		}

		[DebuggerStepThrough]
		private Token Peek()
		{
			return Tokens[_current];
		}

		[DebuggerStepThrough]
		private Token Previous()
		{
			return Tokens[_current - 1];
		}
	}
}
