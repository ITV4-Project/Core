using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Database.Exceptions {
	public class InvalidSequenceException : Exception {
		public InvalidSequenceException() { }

		public InvalidSequenceException(string message) : base(message) { }
	}
}
