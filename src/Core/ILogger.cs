namespace CSharpLox
{
	public interface ILogger
	{
		bool HadError { get; }

		void Error(int line, string where, string message);

		void ResetError();
	}

	public static class LoggerExtensions
	{
		public static void Error(this ILogger logger, int line, string message)
		{
			logger.Error(line, string.Empty, message);
		}

		public static void Error(this ILogger logger, Token token, string message)
		{
			if (token.Type == TokenType.EOF)
			{
				logger.Error(token.Line, " at end", message);
			}
			else
			{
				logger.Error(token.Line, " at '" + token.Lexeme + "'", message);
			}
		}
	}
}
