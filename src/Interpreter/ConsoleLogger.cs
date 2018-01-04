using System;

namespace CSharpLox
{
	public class ConsoleLogger : ILogger
	{
		public bool HadError { get; private set; }

		public void Error(int line, string where, string message)
		{
			Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
			HadError = true;
		}

		public void ResetError()
		{
			HadError = false;
		}
	}
}
