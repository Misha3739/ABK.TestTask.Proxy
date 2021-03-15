using System.Collections.Generic;

namespace Proxy {
	public interface IServersManager {
		Server FindServerAndIncrement();
		void Decrement(Server server);
	}
}