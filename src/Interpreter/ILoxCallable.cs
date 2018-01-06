using System;
using System.Collections.Generic;

namespace CSharpLox
{
	public interface ILoxCallable
	{
		int Arity { get; }

		object Call(Interpreter interpreter, List<Object> arguments);
	}

	public class NativeLoxCallable : ILoxCallable
	{
		private static readonly Func<Interpreter, List<object>, object> DefaultFunc = (_, __) => null;

		private Func<Interpreter, List<object>, object> _func = DefaultFunc;

		public virtual int Arity { get; private set; }

		public virtual object Call(Interpreter interpreter, List<object> arguments)
		{
			return _func(interpreter, arguments);
		}

		public static NativeLoxCallable Create(Func<Interpreter, List<object>, object> func, int arity)
		{
			return new NativeLoxCallable
			{
				_func = func,
				Arity = arity
			};
		}
	}

	public class LoxFunction : ILoxCallable
	{
		private readonly Stmt.Function _declaration;

		public int Arity => _declaration.Parameters.Count;

		private LoxFunction(Stmt.Function declaration)
		{
			_declaration = declaration;
		}

		public object Call(Interpreter interpreter, List<object> arguments)
		{
			var environment = new EnvironmentScope(interpreter.GlobalScope);
			for (var i = 0; i < _declaration.Parameters.Count; i++)
			{
				environment.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
			}

			interpreter.ExecuteBlock(_declaration.Body, environment);
			return null;
		}

		public override string ToString()
		{
			return "<fn " + _declaration.Name.Lexeme + ">";
		}
	}
}
