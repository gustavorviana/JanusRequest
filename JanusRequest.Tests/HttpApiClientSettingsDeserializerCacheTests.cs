using JanusRequest.Attributes;

namespace JanusRequest.Tests
{
    public class HttpApiClientSettingsDeserializerCacheTests
    {
        private readonly HttpApiClientSettings _settings;

        public HttpApiClientSettingsDeserializerCacheTests()
        {
            _settings = new HttpApiClientSettings();
        }

        public class TestResponse { }

        public class TestDeserializer : IResponseDeserializer<TestResponse>
        {
            public Task<TestResponse> DeserializeAsync(HttpResponseMessage response, HttpApiClientSettings settings)
                => Task.FromResult(new TestResponse());
        }

        [ResponseDeserializer(typeof(TestDeserializer))]
        public class ResponseWithAttribute { }

        [ResponseDeserializer(typeof(TestDeserializer))]
        public class PlainTypeWithAttribute { }

        // A request type that implements IRequestResponse<TResponse, TDeserializer>
        public class RequestWithDeserializer : IRequestResponse<TestResponse, TestDeserializer> { }

        // A type that does NOT implement IRequestResponse<,>
        public class PlainType { }

        [Fact]
        public void GetDeserializerType_WithIRequestResponse_ReturnsDeserializerType()
        {
            // Act
            var result = _settings.GetDeserializerType(typeof(RequestWithDeserializer));

            // Assert
            Assert.Equal(typeof(TestDeserializer), result);
        }

        [Fact]
        public void GetDeserializerType_WithResponseDeserializerAttribute_ReturnsDeserializerType()
        {
            // Act
            var result = _settings.GetDeserializerType(typeof(ResponseWithAttribute));

            // Assert
            Assert.Equal(typeof(TestDeserializer), result);
        }

        [Fact]
        public void GetDeserializerType_WithAttributeAndWithoutIRequestResponse_ReturnsDeserializerType()
        {
            // Act
            var result = _settings.GetDeserializerType(typeof(PlainTypeWithAttribute));

            // Assert
            Assert.Equal(typeof(TestDeserializer), result);
        }

        [Fact]
        public void GetDeserializerType_WithoutIRequestResponseOrAttribute_ReturnsNull()
        {
            // Act
            var result = _settings.GetDeserializerType(typeof(PlainType));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetDeserializerType_WithAttribute_SecondCall_ReturnsSameResult()
        {
            // Act
            var first = _settings.GetDeserializerType(typeof(ResponseWithAttribute));
            var second = _settings.GetDeserializerType(typeof(ResponseWithAttribute));

            // Assert
            Assert.Equal(typeof(TestDeserializer), first);
            Assert.Same(first, second);
        }

        [Fact]
        public void GetDeserializerType_SecondCall_ReturnsSameResult()
        {
            // Act
            var first = _settings.GetDeserializerType(typeof(RequestWithDeserializer));
            var second = _settings.GetDeserializerType(typeof(RequestWithDeserializer));

            // Assert
            Assert.Equal(typeof(TestDeserializer), first);
            Assert.Same(first, second);
        }

        [Fact]
        public void GetDeserializerType_WithoutDeserializer_SecondCallReturnsNullWithoutReReflection()
        {
            // Act - first call triggers reflection, second should use cache
            var first = _settings.GetDeserializerType(typeof(PlainType));
            var second = _settings.GetDeserializerType(typeof(PlainType));

            // Assert - both return null (sentinel working)
            Assert.Null(first);
            Assert.Null(second);
        }

        [Fact]
        public void ClearDeserializerTypeCache_AfterClear_AttributeResolutionRunsAgain()
        {
            // Arrange - populate cache
            _settings.GetDeserializerType(typeof(ResponseWithAttribute));
            _settings.GetDeserializerType(typeof(PlainType));

            // Act
            var returnValue = _settings.ClearDeserializerTypeCache();

            // Assert - method returns 'this' for fluent chaining
            Assert.Same(_settings, returnValue);

            var result = _settings.GetDeserializerType(typeof(ResponseWithAttribute));
            Assert.Equal(typeof(TestDeserializer), result);

            var nullResult = _settings.GetDeserializerType(typeof(PlainType));
            Assert.Null(nullResult);
        }

        [Fact]
        public void ClearDeserializerTypeCache_AfterClear_ReflectionRunsAgain()
        {
            // Arrange - populate cache
            _settings.GetDeserializerType(typeof(RequestWithDeserializer));
            _settings.GetDeserializerType(typeof(PlainType));

            // Act
            var returnValue = _settings.ClearDeserializerTypeCache();

            // Assert - method returns 'this' for fluent chaining
            Assert.Same(_settings, returnValue);

            // After clear, calls still return correct results (reflection runs again)
            var result = _settings.GetDeserializerType(typeof(RequestWithDeserializer));
            Assert.Equal(typeof(TestDeserializer), result);

            var nullResult = _settings.GetDeserializerType(typeof(PlainType));
            Assert.Null(nullResult);
        }

        // Additional fixture types for concurrent different-type resolution
        public class TestResponse2 { }
        public class TestDeserializer2 : IResponseDeserializer<TestResponse2>
        {
            public Task<TestResponse2> DeserializeAsync(HttpResponseMessage response, HttpApiClientSettings settings)
                => Task.FromResult(new TestResponse2());
        }
        public class RequestWithDeserializer2 : IRequestResponse<TestResponse2, TestDeserializer2> { }

        [ResponseDeserializer(typeof(TestDeserializer2))]
        public class ResponseWithAttribute2 { }

        public class TestResponse3 { }
        public class TestDeserializer3 : IResponseDeserializer<TestResponse3>
        {
            public Task<TestResponse3> DeserializeAsync(HttpResponseMessage response, HttpApiClientSettings settings)
                => Task.FromResult(new TestResponse3());
        }
        public class RequestWithDeserializer3 : IRequestResponse<TestResponse3, TestDeserializer3> { }

        [Fact]
        public async Task ConcurrentAccess_SameType_NoExceptionsAndCorrectResult()
        {
            // Arrange
            const int concurrency = 50;
            var tasks = new Task<Type>[concurrency];

            // Act - many threads resolve the same type simultaneously
            for (int i = 0; i < concurrency; i++)
            {
                tasks[i] = Task.Run(() => _settings.GetDeserializerType(typeof(RequestWithDeserializer)));
            }

            var results = await Task.WhenAll(tasks);

            // Assert - all results are correct
            foreach (var result in results)
            {
                Assert.Equal(typeof(TestDeserializer), result);
            }
        }

        [Fact]
        public async Task ConcurrentAccess_SameAttributeType_NoExceptionsAndCorrectResult()
        {
            // Arrange
            const int concurrency = 50;
            var tasks = new Task<Type>[concurrency];

            // Act - many threads resolve the same type simultaneously
            for (int i = 0; i < concurrency; i++)
            {
                tasks[i] = Task.Run(() => _settings.GetDeserializerType(typeof(ResponseWithAttribute)));
            }

            var results = await Task.WhenAll(tasks);

            // Assert - all results are correct
            foreach (var result in results)
            {
                Assert.Equal(typeof(TestDeserializer), result);
            }
        }

        [Fact]
        public async Task ConcurrentAccess_DifferentTypes_NoExceptionsAndCorrectResults()
        {
            // Arrange
            const int concurrency = 50;
            var types = new[]
            {
                typeof(RequestWithDeserializer),
                typeof(RequestWithDeserializer2),
                typeof(RequestWithDeserializer3),
                typeof(ResponseWithAttribute),
                typeof(ResponseWithAttribute2),
                typeof(PlainType)
            };
            var expectedDeserializers = new Dictionary<Type, Type>
            {
                { typeof(RequestWithDeserializer), typeof(TestDeserializer) },
                { typeof(RequestWithDeserializer2), typeof(TestDeserializer2) },
                { typeof(RequestWithDeserializer3), typeof(TestDeserializer3) },
                { typeof(ResponseWithAttribute), typeof(TestDeserializer) },
                { typeof(ResponseWithAttribute2), typeof(TestDeserializer2) },
                { typeof(PlainType), null }
            };

            var tasks = new List<Task>();

            // Act - many threads resolve different types simultaneously
            for (int i = 0; i < concurrency; i++)
            {
                var type = types[i % types.Length];
                var expected = expectedDeserializers[type];
                tasks.Add(Task.Run(() =>
                {
                    var result = _settings.GetDeserializerType(type);
                    Assert.Equal(expected, result);
                }));
            }

            // Assert - no exceptions thrown
            await Task.WhenAll(tasks);
        }
    }
}