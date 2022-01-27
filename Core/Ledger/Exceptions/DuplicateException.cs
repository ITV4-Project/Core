using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ledger.Exceptions {
	public class DuplicateException : Exception {
		public DuplicateException() { }
		public DuplicateException(string message) : base(message) { }
	}
}
