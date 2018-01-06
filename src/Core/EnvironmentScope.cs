using System.Collections.Generic;

namespace CSharpLox
{
	public class EnvironmentScope
	{
		private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

		public EnvironmentScope(EnvironmentScope enclosing = null)
		{
			Enclosing = enclosing;
		}

		public EnvironmentScope Enclosing { get; }

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

			if (Enclosing != null)
			{
				return Enclosing.Get(name);
			}

			throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
		}

		public object GetAt(int distance, string name)
		{
			return Ancestor(distance)._values[name];
		}

		public void Assign(Token name, object value)
		{
			if (_values.ContainsKey(name.Lexeme))
			{
				_values[name.Lexeme] = value;
				return;
			}

			if (Enclosing != null)
			{
				Enclosing.Assign(name, value);
				return;
			}

			throw new RuntimeError(name, "Undefined variable '" + name.Lexeme + "'.");
		}

		public void AssignAt(int distance, Token name, object value)
		{
			Ancestor(distance)._values[name.Lexeme] = value;
		}

		private EnvironmentScope Ancestor(int distance)
		{
			var environment = this;
			for (var i = 0; i < distance; i++)
			{
				environment = environment.Enclosing;
			}

			return environment;
		}
	}
}
