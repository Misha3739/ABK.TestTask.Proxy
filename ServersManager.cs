using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proxy {
	public class ServersManager : IServersManager {
		public IDictionary<int, Server> Servers { get; }
		private readonly object locker;

		public ServersManager(IList<Server> servers) {
			if (servers == null || !servers.Any()) {
				throw new ArgumentException("Servers cannot be null or empty list!");
			}
			this.Servers = new Dictionary<int, Server>();
			for (int i = 0; i < servers.Count; i++) {
				this.Servers.Add(i, servers[i]);
			}

			locker = new object();
		}
		public int GetAvailableServerKey() {
			lock (locker) {
				var key = Servers.OrderBy(s => s.Value.CurrentConnections).First().Key;
				Servers[key].CurrentConnections += 1;
				return key;
			}
		}

		public void ReleaseServer(int key) {
			lock (locker) {
				Servers[key].CurrentConnections -= 1;
			}
		}

	}
}