using System;
using System.IO;
using System.Threading.Tasks;

namespace CSharpLox
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			if (args.Length > 1)
			{
				Console.WriteLine("Usage: clox [script]");
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
		}

		private static Task RunPromptAsync()
		{
			while (true)
			{
				Console.WriteLine("> ");
				RunAsync(Console.ReadLine());
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
	}
}
