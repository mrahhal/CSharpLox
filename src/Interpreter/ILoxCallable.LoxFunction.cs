using System.Collections.Generic;

namespace CSharpLox
{
	public class LoxFunction : ILoxCallable
	{
		private readonly Stmt.Function _declaration;

		public int Arity => _declaration.Parameters.Count;

		public LoxFunction(Stmt.Function declaration)
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
