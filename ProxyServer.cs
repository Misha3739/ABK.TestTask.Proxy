using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy {
	public class ProxyServer : IProxyServer, IDisposable {
		private readonly int listeningPort;
		private TcpListener tcpListener;
		private readonly IServersManager serversManager;
		private readonly ILogger logger;

		private int connectedClients;

		private readonly CancellationTokenSource cts;
		public ProxyServer(int listeningPort, ILogger logger, IServersManager serversManager) {
			this.listeningPort = listeningPort;

			this.logger = logger;
			this.serversManager = serversManager;

			tcpListener = new TcpListener(IPAddress.Any, this.listeningPort);
			cts = new CancellationTokenSource();
			connectedClients = 0;
		}

		public Task ListenForClients() {
			logger.Trace("IN ProxyServer.ListenForClients");
			try {
				tcpListener.Start();
				Task listeningTask = Task.Run(async () => {
					while (!cts.IsCancellationRequested) {
						var client = await tcpListener.AcceptTcpClientAsync();
						var clientPort = ((IPEndPoint)client.Client.RemoteEndPoint).Port;
						Interlocked.Increment(ref connectedClients);
						logger.Info($"IN ProxyServer.ListenForClients, New client attached, listeningPort = \"{clientPort}\"");
						logger.Info($"CURRENT CLIENTS = \"{connectedClients}\"");
						var server = serversManager.FindServerAndIncrementConnections();
						Task clientTask = HandleTcpClient(client, server);
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

						Task clintToServerTask = clientStream.CopyToAsync(serverStream);
						Task serverToClientTask = serverStream.CopyToAsync(clientStream);
						await Task.WhenAny(clintToServerTask, serverToClientTask);
					} catch (Exception e) {
						logger.Error(e, "Streaming error occurred...");
					} finally {
						try {
							inboundClient.Close();
							serverClient.Close();
						} catch (Exception e) {
							logger.Error(e, "Streaming dispose error occurred...");
						}
						serversManager.DecrementConnections(server);
						logger.Info($"IN ProxyServer.HandleTcpClient, client detached, listeningPort = \"{clientPort}\"");
						Interlocked.Decrement(ref connectedClients);
						logger.Info($"CURRENT CLIENTS = \"{connectedClients}\"");
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