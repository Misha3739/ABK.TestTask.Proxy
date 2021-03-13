using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy {
	public class ProxyServer : IProxyServer, IDisposable {
		private readonly int port;
		private readonly string ip;
		private readonly int chunkSize;

		private TcpListener tcpListener;
		private readonly IServersManager serversManager;
		private readonly ILogger logger;

		private readonly CancellationTokenSource cts;
		public ProxyServer(string ip, int port, int chunkSize, ILogger logger, IServersManager serversManager) {
			this.port = port;
			this.ip = ip;
			
			this.logger = logger;
			this.serversManager = serversManager;
			this.chunkSize = chunkSize;

			tcpListener = new TcpListener(IPAddress.Any, this.port);
			cts = new CancellationTokenSource();
		}

		public Task ListenForClients() {
			logger.Trace("IN ProxyServer.ListenForClients");
			try {
				tcpListener.Start();
				Task listeningTask = Task.Run(async () => {
					while (!cts.IsCancellationRequested) {
						var client = tcpListener.AcceptTcpClient();
						var clientPort = ((IPEndPoint) client.Client.RemoteEndPoint).Port;
						logger.Info($"IN ProxyServer.ListenForClients, New client attached, port = \"{clientPort}\"");
						Task clientTask = this.HandleTcpClient(client);
						
						await Task.Delay(100);
					}
				});
				return listeningTask;
			} catch (Exception e) {
				logger.Error(e, "IN ProxyServer.ListenForClients");
				return null;
			} finally {
				logger.Trace("OUT ProxyServer.ListenForClients");
			}
		}

		internal Task HandleTcpClient(TcpClient inboundClient) {
			logger.Trace("IN ProxyServer.HandleTcpClient");
			try {
				var clientPort = ((IPEndPoint)inboundClient.Client.RemoteEndPoint).Port;
				var serverKey = serversManager.GetAvailableServerKey();
				var serverClient = new TcpClient(ip, serversManager.Servers[serverKey].Port);
				Task clientTask = Task.Run(async () => {
					try {
						NetworkStream clientStream = inboundClient.GetStream();
						NetworkStream serverStream = serverClient.GetStream();
						Byte[] bytes = new Byte[chunkSize];
						while (!cts.IsCancellationRequested && (await clientStream.ReadAsync(bytes, 0, bytes.Length) != 0)) {
							logger.Info($"Received package from port = \"{clientPort}\", length = \"{bytes.Length}\"");
							serverStream.Write(bytes, 0, bytes.Length);
						}
					} finally {
						inboundClient.Close();
						serversManager.ReleaseServer(serverKey);
						logger.Info($"IN ProxyServer.HandleTcpClient, client detached, port = \"{clientPort}\"");
					}
				});
				return clientTask;
			} catch (Exception e) {
				logger.Error(e, "IN ProxyServer.HandleTcpClient");
				return null;
			} finally {
				logger.Trace("OUT ProxyServer.HandleTcpClient");
			}
		}

		public void Dispose() {
			logger.Trace("IN ProxyServer.ListenForClientsAsync");
			try {
				cts.Dispose();
				tcpListener.Stop();
			} catch (Exception e) {
				logger.Error(e, "IN ProxyServer.Dispose");
			} finally {
				logger.Trace("OUT ProxyServer.ListenForClientsAsync");
			}

		}
	}
}