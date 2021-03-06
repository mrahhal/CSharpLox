﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLox
{
	public class Program
	{
		private readonly ConsoleLogger _logger = new ConsoleLogger();
		private readonly Interpreter _interpreter;
		private string[] _args;

		public static Task Main(string[] args)
		{
			return new Program(args).RunInternalAsync();
		}

		public Program(string[] args)
		{
			_args = args;

			_interpreter = new Interpreter(_logger);
		}

		private async Task RunInternalAsync()
		{
			if (ShouldPrintHelp())
			{
				PrintHelp();
			}
			else if (_args.Length == 0)
			{
				await RunPromptAsync();
			}
			else
			{
				await RunFileAsync(_args[0]);
			}
		}

		private static void PrintHelp()
		{
			Console.WriteLine("Usage: cslox [script]");
			Console.WriteLine();
			Console.WriteLine("Options:");
			Console.WriteLine("--help       Show help");
			Console.WriteLine("--print-ast  Print the ast and exit");
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

			if (ShouldPrintAst())
			{
				var astPrinter = new AstPrinter();
				foreach (var statement in statements)
				{
					Console.WriteLine(astPrinter.Print(statement));
				}
				return Task.CompletedTask;
			}

			var resolver = new Resolver(_interpreter, _logger);
			resolver.Resolve(statements);

			if (_logger.HadError) return Task.CompletedTask;

			_interpreter.Interpret(statements);

			return Task.CompletedTask;
		}

		private bool ShouldPrintAst()
		{
			return _args.Contains("--print-ast");
		}

		private bool ShouldPrintHelp()
		{
			return _args.Contains("--help");
		}
	}
}
