using System;
using System.IO;
using System.Threading.Tasks;

namespace CSharpLox
{
	public class Program
	{
		private static bool _hadError = false;

		public static async Task Main(string[] args)
		{
			if (args.Length > 1)
			{
				Console.WriteLine("Usage: cslox [script]");
			}
			else if (args.Length == 1)
			{
				await RunFileAsync(args[0]);
			}
			else
			{
				await RunPromptAsync();
			}
		}

		private static async Task RunFileAsync(string filePath)
		{
			var source = await File.ReadAllTextAsync(filePath);
			await RunAsync(source);
			if (_hadError)
			{
				Environment.Exit(65);
			}
		}

		private static Task RunPromptAsync()
		{
			while (true)
			{
				Console.WriteLine("> ");
				RunAsync(Console.ReadLine());
				_hadError = false;
			}
		}

		private static Task RunAsync(string source)
		{
			throw new NotImplementedException();

			//var scanner = new Scanner(source);
			//var tokens = scanner.ScanTokens();

			//foreach (var token in tokens)
			//{
			//	Console.WriteLine(token);
			//}
		}

		public static void Error(int line, string message)
		{
			Report(line, string.Empty, message);
		}

		private static void Report(int line, string where, string message)
		{
			Console.WriteLine("[line " + line + "] Error" + where + ": " + message);
			_hadError = true;
		}
	}
}
