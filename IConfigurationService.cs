using System.Collections.Generic;
using System.Text;

namespace Proxy {
	public interface IConfigurationService {
		T GetValue<T>(string key);
	}
}
