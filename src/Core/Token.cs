namespace CSharpLox
{
	public class Token
	{
		public Token(TokenType type, string lexeme, object literal, int line)
		{
			Type = type;
			Lexeme = lexeme;
			Literal = literal;
			Line = line;
		}

		public TokenType Type { get; }

		public string Lexeme { get; }

		public object Literal { get; }

		public int Line { get; }

		public override string ToString()
			=> $"{Type} {Lexeme} {Literal}";
	}
}
