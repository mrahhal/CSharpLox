using System;

namespace CSharpLox
{
	public class ConsoleLogger : ILogger
	{
		public bool HadError { get; private set; }

		public bool HadRuntimeError { get; private set; }

		public void Error(int line, string where, string message)
		{
			Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
			HadError = true;
		}

		public void RuntimeError(RuntimeError error)
		{
			Console.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
			HadRuntimeError = true;
		}

		public void ResetError()
		{
			HadError = false;
		}

		public void ResetRuntimeError()
		{
			HadRuntimeError = false;
		}
	}
}
