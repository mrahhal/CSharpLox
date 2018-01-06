using System;

namespace CSharpLox
{
	public class Return : Exception
	{
		public Return(object value)
		{
			Value = value;
		}

		public object Value { get; set; }
	}
}
