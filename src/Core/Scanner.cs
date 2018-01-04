using System.Collections.Generic;
using static CSharpLox.TokenType;

namespace CSharpLox
{
	public class Scanner
	{
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

				default:
					_logger.Error(_line, "Unexpected character.");
					break;
			}
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
			var text = Source.Substring(_start, _current);
			_tokens.Add(new Token(type, text, literal, _line));
		}
	}
}
