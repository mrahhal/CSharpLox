using System.Collections.Generic;

namespace CSharpLox
{
	public class LoxInstance
	{
		private readonly LoxClass _klass;
		private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

		public LoxInstance(LoxClass klass)
		{
			_klass = klass;
		}

		public object Get(Token name)
		{
			if (_fields.TryGetValue(name.Lexeme, out var value))
			{
				return value;
			}

			var method = _klass.FindMethod(this, name.Lexeme);
			if (method != null) return method;

			throw new RuntimeError(name, "Undefined property '" + name.Lexeme + "'.");
		}

		public void Set(Token name, object value)
		{
			_fields[name.Lexeme] = value;
		}

		public override string ToString()
		{
			return _klass.Name + " instance";
		}
	}
}
