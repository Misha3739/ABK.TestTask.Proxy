using System;
using System.Collections.Generic;

namespace Proxy {
	class Program {
		static void Main(string[] args) {
			var port = 10501;
			var ip = "127.0.0.1";
			var bytesPerChunk = 2*1024;
			var servers = new List<Server>() {
			};
			for (int i = 0; i < 5; i++) {
				servers.Add(new Server() {
					Port = 4830 + i
				});
			}
			var serversManager = new ServersManager(servers);
			ILogger logger = new ConsoleLogger();
			IProxyServer proxy = new ProxyServer(ip, port, bytesPerChunk, logger, serversManager);
			proxy.ListenForClients();
			logger.Info("Proxy successfully started. Press any key to terminate proxy...");
			Console.ReadLine();
			proxy.Dispose();
			logger.Info("Proxy disposed, terminating...");
		}
	}
}
