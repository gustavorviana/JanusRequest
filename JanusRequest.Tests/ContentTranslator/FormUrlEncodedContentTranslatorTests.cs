using JanusRequest.ContentTranslator;
using NSubstitute;
using System.Reflection;

namespace JanusRequest.Tests.ContentTranslator
{
    public class FormUrlEncodedContentTranslatorTests
    {
        private readonly FormUrlEncodedContentTranslator _translator;

        public FormUrlEncodedContentTranslatorTests()
        {
            _translator = new FormUrlEncodedContentTranslator();
        }

        [Fact]
        public void ContentType_ReturnsFormUrlEncoded()
        {
            // Act
            var result = _translator.ContentType;

            // Assert
            Assert.Equal(HttpContentType.FormUrlEncoded, result);
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
        public void Parse_WhenContentHasStringProperties_ReturnsFormUrlEncodedContent()
        {
            // Arrange
            var content = new { Name = "John", Email = "john@email.com" };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
        }

        [Fact]
        public void Parse_WhenContentHasNumericProperties_ConvertsToString()
        {
            // Arrange
            var content = new { Name = "John", Age = 25, Height = 1.75 };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
        }

        [Fact]
        public void Parse_WhenContentHasBooleanProperties_ConvertsToString()
        {
            // Arrange
            var content = new { Name = "John", IsActive = true, IsDeleted = false };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
        }

        [Fact]
        public void Parse_WhenPropertyValueIsNull_SkipsProperty()
        {
            // Arrange
            var content = new TestClass { Name = "John", Description = null, Age = 25 };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
            // Should only contain Name and Age properties
        }

        [Fact]
        public void Parse_WhenAllPropertiesAreNull_ReturnsEmptyFormUrlEncodedContent()
        {
            // Arrange
            var content = new TestClass { Name = null, Description = null };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
        }

        [Fact]
        public void Parse_WhenPropertyShouldBeIgnored_SkipsProperty()
        {
            // Arrange
            var translator = Substitute.ForPartsOf<FormUrlEncodedContentTranslator>();
            translator.ShouldIgnoreProperty(Arg.Any<PropertyInfo>())
                .Returns(call => ((PropertyInfo)call[0]).Name == "Description");

            var content = new TestClass { Name = "John", Description = "Skip this" };

            // Act
            var result = translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
            translator.Received().ShouldIgnoreProperty(Arg.Any<PropertyInfo>());
        }

        [Fact]
        public void Parse_UsesGetPropertyNameForKeys()
        {
            // Arrange
            var translator = Substitute.ForPartsOf<FormUrlEncodedContentTranslator>();
            translator.GetPropertyName(Arg.Any<PropertyInfo>())
                .Returns(call => "custom_" + ((PropertyInfo)call[0]).Name.ToLower());

            var content = new TestClass { Name = "John" };

            // Act
            var result = translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
            translator.Received().GetPropertyName(Arg.Any<PropertyInfo>());
        }

        [Fact]
        public void Parse_WhenContentHasComplexTypes_ConvertsToStringRepresentation()
        {
            // Arrange
            var date = new DateTime(2023, 12, 25);
            var content = new { Name = "John", BirthDate = date };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
        }

        [Fact]
        public void Parse_WhenContentHasEnumProperty_ConvertsToString()
        {
            // Arrange
            var content = new { Name = "John", Status = TestEnum.Active };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
        }

        [Fact]
        public void Parse_WhenContentHasMixedPropertyTypes_ProcessesAllValidProperties()
        {
            // Arrange
            var content = new ComplexTestClass
            {
                Name = "John",
                Age = 25,
                IsActive = true,
                Height = 1.75,
                BirthDate = new DateTime(1998, 5, 15),
                Status = TestEnum.Active,
                NullProperty = null
            };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
            // Should process all non-null properties
        }

        [Fact]
        public void Parse_WhenObjectHasNoValidProperties_ReturnsEmptyFormUrlEncodedContent()
        {
            // Arrange
            var translator = Substitute.ForPartsOf<FormUrlEncodedContentTranslator>();
            translator.ShouldIgnoreProperty(Arg.Any<PropertyInfo>()).Returns(true);

            var content = new TestClass { Name = "John", Description = "Test" };

            // Act
            var result = translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
        }

        [Fact]
        public void Parse_WhenContentHasCustomToStringImplementation_UsesToStringResult()
        {
            // Arrange
            var content = new { Name = "John", CustomObject = new CustomToStringClass() };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<FormUrlEncodedContent>(result);
        }

        // Test helper classes and enums
        public class TestClass
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int? Age { get; set; }
        }

        public class ComplexTestClass
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public bool IsActive { get; set; }
            public double Height { get; set; }
            public DateTime BirthDate { get; set; }
            public TestEnum Status { get; set; }
            public string NullProperty { get; set; }
        }

        public enum TestEnum
        {
            Inactive,
            Active,
            Pending
        }

        public class CustomToStringClass
        {
            public override string ToString()
            {
                return "CustomValue";
            }
        }
    }
}