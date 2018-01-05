using System.Collections.Generic;

namespace CSharpLox
{
	public class EnvironmentScope
	{
		private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

		public void Define(string name, object value)
		{
			_values[name] = value;
		}

		public object Get(Token name)
		{
			if (_values.TryGetValue(name.Lexeme, out var value))
			{
				return value;
			}

			throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
		}

		public void Assign(Token name, object value)
		{
			if (_values.ContainsKey(name.Lexeme))
			{
				_values[name.Lexeme] = value;
				return;
			}

			throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
		}
	}
}
