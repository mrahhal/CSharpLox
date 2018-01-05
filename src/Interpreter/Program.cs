using System;
using System.IO;
using System.Threading.Tasks;

namespace CSharpLox
{
	public class Lox
	{
		private readonly ConsoleLogger _logger = new ConsoleLogger();
		private string[] _args;

		public static Task Main(string[] args)
		{
			return new Lox(args).RunInternalAsync();
		}

		public Lox(string[] args)
		{
			_args = args;
		}

		private async Task RunInternalAsync()
		{
			if (_args.Length > 1)
			{
				Console.WriteLine("Usage: cslox [script]");
			}
			else if (_args.Length == 1)
			{
				await RunFileAsync(_args[0]);
			}
			else
			{
				await RunPromptAsync();
			}
		}

		private async Task RunFileAsync(string filePath)
		{
			var source = await File.ReadAllTextAsync(filePath);
			await RunAsync(source);
			if (_logger.HadError)
			{
				Environment.Exit(65);
			}
		}

		private Task RunPromptAsync()
		{
			while (true)
			{
				Console.WriteLine("> ");
				RunAsync(Console.ReadLine());
				_logger.ResetError();
			}
		}

		private Task RunAsync(string source)
		{
			var scanner = new Scanner(_logger, source);
			var tokens = scanner.ScanTokens();

			foreach (var token in tokens)
			{
				Console.WriteLine(token);
			}

			Console.WriteLine("===");

			var parser = new Parser(_logger, tokens);
			var expression = parser.Parse();

			// Stop if there was a syntax error.
			if (_logger.HadError) return Task.CompletedTask;

			Console.WriteLine(new AstPrinterVisitor().Print(expression));

			return Task.CompletedTask;
		}
	}
}
