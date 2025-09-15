using JanusRequest.ContentTranslator;
using System.Text;
using System.Xml.Serialization;

namespace JanusRequest.Tests.ContentTranslator
{
    public class XmlContentTranslatorTests
    {
        private readonly XmlContentTranslator _translator;

        public XmlContentTranslatorTests()
        {
            _translator = new XmlContentTranslator();
        }

        [Fact]
        public void ContentType_ReturnsXml()
        {
            // Act
            var result = _translator.ContentType;

            // Assert
            Assert.Equal(HttpContentType.Xml, result);
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
        public void Parse_WhenContentIsValidObject_ReturnsStringContentWithXml()
        {
            // Arrange
            var content = new TestPerson { Name = "John", Age = 25 };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<StringContent>(result);
            Assert.Equal("application/xml", result.Headers.ContentType.MediaType);
            Assert.Equal(Encoding.UTF8, Encoding.GetEncoding(result.Headers.ContentType.CharSet));
        }

        [Fact]
        public void Parse_WhenSerializeReturnsNull_ReturnsNull()
        {
            // This test verifies the null check after serialization
            // Since we can't easily mock the internal Serialize method,
            // we'll test with null content which should return null from Serialize

            // Act
            var result = _translator.Parse(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Serialize_WhenContentIsNull_ReturnsNull()
        {
            // Act
            var result = _translator.Serialize(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Serialize_WhenContentIsValidObject_ReturnsXmlString()
        {
            // Arrange
            var content = new TestPerson { Name = "John", Age = 25 };

            // Act
            var result = _translator.Serialize(content);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("<?xml version=\"1.0\" encoding=\"utf-16\"?>", result);
            Assert.Contains("<TestPerson", result);
            Assert.Contains("<Name>John</Name>", result);
            Assert.Contains("<Age>25</Age>", result);
        }

        [Fact]
        public void Serialize_CreatesIndentedXml()
        {
            // Arrange
            var content = new TestPerson { Name = "John", Age = 25 };

            // Act
            var result = _translator.Serialize(content);

            // Assert
            Assert.Contains("\r\n", result); // Should contain line breaks due to indentation
        }

        [Fact]
        public void Serialize_IncludesXmlDeclaration()
        {
            // Arrange
            var content = new TestPerson { Name = "John", Age = 25 };

            // Act
            var result = _translator.Serialize(content);

            // Assert
            Assert.StartsWith("<?xml version=\"1.0\"", result);
        }

        [Fact]
        public void Deserialize_WhenXmlIsNull_ReturnsDefault()
        {
            // Act
            var result = _translator.Deserialize<TestPerson>(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_WhenXmlIsEmpty_ReturnsDefault()
        {
            // Act
            var result = _translator.Deserialize<TestPerson>("");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_WhenXmlIsWhitespace_ReturnsDefault()
        {
            // Act
            var result = _translator.Deserialize<TestPerson>("   ");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Deserialize_WhenXmlIsValid_ReturnsDeserializedObject()
        {
            // Arrange
            var xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                     "<TestPerson><Name>John</Name><Age>25</Age></TestPerson>";

            // Act
            var result = _translator.Deserialize<TestPerson>(xml);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal(25, result.Age);
        }

        [Fact]
        public void Deserialize_WhenXmlIsInvalid_ThrowsInvalidOperationException()
        {
            // Arrange
            var invalidXml = "<invalid>xml<content>";

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _translator.Deserialize<TestPerson>(invalidXml));
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
            Assert.Contains("<Name>John</Name>", result);
            Assert.Contains("<RegularProperty>ShouldBeIncluded</RegularProperty>", result);
            Assert.DoesNotContain("QueryParam", result);
            Assert.DoesNotContain("PathParam", result);
        }

        [Fact]
        public void Deserialize_WhenXmlHasIgnoredProperties_IgnoresThemDuringDeserialization()
        {
            // Arrange
            var xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                     "<TestClassWithAttributes>" +
                     "<Name>John</Name>" +
                     "<QueryParam>Value</QueryParam>" +
                     "<RegularProperty>Test</RegularProperty>" +
                     "</TestClassWithAttributes>";

            // Act
            var result = _translator.Deserialize<TestClassWithAttributes>(xml);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.Name);
            Assert.Equal("Test", result.RegularProperty);
            // QueryParam should be ignored during deserialization
            Assert.Null(result.QueryParam);
        }

        [Fact]
        public void SerializeAndDeserialize_RoundTrip_WorksCorrectly()
        {
            // Arrange
            var original = new TestPerson { Name = "John", Age = 25 };

            // Act
            var xml = _translator.Serialize(original);
            var deserialized = _translator.Deserialize<TestPerson>(xml);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(original.Name, deserialized.Name);
            Assert.Equal(original.Age, deserialized.Age);
        }

        [Fact]
        public void Parse_WhenContentHasComplexNestedObject_SerializesCorrectly()
        {
            // Arrange
            var content = new TestComplexObject
            {
                User = new TestPerson { Name = "John", Age = 25 },
                Description = "Test Description"
            };

            // Act
            var result = _translator.Parse(content);

            // Assert
            Assert.IsType<StringContent>(result);
            var stringContent = result as StringContent;
            var xml = GetStringContentValue(stringContent);
            Assert.Contains("<User>", xml);
            Assert.Contains("<Name>John</Name>", xml);
            Assert.Contains("<Description>Test Description</Description>", xml);
        }

        [Fact]
        public void CreateXmlAttributeOverrides_WhenTypeHasNoIgnoredProperties_ReturnsEmptyOverrides()
        {
            // This test verifies the private method behavior indirectly
            // by checking that normal properties are serialized
            // Arrange
            var content = new TestPerson { Name = "John", Age = 25 };

            // Act
            var result = _translator.Serialize(content);

            // Assert
            Assert.Contains("<Name>John</Name>", result);
            Assert.Contains("<Age>25</Age>", result);
        }

        // Helper method to extract string content
        private string GetStringContentValue(StringContent content)
        {
            return content.ReadAsStringAsync().Result;
        }


        // Test helper classes
        [XmlRoot("TestPerson")]
        public class TestPerson
        {
            public string Name { get; set; }
            public int Age { get; set; }
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

        public class TestComplexObject
        {
            public TestPerson User { get; set; }
            public string Description { get; set; }
        }
    }
}