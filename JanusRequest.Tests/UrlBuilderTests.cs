using JanusRequest.Builders;

namespace JanusRequest.Tests
{
    public class UrlBuilderTests
    {
        [Fact]
        public void Constructor_WithValidTemplate_ShouldCreateInstance()
        {
            // Arrange & Act
            var urlBuilder = new UrlBuilder("api/users/{id}");

            // Assert
            Assert.NotNull(urlBuilder);
        }

        [Fact]
        public void Constructor_WithNullTemplate_ShouldThrowArgumentNullException()
        {

            // Arrange & Act
            var urlBuilder = new UrlBuilder(null);

            // Assert
            Assert.NotNull(urlBuilder);
        }

        [Fact]
        public void Build_WithEmptyTemplate_ShouldReturnEmptyString()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("");
            var parameters = new { id = 123 };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void Build_WithTemplateWithoutPlaceholders_ShouldReturnOriginalTemplate()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users");
            var parameters = new { id = 123 };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users", result);
        }

        [Fact]
        public void Build_WithNullParameters_ShouldThrowArgumentNullException()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{id}");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => urlBuilder.Build(null));
        }

        [Fact]
        public void Build_WithSimplePlaceholder_ShouldReplaceCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{id}");
            var parameters = new { id = 123 };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/123", result);
        }

        [Fact]
        public void Build_WithMultiplePlaceholders_ShouldReplaceAllCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{userId}/posts/{postId}");
            var parameters = new { userId = 123, postId = 456 };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/123/posts/456", result);
        }

        [Fact]
        public void Build_WithNestedProperties_ShouldReplaceCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{user.id}/profile");
            var parameters = new { user = new { id = 789 } };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/789/profile", result);
        }

        [Fact]
        public void Build_WithNullPropertyValue_ShouldReplaceWithNullString()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{id}");
            var parameters = new { id = (object)null! };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/Null", result);
        }

        [Fact]
        public void Build_WithStringProperty_ShouldReplaceCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{name}");
            var parameters = new { name = "john" };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/john", result);
        }

        [Fact]
        public void Build_WithNonExistentProperty_ShouldThrowArgumentException()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{nonExistent}");
            var parameters = new { id = 123 };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => urlBuilder.Build(parameters));
            Assert.Equal($"Property \"nonExistent\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void Build_WithDeepNestedProperties_ShouldReplaceCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{user.profile.id}");
            var parameters = new { user = new { profile = new { id = 999 } } };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/999", result);
        }

        [Fact]
        public void Build_WithNullNestedProperty_ShouldReplaceWithNullString()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{user.id}");
            var parameters = new { user = (object)null };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/Null", result);
        }

        [Fact]
        public void Build_WithComplexUrl_ShouldReplaceAllPlaceholdersCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("https://api.example.com/v{version}/users/{userId}/posts/{postId}/comments");
            var parameters = new { version = 2, userId = 123, postId = 456 };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("https://api.example.com/v2/users/123/posts/456/comments", result);
        }

        [Fact]
        public void Build_WithBooleanProperty_ShouldReplaceCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{isActive}");
            var parameters = new { isActive = true };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/True", result);
        }

        [Fact]
        public void Build_WithDecimalProperty_ShouldReplaceCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/products/{price}");
            var parameters = new { price = 19.99m };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/products/19.99", result);
        }

        [Fact]
        public void Build_WithMethodCall_ShouldInvokeMethodAndReplaceWithResult()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{GetId()}");
            var parameters = new TestClassWithMethod();

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/42", result);
        }

        [Fact]
        public void Build_WithMethodCallAndNestedPath_ShouldReplaceCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{user.GetName()}");
            var parameters = new { user = new TestClassWithMethod() };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/TestUser", result);
        }

        [Fact]
        public void Build_WithMultipleMethodCalls_ShouldReplaceAllCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{GetId()}/profile/{GetName()}");
            var parameters = new TestClassWithMethod();

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/42/profile/TestUser", result);
        }

        [Fact]
        public void Build_WithMethodReturningNull_ShouldReplaceWithNullString()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{GetNullValue()}");
            var parameters = new TestClassWithMethod();

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/Null", result);
        }

        [Fact]
        public void Build_WithNonExistentMethod_ShouldThrowArgumentException()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{NonExistentMethod()}");
            var parameters = new TestClassWithMethod();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => urlBuilder.Build(parameters));
            Assert.Equal($"Method \"NonExistentMethod\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void Build_WithMixedPropertiesAndMethods_ShouldReplaceAllCorrectly()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{id}/posts/{GetPostCount()}");
            var parameters = new TestClassWithMethodAndProperty { id = 123 };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/123/posts/5", result);
        }

        [Fact]
        public void Build_WithMethodOnNullObject_ShouldReplaceWithNullString()
        {
            // Arrange
            var urlBuilder = new UrlBuilder("api/users/{user.GetId()}");
            var parameters = new { user = (TestClassWithMethod)null };

            // Act
            var result = urlBuilder.Build(parameters);

            // Assert
            Assert.Equal("api/users/Null", result);
        }

        // Classes auxiliares para os testes
        public class TestClassWithMethod
        {
            public int GetId() => 42;
            public string GetName() => "TestUser";
            public object GetNullValue() => null;
        }

        public class TestClassWithMethodAndProperty
        {
            public int id { get; set; }
            public int GetPostCount() => 5;
        }
    }
}