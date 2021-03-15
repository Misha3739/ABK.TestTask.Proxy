using System;
using System.Collections.Generic;
using System.Linq;

namespace Proxy {
	class Program {
		static void Main(string[] args) {
			ILogger logger = new ConsoleLogger();
			try {
				var configurationService = new ConfigurationService();
				var listeningPort = configurationService.GetValue<int>(SettingKeys.ListeningPort);
				var serverIp = configurationService.GetValue<string>(SettingKeys.ServerIP);
				var portsRangs = configurationService.GetValue<string>(SettingKeys.ServerPorts)
					.Split(";").Select(int.Parse);
				var servers = new List<Server>();
				foreach (var port in portsRangs) {
					servers.Add(new Server() {
						IP = serverIp,
						Port = port
					});
				}
				logger.Info($"Found {servers.Count} available servers...");
				var serversManager = new ServersManager(servers);
				IProxyServer proxy = new ProxyServer(listeningPort, logger, serversManager);
				proxy.ListenForClients();
				logger.Info("Proxy successfully started. Press any key to terminate proxy...");
				Console.ReadLine();
				proxy.Dispose();
				logger.Info("Proxy disposed, terminating...");
			} catch (Exception e) {
				logger.Error(e, "Cannot initialize proxy...");
			}

		}
	}
}
