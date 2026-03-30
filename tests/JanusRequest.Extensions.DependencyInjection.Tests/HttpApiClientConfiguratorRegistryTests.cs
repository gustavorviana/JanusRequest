namespace JanusRequest.Extensions.DependencyInjection.Tests
{
    public class HttpApiClientConfiguratorRegistryTests
    {
        [Fact]
        public void Register_NullName_ThrowsArgumentNullException()
        {
            // Arrange
            var registry = new HttpApiClientConfiguratorRegistry();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                registry.Register(null!, (provider, client) => { }));
        }

        [Fact]
        public void Register_EmptyName_ThrowsArgumentException()
        {
            // Arrange
            var registry = new HttpApiClientConfiguratorRegistry();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                registry.Register("", (provider, client) => { }));
        }

        [Fact]
        public void Register_WhitespaceName_ThrowsArgumentException()
        {
            // Arrange
            var registry = new HttpApiClientConfiguratorRegistry();

            // Act & Assert
            Assert.Throws<ArgumentException>(() =>
                registry.Register("   ", (provider, client) => { }));
        }

        [Fact]
        public void Register_ValidName_CanBeRetrieved()
        {
            // Arrange
            var registry = new HttpApiClientConfiguratorRegistry();
            Action<IServiceProvider, HttpApiClient> configurator = (provider, client) => { };

            // Act
            registry.Register("test-client", configurator);

            // Assert
            var result = registry.Get("test-client");
            Assert.Same(configurator, result);
        }

        [Fact]
        public void Get_NullName_ReturnsNull()
        {
            // Arrange
            var registry = new HttpApiClientConfiguratorRegistry();
            registry.Register("test", (provider, client) => { });

            // Act
            var result = registry.Get(null!);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Get_NonExistentName_ReturnsNull()
        {
            // Arrange
            var registry = new HttpApiClientConfiguratorRegistry();

            // Act
            var result = registry.Get("non-existent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Register_SameNameTwice_OverwritesPrevious()
        {
            // Arrange
            var registry = new HttpApiClientConfiguratorRegistry();
            Action<IServiceProvider, HttpApiClient> first = (provider, client) => { };
            Action<IServiceProvider, HttpApiClient> second = (provider, client) => { };

            // Act
            registry.Register("client", first);
            registry.Register("client", second);

            // Assert
            var result = registry.Get("client");
            Assert.Same(second, result);
        }
    }
}
