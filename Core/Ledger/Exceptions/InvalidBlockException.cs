using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ledger.Exceptions {
	public class InvalidBlockException : Exception {
		public InvalidBlockException() { }
		public InvalidBlockException(string message) : base(message) { }
	}
}
