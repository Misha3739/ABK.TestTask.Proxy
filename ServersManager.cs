using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proxy {
	public class ServersManager : IServersManager {
		internal IDictionary<int, Server> Servers { get; }
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
		public Server FindServerAndIncrement() {
			//Поиск наименее загруженного сервера и увеличение его счетчика текущих соединений должны быть атомарной операцией
			lock (locker) {
				//Поиск осуществляется с помощью сортировки по наименьшему числу одновременных соединений
				var server = Servers.OrderBy(s => s.Value.CurrentConnections).First().Value;
				//Возможность что от какого-то сервера отвалится соедниение в момент выполнения функции поиска не исключена (при конкурентном досупе), 
				//но считаю это меньшим злом чем жесткую блокироку FindServerAndIncrement() еще и в момент уменьшения счетчика
				//Потокобезопасная функция - инкремент
				server.IncrementConnections();
				return server;
			}
		}

		public void Decrement(Server server) {
			//Потокобезопасная функция - декремент
			server.DecrementConnections();
		}
	}
}