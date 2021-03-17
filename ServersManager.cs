using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proxy {
	public class ServersManager : IServersManager {
		internal ConcurrentDictionary<int, Server> Servers { get; }
		private readonly ReaderWriterLockSlim slim;

		public ServersManager(IList<Server> servers) {
			if (servers == null || !servers.Any()) {
				throw new ArgumentException("Servers cannot be null or empty list!");
			}
			this.Servers = new ConcurrentDictionary<int, Server>();
			for (int i = 0; i < servers.Count; i++) {
				this.Servers.TryAdd(i, servers[i]);
			}

			slim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
		}
		//Данная функция не конкурирует сама с собой, только с DecrementCounter
		public Server FindServerAndIncrementConnections() {
			Server server = null;
			slim.EnterUpgradeableReadLock();
			try {
				//Ищем сервер, самый минимальный по числу одновременных соединений (на данный момент)
				server = Servers.OrderBy(s => s.Value.CurrentConnections).First().Value;
				slim.EnterWriteLock();
				try {
					//Обновление числа должно быть в критической секции, чтобы значение всегда было корректным (не так чтобы 1 + 1 - 1 == 2)
					server.CurrentConnections++;
				} finally {
					slim.ExitWriteLock();
				}
			} finally {
				slim.ExitUpgradeableReadLock();
			}

			return server;
		}
		//Данная функция конкурирует как сама с собой, так и с FindServerAndIncrementCounter
		public void DecrementConnections(Server server) {
			slim.EnterWriteLock();
			try {
				//Обновление числа должно быть в критической секции, чтобы значение всегда было корректным (не так чтобы 1 + 1 - 1 == 2)
				server.CurrentConnections--;
			} finally {
				slim.ExitWriteLock();
			}
		}
	}
}