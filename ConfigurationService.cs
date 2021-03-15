using System;
using System.Configuration;

namespace Proxy {
	public class ConfigurationService : IConfigurationService {
		public T GetValue<T>(string key) {
			var value = ConfigurationManager.AppSettings[key];
			var tType = typeof(T);
			if (!string.IsNullOrEmpty(value) && (tType.IsPrimitive || tType == typeof(string))) {
				return (T)Convert.ChangeType(value, typeof(T));
			}
			return default(T);
		}
	}
}