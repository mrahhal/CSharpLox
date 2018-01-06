using System.Collections.Generic;

namespace CSharpLox
{
	public class LoxFunction : ILoxCallable
	{
		private readonly Stmt.Function _declaration;
		private readonly EnvironmentScope _closure;
		private readonly bool _isInitializer;

		public LoxFunction(Stmt.Function declaration, EnvironmentScope closure, bool isInitializer)
		{
			_declaration = declaration;
			_closure = closure;
			_isInitializer = isInitializer;
		}

		public int Arity => _declaration.Parameters.Count;

		public object Call(Interpreter interpreter, List<object> arguments)
		{
			var environment = new EnvironmentScope(_closure);
			for (var i = 0; i < _declaration.Parameters.Count; i++)
			{
				environment.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
			}

			try
			{
				interpreter.ExecuteBlock(_declaration.Body, environment);
			}
			catch (Return @return)
			{
				return @return.Value;
			}

			if (_isInitializer) return _closure.GetAt(0, "this");
			return null;
		}

		public LoxFunction Bind(LoxInstance instance)
		{
			var environment = new EnvironmentScope(_closure);
			environment.Define("this", instance);
			return new LoxFunction(_declaration, environment, _isInitializer);
		}

		public override string ToString()
		{
			return "<fn " + _declaration.Name.Lexeme + ">";
		}
	}
}
