using System.Net.Http;
using System.Text;
using JanusRequest.ContentTranslator;
using Newtonsoft.Json;
using Xunit;

namespace JanusRequest.Net45.Tests
{
    /// <summary>
    /// Tests that validate the JSON parsing behavior for the .NET Framework 4.5 build,
    /// which uses the Newtonsoft.Json-based pipeline inside JsonContentTranslator.
    /// </summary>
    public class JsonContentTranslatorNet45Tests
    {
        private readonly JsonContentTranslator _translator;

        public JsonContentTranslatorNet45Tests()
        {
            _translator = new JsonContentTranslator();
        }

        [Fact]
        public void ContentType_ForNet45_RemainsJson()
        {
            var result = _translator.ContentType;
            Assert.Equal(HttpContentType.Json, result);
        }

        [Fact]
        public void Parse_OnNet45_UsesNewtonsoftAndIgnoresQueryAndPathAttributes()
        {
            var content = new TestClassWithAttributes
            {
                Name = "John",
                QueryParam = "ShouldBeIgnored",
                PathParam = "AlsoIgnored",
                RegularProperty = "ShouldBeIncluded",
            };

            HttpContent httpContent = _translator.Parse(content);

            Assert.IsType<StringContent>(httpContent);
            var json = httpContent.ReadAsStringAsync().Result;

            // Newtonsoft on full framework should serialize with PascalCase names,
            // and the custom contract resolver must ignore QueryArg/PathOnly properties.
            Assert.Contains("\"Name\":\"John\"", json);
            Assert.Contains("\"RegularProperty\":\"ShouldBeIncluded\"", json);
            Assert.DoesNotContain("QueryParam", json);
            Assert.DoesNotContain("PathParam", json);
        }

        [Fact]
        public void Deserialize_OnNet45_WithEmptyString_ThrowsJsonReaderException()
        {
            var json = string.Empty;

            // This asserts specifically the Newtonsoft.Json behavior used on net45.
            Assert.Throws<JsonReaderException>(() => _translator.Deserialize<TestPerson>(json));
        }

        private sealed class TestPerson
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }

        private sealed class TestClassWithAttributes
        {
            public string Name { get; set; }

            [JanusRequest.Attributes.QueryArg]
            public string QueryParam { get; set; }

            [JanusRequest.Attributes.PathOnly]
            public string PathParam { get; set; }

            public string RegularProperty { get; set; }
        }
    }
}

