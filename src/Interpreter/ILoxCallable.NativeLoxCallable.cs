using System;
using System.Collections.Generic;

namespace CSharpLox
{
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
}
