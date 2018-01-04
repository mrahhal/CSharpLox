using System.Collections.Generic;
using static CSharpLox.TokenType;

namespace CSharpLox
{
	public class Scanner
	{
		private static readonly Dictionary<string, TokenType> KeywordsMap = new Dictionary<string, TokenType>
		{
			{ "and", AND },
			{ "class", CLASS },
			{ "else", ELSE },
			{ "false", FALSE },
			{ "for", FOR },
			{ "fun", FUN },
			{ "if", IF },
			{ "nil", NIL },
			{ "or", OR },
			{ "print", PRINT },
			{ "return", RETURN },
			{ "super", SUPER },
			{ "this", THIS },
			{ "true", TRUE },
			{ "var", VAR },
			{ "while", WHILE },
		};

		private readonly List<Token> _tokens = new List<Token>();
		private int _start = 0;
		private int _current = 0;
		private int _line = 1;

		private readonly ILogger _logger;

		public Scanner(
			ILogger logger,
			string source)
		{
			_logger = logger;
			Source = source;
		}

		public string Source { get; }

		public List<Token> ScanTokens()
		{
			while (!IsAtEnd())
			{
				// We are at the beginning of the next lexeme.
				_start = _current;
				ScanToken();
			}

			_tokens.Add(new Token(EOF, "", null, _line));
			return _tokens;
		}

		private void ScanToken()
		{
			var c = Advance();
			switch (c)
			{
				case '(': AddToken(LEFT_PAREN); break;
				case ')': AddToken(RIGHT_PAREN); break;
				case '{': AddToken(LEFT_BRACE); break;
				case '}': AddToken(RIGHT_BRACE); break;
				case ',': AddToken(COMMA); break;
				case '.': AddToken(DOT); break;
				case '-': AddToken(MINUS); break;
				case '+': AddToken(PLUS); break;
				case ';': AddToken(SEMICOLON); break;
				case '*': AddToken(STAR); break;
				case '!': AddToken(Match('=') ? BANG_EQUAL : BANG); break;
				case '=': AddToken(Match('=') ? EQUAL_EQUAL : EQUAL); break;
				case '<': AddToken(Match('=') ? LESS_EQUAL : LESS); break;
				case '>': AddToken(Match('=') ? GREATER_EQUAL : GREATER); break;
				case '/':
					if (Match('/'))
					{
						// A comment goes until the end of the line.
						while (Peek() != '\n' && !IsAtEnd()) Advance();
					}
					else
					{
						AddToken(SLASH);
					}
					break;

				case ' ':
				case '\r':
				case '\t':
					// Ignore whitespace.
					break;

				case '\n':
					_line++;
					break;

				case '"': ScanString(); break;

				default:
					if (IsDigit(c))
					{
						ScanNumber();
					}
					else if (IsAlpha(c))
					{
						ScanIdentifier();
					}
					else
					{
						_logger.Error(_line, "Unexpected character.");
					}
					break;
			}
		}

		private void ScanString()
		{
			while (Peek() != '"' && !IsAtEnd())
			{
				if (Peek() == '\n') _line++;
				Advance();
			}

			// Unterminated string.
			if (IsAtEnd())
			{
				_logger.Error(_line, "Unterminated string.");
				return;
			}

			// The closing ".
			Advance();

			// Trim the surrounding quotes.
			var value = Substring(_start + 1, _current - 1);
			AddToken(STRING, value);
		}

		private void ScanNumber()
		{
			PeekWhileIsDigit();

			// Look for a fractional part.
			if (Peek() == '.' && IsDigit(PeekNext()))
			{
				// Consume the "."
				Advance();

				PeekWhileIsDigit();
			}

			var literal = double.Parse(Substring(_start, _current));
			AddToken(NUMBER, literal);

			void PeekWhileIsDigit()
			{
				while (IsDigit(Peek())) Advance();
			}
		}

		private void ScanIdentifier()
		{
			while (IsAlphaNumeric(Peek())) Advance();

			// See if the identifier is a reserved word.
			var text = Substring(_start, _current);

			var resolvedType = IDENTIFIER;
			if (KeywordsMap.TryGetValue(text, out var type))
			{
				resolvedType = type;
			}
			AddToken(resolvedType);
		}

		private bool Match(char expected)
		{
			if (IsAtEnd()) return false;
			if (Source[_current] != expected) return false;

			_current++;
			return true;
		}

		private char Peek()
		{
			if (IsAtEnd()) return '\0';
			return Source[_current];
		}

		private char PeekNext()
		{
			if (_current + 1 >= Source.Length) return '\0';
			return Source[_current + 1];
		}

		private bool IsAlpha(char c)
		{
			return
				(c >= 'a' && c <= 'z') ||
				(c >= 'A' && c <= 'Z') ||
				c == '_';
		}

		private bool IsAlphaNumeric(char c)
		{
			return IsAlpha(c) || IsDigit(c);
		}

		private bool IsDigit(char c)
		{
			return c >= '0' && c <= '9';
		}

		private bool IsAtEnd()
		{
			return _current >= Source.Length;
		}

		private char Advance()
		{
			_current++;
			return Source[_current - 1];
		}

		private void AddToken(TokenType type)
		{
			AddToken(type, null);
		}

		private void AddToken(TokenType type, object literal)
		{
			var text = Substring(_start, _current);
			_tokens.Add(new Token(type, text, literal, _line));
		}

		private string Substring(int start, int end)
		{
			return Source.Substring(start, end - start);
		}
	}
}
