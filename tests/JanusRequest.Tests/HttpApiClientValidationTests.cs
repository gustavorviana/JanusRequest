using System.ComponentModel.DataAnnotations;
using System.Net;

namespace JanusRequest.Tests
{
    public class HttpApiClientValidationTests : HttpApiClientTestBase
    {
        [Fact]
        public async Task ValidateRequest_WithValidModel_DoesNotThrowAsync()
        {
            // Arrange
            var request = new ValidatedRequest { Name = "Valid" };
            _settings.ValidateRequest = true;
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Fact]
        public async Task ValidateRequest_WithInvalidModel_ThrowsValidationExceptionAsync()
        {
            // Arrange
            var request = new ValidatedRequest { Name = null };
            _settings.ValidateRequest = true;

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => _httpApiClient.SendAsync(request));
        }

        [Fact]
        public async Task ValidateRequest_WithInvalidModel_ThrowsValidationExceptionWithExpectedFormatAsync()
        {
            // Arrange
            var request = new ValidatedRequest { Name = null };
            _settings.ValidateRequest = true;
            const string expectedErrorMessage = "Name is required";

            // Act
            var exception = await Assert.ThrowsAsync<ValidationException>(() => _httpApiClient.SendAsync(request));

            // Assert
            Assert.NotNull(exception.Message);
            Assert.Contains(expectedErrorMessage, exception.Message);
            Assert.NotNull(exception.ValidationResult);
            Assert.Equal(expectedErrorMessage, exception.ValidationResult.ErrorMessage);
            Assert.Contains("Name", exception.ValidationResult.MemberNames);
        }

        [Fact]
        public async Task ValidateRequest_WhenDisabled_DoesNotValidateAsync()
        {
            // Arrange
            var request = new ValidatedRequest { Name = null };
            _settings.ValidateRequest = false;
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");

            // Act
            var result = await _httpApiClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Fact]
        public async Task ValidateRequest_WithNullBody_DoesNotValidateAsync()
        {
            // Arrange
            _settings.ValidateRequest = true;
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");
            var info = new HttpRequestInfo { Path = "/test", Method = "GET" };

            // Act & Assert
            var result = await _httpApiClient.SendAsync<TestResponse>(info);
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }

        [Fact]
        public async Task ValidateRequest_WithNativeType_DoesNotValidateAsync()
        {
            // Arrange
            _settings.ValidateRequest = true;
            SetupHttpResponse(HttpStatusCode.OK, "{\"Id\":1,\"Name\":\"Test\"}");
            var info = new HttpRequestInfo { Path = "/test", Method = "POST" };

            // Act & Assert
            var result = await _httpApiClient.SendRequestAsync("string body", info);
            Assert.Equal(HttpStatusCode.OK, result.Status);
        }
    }
}
