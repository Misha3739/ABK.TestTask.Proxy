using System.Collections;
using System.Threading;

namespace Proxy {
	public class Server {
		public int Port { get; set; }

		public int CurrentConnections { get; set; }

		public string IP { get; set; }
	}
}