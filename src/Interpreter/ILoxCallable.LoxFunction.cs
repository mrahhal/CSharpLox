using System.Collections.Generic;

namespace CSharpLox
{
	public class LoxFunction : ILoxCallable
	{
		private readonly Stmt.Function _declaration;
		private readonly EnvironmentScope _closure;

		public int Arity => _declaration.Parameters.Count;

		public LoxFunction(Stmt.Function declaration, EnvironmentScope closure)
		{
			_declaration = declaration;
			_closure = closure;
		}

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

			return null;
		}

		public override string ToString()
		{
			return "<fn " + _declaration.Name.Lexeme + ">";
		}
	}
}
