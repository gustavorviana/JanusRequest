using System;
using System.Collections.Concurrent;

namespace JanusRequest.Extensions.DependencyInjection
{
    internal class HttpApiClientConfiguratorRegistry
    {
        private readonly ConcurrentDictionary<string, Action<IServiceProvider, HttpApiClient>> _configurators = new ConcurrentDictionary<string, Action<IServiceProvider, HttpApiClient>>();

        public Action<IServiceProvider, HttpApiClient> Get(string name)
        {
            if (name == null)
                return null;

            if (_configurators.TryGetValue(name, out var configurator))
                return configurator;

            return null;
        }

        public void Register(string name, Action<IServiceProvider, HttpApiClient> clientConfigurator)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty or whitespace.", nameof(name));

            _configurators[name] = clientConfigurator;
        }
    }
}