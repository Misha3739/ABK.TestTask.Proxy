using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy {
	public class ProxyServer : IProxyServer, IDisposable {
		private readonly int listeningPort;
		private readonly int chunkSize = 2048;

		private TcpListener tcpListener;
		private readonly IServersManager serversManager;
		private readonly ILogger logger;

		private readonly CancellationTokenSource cts;
		public ProxyServer(int listeningPort, ILogger logger, IServersManager serversManager) {
			this.listeningPort = listeningPort;

			this.logger = logger;
			this.serversManager = serversManager;

			tcpListener = new TcpListener(IPAddress.Any, this.listeningPort);
			cts = new CancellationTokenSource();
		}

		public Task ListenForClients() {
			logger.Trace("IN ProxyServer.ListenForClients");
			try {
				tcpListener.Start();
				Task listeningTask = Task.Run(async () => {
					while (!cts.IsCancellationRequested) {
						var client = await tcpListener.AcceptTcpClientAsync();
						var clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
						logger.Info($"IN ProxyServer.ListenForClients, New client attached, listeningPort = \"{clientPort}\"");
						var server = serversManager.FindServerAndIncrementConnections();
						Task clientTask = this.HandleTcpClient(client, server);
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

		internal Task HandleTcpClient(TcpClient inboundClient, Server server) {
			logger.Trace("IN ProxyServer.HandleTcpClient");
			try {
				var clientPort = ((IPEndPoint)inboundClient.Client.RemoteEndPoint).Port;
				var serverClient = new TcpClient(server.IP, server.Port);
				Task clientTask = Task.Run(async () => {
					try {
						await using NetworkStream clientStream = inboundClient.GetStream();
						await using NetworkStream serverStream = serverClient.GetStream();
						logger.Info(
							$"Start streaming to server from client port = \"{clientPort}\" to server port = \"{server.Port}\"");
						await clientStream.CopyToAsync(serverStream);
						logger.Info(
							$"Start streaming to client from server port = \"{server.Port}\" to client port = \"{clientPort}\"");
						await serverStream.CopyToAsync(clientStream);
						logger.Info($"Streaming within client port = \"{clientPort}\" and server port = \"{server.Port}\" completed");
					} finally {
						inboundClient.Close();
						serverClient.Close();
						serversManager.DecrementConnections(server);
						logger.Info($"IN ProxyServer.HandleTcpClient, client detached, listeningPort = \"{clientPort}\"");
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