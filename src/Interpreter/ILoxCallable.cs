using System;
using System.Collections.Generic;

namespace CSharpLox
{
	public interface ILoxCallable
	{
		int Arity { get; }

		object Call(Interpreter interpreter, List<Object> arguments);
	}
}
