using System;
using System.Collections.Generic;
using System.Text;

namespace JanusRequest.Extensions.DependencyInjection
{
    internal class HttpApiClientConfiguratorRegistry
    {
        private readonly Dictionary<string, Action<IServiceProvider, HttpApiClient>> _configurators = new Dictionary<string, Action<IServiceProvider, HttpApiClient>>();

        public Action<IServiceProvider, HttpApiClient> Get(string name)
        {
            if (_configurators.TryGetValue(name, out var configurator))
                return configurator;

            return null;
        }

        public void Register(string name, Action<IServiceProvider, HttpApiClient> clientConfigurator)
        {
            _configurators[name] = clientConfigurator;
        }
    }
}