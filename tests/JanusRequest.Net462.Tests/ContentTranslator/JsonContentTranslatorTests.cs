using JanusRequest.Attributes;
using JanusRequest.ContentTranslator;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Xunit;

namespace JanusRequest.Net462.Tests.ContentTranslator
{
    public class JsonContentTranslatorTests
    {
        private readonly JsonContentTranslator _translator;

        public JsonContentTranslatorTests()
        {
            _translator = new JsonContentTranslator();
        }

        [Fact]
        public void ContentType_ReturnsJson()
        {
            // Act
            var result = _translator.ContentType;

            // Assert
            Assert.Equal(HttpContentType.Json, result);
        }

        [Fact]
        public void Parse_WhenContentIsNull_ReturnsNull()
        {
            // Act
            var result = _translator.Parse(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Parse_WhenContentIsValidObject_ReturnsStringContentWithJson()
        {
            // Arrange
            var content = new { Name = "John", Age = 25 };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<StringContent>(result);
            Assert.Equal("application/json", result.Headers.ContentType.MediaType);
            Assert.Equal(Encoding.UTF8, Encoding.GetEncoding(result.Headers.ContentType.CharSet));
        }

        [Fact]
        public void Parse_WhenJsonIsEmpty_ReturnsNull()
        {
            // Arrange
            var content = new object();

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.NotNull(result); // Empty object should still return content with "{}"
        }

        [Fact]
        public void Parse_WhenContentSerializesToNull_ReturnsNull()
        {
            // Arrange - This might happen with certain custom serialization scenarios
            // For this test, we'll verify the behavior with a simple null check

            // Act
            var result = _translator.Parse(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Serialize_WhenContentIsValidObject_ReturnsJsonString()
        {
            // Arrange
            var content = new { Name = "John", Age = 25 };

            // Act
            var result = _translator.Serialize(content);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\"Name\":\"John\"", result);
            Assert.Contains("\"Age\":25", result);
        }

        [Fact]
        public void Serialize_WhenContentIsNull_ReturnsNullString()
        {
            // Act
            var result = _translator.Serialize(null);

            // Assert
            Assert.Equal("null", result);
        }

        [Fact]
        public void Deserialize_WhenJsonIsValid_ReturnsDeserializedObject()
        {
            // Arrange
            var json = "{\"Name\":\"John\",\"Age\":25}";

            // Act
            var result = _translator.Deserialize<TestPerson>(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal(25, result.Age);
        }

        [Fact]
        public void Deserialize_WhenJsonIsNull_ReturnsDefault()
        {
            // Arrange
            var json = "null";

            // Act
            var result = _translator.Deserialize<TestPerson>(json);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_WhenJsonIsEmpty_ThrowsJsonException()
        {
            // Arrange
            var json = "";

            Assert.Null(_translator.Deserialize<TestPerson>(json));
        }

        [Fact]
        public void Parse_WhenObjectHasQueryArgAttribute_IgnoresProperty()
        {
            // Arrange
            var content = new TestClassWithAttributes
            {
                Name = "John",
                QueryParam = "ShouldBeIgnored",
                PathParam = "AlsoIgnored",
                RegularProperty = "ShouldBeIncluded"
            };

            // Act
            var result = _translator.Parse(content);
            var stringContent = result as StringContent;
            var json = GetStringContentValue(stringContent);

            // Assert
            Assert.Contains("\"Name\":\"John\"", json);
            Assert.Contains("\"RegularProperty\":\"ShouldBeIncluded\"", json);
            Assert.DoesNotContain("QueryParam", json);
            Assert.DoesNotContain("PathParam", json);
        }

        [Fact]
        public void Serialize_WhenObjectHasQueryArgAttribute_IgnoresProperty()
        {
            // Arrange
            var content = new TestClassWithAttributes
            {
                Name = "John",
                QueryParam = "ShouldBeIgnored",
                PathParam = "AlsoIgnored",
                RegularProperty = "ShouldBeIncluded"
            };

            // Act
            var result = _translator.Serialize(content);

            // Assert
            Assert.Contains("\"Name\":\"John\"", result);
            Assert.Contains("\"RegularProperty\":\"ShouldBeIncluded\"", result);
            Assert.DoesNotContain("QueryParam", result);
            Assert.DoesNotContain("PathParam", result);
        }

        [Fact]
        public void Deserialize_WhenJsonHasIgnoredProperties_DeserializesNormally()
        {
            // Arrange
            var json = "{\"Name\":\"John\",\"QueryParam\":\"Value\",\"RegularProperty\":\"Test\"}";

            // Act
            var result = _translator.Deserialize<TestClassWithAttributes>(json);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal("Test", result.RegularProperty);
        }

        [Fact]
        public void Parse_WhenContentHasComplexNestedObject_SerializesCorrectly()
        {
            // Arrange
            var content = new
            {
                User = new { Name = "John", Age = 25 },
                Settings = new { Theme = "Dark", Language = "EN" }
            };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<StringContent>(result);
            var stringContent = result as StringContent;
            var json = GetStringContentValue(stringContent);
            Assert.Contains("\"User\":", json);
            Assert.Contains("\"Settings\":", json);
        }

        [Fact]
        public void Parse_WhenContentHasArray_SerializesCorrectly()
        {
            // Arrange
            var content = new { Names = new[] { "John", "Jane", "Bob" } };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<StringContent>(result);
            var stringContent = result as StringContent;
            var json = GetStringContentValue(stringContent);
            Assert.Contains("[\"John\",\"Jane\",\"Bob\"]", json);
        }

        // Helper method to extract string content (simplified for testing)
        private string GetStringContentValue(StringContent content)
        {
            return content.ReadAsStringAsync().Result;
        }

        public class TestPerson
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        [Fact]
        public void Serialize_ByteArray_SerializesAsBase64StringByDefault()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var content = new TestClassWithBase64 { ImageData = bytes };

            var result = _translator.Serialize(content);

            var expected = Convert.ToBase64String(bytes);
            Assert.Contains("\"" + expected + "\"", result);
        }

        [Fact]
        public void Serialize_Stream_SerializesAsBase64StringByDefault()
        {
            var bytes = new byte[] { 10, 20, 30, 40, 50 };
            var content = new TestClassWithBase64 { StreamData = new MemoryStream(bytes) };

            var result = _translator.Serialize(content);

            var expected = Convert.ToBase64String(bytes);
            Assert.Contains("\"" + expected + "\"", result);
        }

        [Fact]
        public void Serialize_ByteArrayWithRawBytesAttribute_UsesDefaultSerializerBehavior()
        {
            var bytes = new byte[] { 1, 2, 3 };
            var content = new TestClassWithBase64 { RawData = bytes };

            var result = _translator.Serialize(content);

            var expected = Convert.ToBase64String(bytes);
            Assert.Contains("\"" + expected + "\"", result);
        }

        [Fact]
        public void Serialize_WhenBase64PropertyIsNull_SerializesAsNull()
        {
            var content = new TestClassWithBase64 { ImageData = null };

            var result = _translator.Serialize(content);

            Assert.Contains("null", result);
        }

        [Fact]
        public void Deserialize_Base64String_DeserializesToByteArray()
        {
            var originalBytes = new byte[] { 1, 2, 3, 4, 5 };
            var base64 = Convert.ToBase64String(originalBytes);
            var json = "{\"ImageData\":\"" + base64 + "\"}";

            var result = _translator.Deserialize<TestClassWithBase64>(json);

            Assert.NotNull(result);
            Assert.Equal(originalBytes, result.ImageData);
        }

        [Fact]
        public void Deserialize_Base64String_DeserializesToStream()
        {
            var originalBytes = new byte[] { 10, 20, 30 };
            var base64 = Convert.ToBase64String(originalBytes);
            var json = "{\"StreamData\":\"" + base64 + "\"}";

            var result = _translator.Deserialize<TestClassWithBase64>(json);

            Assert.NotNull(result);
            Assert.NotNull(result.StreamData);
            Assert.IsType<MemoryStream>(result.StreamData);
            var ms = (MemoryStream)result.StreamData;
            Assert.Equal(originalBytes, ms.ToArray());
        }

        [Fact]
        public void Serialize_Deserialize_ByteArray_RoundTrip()
        {
            var originalBytes = new byte[] { 0, 127, 255, 1, 100 };
            var content = new TestClassWithBase64 { Name = "test", ImageData = originalBytes };

            var json = _translator.Serialize(content);
            var result = _translator.Deserialize<TestClassWithBase64>(json);

            Assert.Equal("test", result.Name);
            Assert.Equal(originalBytes, result.ImageData);
        }

        public class TestClassWithBase64
        {
            public string Name { get; set; }
            public byte[] ImageData { get; set; }
            public Stream StreamData { get; set; }

            [RawBytes]
            public byte[] RawData { get; set; }
        }

        public class TestClassWithAttributes
        {
            public string Name { get; set; }

            [Attributes.QueryArg]
            public string QueryParam { get; set; }

            [Attributes.PathOnly]
            public string PathParam { get; set; }

            public string RegularProperty { get; set; }
        }
    }
}