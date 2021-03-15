using System.Collections;
using System.Threading;

namespace Proxy {
	public class Server {
		private int currentConnections = 0;
		public int Port { get; set; }

		public int CurrentConnections => currentConnections;

		public string IP { get; set; }

		public void IncrementConnections() {
			Interlocked.Increment(ref currentConnections);
		}

		public void DecrementConnections() {
			Interlocked.Decrement(ref currentConnections);
		}
	}
}