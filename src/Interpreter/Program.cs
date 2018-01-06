using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
	public class Lox
	{
		private readonly ConsoleLogger _logger = new ConsoleLogger();
		private readonly Interpreter _interpreter;
		private string[] _args;

		public static Task Main(string[] args)
		{
			return new Lox(args).RunInternalAsync();
		}

		public Lox(string[] args)
		{
			_args = args;

			_interpreter = new Interpreter(_logger);
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
			if (_logger.HadRuntimeError)
			{
				Environment.Exit(70);
			}
		}

		private Task RunPromptAsync()
		{
			while (true)
			{
				Console.Write("> ");
				RunAsync(ReadConsoleInput());
				_logger.ResetError();
			}
		}

		private string ReadConsoleInput()
		{
			var builder = new StringBuilder();

			// TODO
			builder.Append(Console.ReadLine());

			return builder.ToString();
		}

		private Task RunAsync(string source)
		{
			var scanner = new Scanner(_logger, source);
			var tokens = scanner.ScanTokens();

			var parser = new Parser(_logger, tokens);
			var statements = parser.Parse();

			// Stop if there was a syntax error.
			if (_logger.HadError) return Task.CompletedTask;

			_interpreter.Interpret(statements);

			return Task.CompletedTask;
		}
	}
}
