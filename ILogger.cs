using System;

namespace Proxy {
	public interface ILogger {
		void Trace(string message);
		void Info(string message);
		void Error(Exception e, string message);
	}
}