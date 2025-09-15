using JanusRequest.ContentTranslator;
using NSubstitute;
using System.Reflection;

namespace JanusRequest.Tests.ContentTranslator
{
    public class FormDataContentTranslatorTests
    {
        private readonly FormDataContentTranslator _translator;

        public FormDataContentTranslatorTests()
        {
            _translator = new FormDataContentTranslator();
        }

        [Fact]
        public void ContentType_ReturnsFormData()
        {
            // Act
            var result = _translator.ContentType;

            // Assert
            Assert.Equal(HttpContentType.FormData, result);
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
        public void Parse_WhenContentHasStringProperty_AddsStringContent()
        {
            // Arrange
            var content = new { Name = "Test", Age = 25 };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
            var multipartContent = (MultipartFormDataContent)result;
            Assert.NotEmpty(multipartContent);
        }

        [Fact]
        public void Parse_WhenContentHasStreamProperty_AddsStreamContent()
        {
            // Arrange
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var content = new TestClassWithStream { File = stream, Name = "Test" };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
            var multipartContent = (MultipartFormDataContent)result;
            Assert.NotEmpty(multipartContent);
        }

        [Fact]
        public void Parse_WhenContentHasByteArrayProperty_AddsByteArrayContent()
        {
            // Arrange
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var content = new TestClassWithByteArray { Data = bytes, Name = "Test" };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
            var multipartContent = (MultipartFormDataContent)result;
            Assert.NotEmpty(multipartContent);
        }

        [Fact]
        public void Parse_WhenPropertyValueIsNull_SkipsProperty()
        {
            // Arrange
            var content = new TestClass { Name = "Test", Description = null };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
            // Should only contain Name property, Description should be skipped
        }

        [Fact]
        public void Parse_WhenPropertyShouldBeIgnored_SkipsProperty()
        {
            // Arrange
            var translator = Substitute.ForPartsOf<FormDataContentTranslator>();
            translator.ShouldIgnoreProperty(Arg.Any<PropertyInfo>())
                .Returns(call => ((PropertyInfo)call[0]).Name == "Description");

            var content = new TestClass { Name = "Test", Description = "Skip this" };

            // Act
            var result = translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
        }

        [Fact]
        public void Parse_UsesGetPropertyNameForFieldNames()
        {
            // Arrange
            var translator = Substitute.ForPartsOf<FormDataContentTranslator>();
            translator.GetPropertyName(Arg.Any<PropertyInfo>())
                .Returns(call => "custom_" + ((PropertyInfo)call[0]).Name.ToLower());

            var content = new TestClass { Name = "Test" };

            // Act
            var result = translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
            translator.Received().GetPropertyName(Arg.Any<PropertyInfo>());
        }

        [Fact]
        public void Parse_WhenStreamContentAdded_UsesPropertyNameForBothNameAndFileName()
        {
            // Arrange
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var content = new TestClassWithStream { File = stream };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
        }

        [Fact]
        public void Parse_WhenByteArrayContentAdded_UsesPropertyNameForBothNameAndFileName()
        {
            // Arrange
            var bytes = new byte[] { 1, 2, 3, 4, 5 };
            var content = new TestClassWithByteArray { Data = bytes };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
        }

        [Fact]
        public void Parse_WhenMixedPropertyTypes_AddsAllValidProperties()
        {
            // Arrange
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var bytes = new byte[] { 4, 5, 6 };
            var content = new ComplexTestClass
            {
                Name = "Test",
                Age = 25,
                File = stream,
                Data = bytes,
                NullProperty = null
            };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<MultipartFormDataContent>(result);
            var multipartContent = (MultipartFormDataContent)result;
            Assert.NotEmpty(multipartContent);
        }

        public class TestClass
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }

        public class TestClassWithStream
        {
            public string Name { get; set; }
            public Stream File { get; set; }
        }

        public class TestClassWithByteArray
        {
            public string Name { get; set; }
            public byte[] Data { get; set; }
        }

        public class ComplexTestClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Stream File { get; set; }
            public byte[] Data { get; set; }
            public string NullProperty { get; set; }
        }
    }
}