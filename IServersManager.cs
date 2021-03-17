using System.Collections.Generic;

namespace Proxy {
	public interface IServersManager {
		Server FindServerAndIncrementConnections();
		void DecrementConnections(Server server);
	}
}