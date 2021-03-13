using System;
using System.Threading.Tasks;

namespace Proxy {
	public interface IProxyServer : IDisposable {
		Task ListenForClients();
	}
}