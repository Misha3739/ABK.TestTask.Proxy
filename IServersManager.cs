using System.Collections.Generic;

namespace Proxy {
	public interface IServersManager {
		IDictionary<int, Server> Servers { get; }
		int GetAvailableServerKey();
		void ReleaseServer(int key);
	}
}