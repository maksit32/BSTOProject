using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSTOServer.Exceptions
{
	public class EmptyConnectionStringException : Exception
	{
		public EmptyConnectionStringException() { }
		public EmptyConnectionStringException(string message) : base(message) { }
		public EmptyConnectionStringException(string message, Exception inner) : base(message, inner) { }
	}
}
