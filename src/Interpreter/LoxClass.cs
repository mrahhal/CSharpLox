using System.Collections.Generic;

namespace CSharpLox
{
	public class LoxClass : ILoxCallable
	{
		private readonly Dictionary<string, LoxFunction> _methods;

		public LoxClass(string name, Dictionary<string, LoxFunction> methods)
		{
			Name = name;
			_methods = methods;
		}

		public string Name { get; }

		public int Arity
		{
			get
			{
				if (_methods.TryGetValue("init", out var init))
				{
					return init.Arity;
				}
				return 0;
			}
		}

		public object Call(Interpreter interpreter, List<object> arguments)
		{
			var instance = new LoxInstance(this);
			if (_methods.TryGetValue("init", out var init))
			{
				init.Bind(instance).Call(interpreter, arguments);
			}
			return instance;
		}

		public LoxFunction FindMethod(LoxInstance instance, string name)
		{
			if (_methods.TryGetValue(name, out var value))
			{
				return value.Bind(instance);
			}

			return null;
		}

		public override string ToString() => Name;
	}
}
