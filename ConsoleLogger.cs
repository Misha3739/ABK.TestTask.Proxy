using System;
using System.Collections.Generic;
using System.Text;

namespace Proxy {
	public class ConsoleLogger : ILogger {
		public void Trace(string message) {
			var time = TimeToString();
			Console.WriteLine($"{time} TRACE {message}");
		}

		public void Info(string message) {
			var time = TimeToString();
			Console.WriteLine($"{time} INFO {message}");
		}

		public void Error(Exception e, string message) {
			var time = TimeToString();
			Console.WriteLine($"{time} ERROR {message} {e.StackTrace}");
		}

		private string TimeToString() {
			return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
		}
	}
}
