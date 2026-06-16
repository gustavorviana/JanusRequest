using JanusRequest.Json.Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JanusRequest.Json.Newtonsoft.Tests
{
    public class ProblemDetailsNewtonsoftJsonConverterTests
    {
        private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Converters = { new ProblemDetailsNewtonsoftJsonConverter() }
        };

        #region Deserialization - Standard fields

        [Fact]
        public void Read_StandardFields_MapsAllProperties()
        {
            var json = @"{
                ""type"": ""https://example.com/not-found"",
                ""title"": ""Not Found"",
                ""status"": 404,
                ""detail"": ""The item was not found."",
                ""instance"": ""/items/42""
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Equal("https://example.com/not-found", result.Type);
            Assert.Equal("Not Found", result.Title);
            Assert.Equal(404, result.Status);
            Assert.Equal("The item was not found.", result.Detail);
            Assert.Equal("/items/42", result.Instance);
            Assert.Null(result.Extensions);
        }

        [Fact]
        public void Read_OnlyRequiredFields_DefaultsOthersToNull()
        {
            var json = @"{ ""type"": ""about:blank"", ""title"": ""Error"", ""status"": 500 }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Equal("about:blank", result.Type);
            Assert.Equal("Error", result.Title);
            Assert.Equal(500, result.Status);
            Assert.Null(result.Detail);
            Assert.Null(result.Instance);
            Assert.Null(result.Extensions);
        }

        #endregion

        #region Deserialization - Extensions with scalar values

        [Fact]
        public void Read_ExtraStringField_GoesToExtensions()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Error"",
                ""status"": 400,
                ""traceId"": ""abc-123""
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.NotNull(result.Extensions);
            Assert.Single(result.Extensions);
            Assert.Equal("abc-123", result.Extensions["traceId"].Value);
        }

        [Fact]
        public void Read_ExtraNumericField_GoesToExtensions()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Error"",
                ""status"": 400,
                ""retryAfter"": 30
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Equal(30L, result.Extensions["retryAfter"].Value);
        }

        [Fact]
        public void Read_ExtraBooleanField_GoesToExtensions()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Error"",
                ""status"": 400,
                ""retriable"": true
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Equal(true, result.Extensions["retriable"].Value);
        }

        [Fact]
        public void Read_ExtraNullField_GoesToExtensions()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Error"",
                ""status"": 400,
                ""extra"": null
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.NotNull(result.Extensions);
            Assert.Null(result.Extensions["extra"].Value);
        }

        #endregion

        #region Deserialization - Extensions with nested objects

        [Fact]
        public void Read_NestedObject_MapsToChildrenNodes()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Validation Error"",
                ""status"": 422,
                ""errors"": {
                    ""name"": ""Name is required"",
                    ""email"": ""Invalid email""
                }
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            var errors = result.Extensions["errors"];
            Assert.True(errors.HasChildren);
            Assert.Equal("Name is required", errors.Children["name"].Value);
            Assert.Equal("Invalid email", errors.Children["email"].Value);
        }

        [Fact]
        public void Read_DeeplyNestedObject_MapsRecursively()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Error"",
                ""status"": 400,
                ""context"": {
                    ""user"": {
                        ""id"": 42,
                        ""role"": ""admin""
                    }
                }
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            var user = result.Extensions["context"].Children["user"];
            Assert.Equal(42L, user.Children["id"].Value);
            Assert.Equal("admin", user.Children["role"].Value);
        }

        #endregion

        #region Deserialization - Extensions with arrays

        [Fact]
        public void Read_ArrayExtension_MapsToIndexedChildren()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Error"",
                ""status"": 400,
                ""items"": [1, 2, 3, 4]
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            var items = result.Extensions["items"];
            Assert.True(items.HasChildren);
            Assert.Equal(4, items.Children.Count);
            Assert.Equal(1L, items.Children["0"].Value);
            Assert.Equal(2L, items.Children["1"].Value);
            Assert.Equal(3L, items.Children["2"].Value);
            Assert.Equal(4L, items.Children["3"].Value);
        }

        [Fact]
        public void Read_ArrayOfObjects_MapsToIndexedChildrenWithNestedNodes()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Validation Error"",
                ""status"": 422,
                ""errors"": [
                    { ""field"": ""name"", ""message"": ""required"" },
                    { ""field"": ""email"", ""message"": ""invalid"" }
                ]
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            var errors = result.Extensions["errors"];
            Assert.Equal(2, errors.Children.Count);

            var first = errors.Children["0"];
            Assert.Equal("name", first.Children["field"].Value);
            Assert.Equal("required", first.Children["message"].Value);

            var second = errors.Children["1"];
            Assert.Equal("email", second.Children["field"].Value);
            Assert.Equal("invalid", second.Children["message"].Value);
        }

        [Fact]
        public void Read_MixedArray_MapsToIndexedChildren()
        {
            var json = @"{
                ""type"": ""about:blank"",
                ""title"": ""Error"",
                ""status"": 400,
                ""data"": [""text"", 42, true, null]
            }";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            var data = result.Extensions["data"];
            Assert.Equal("text", data.Children["0"].Value);
            Assert.Equal(42L, data.Children["1"].Value);
            Assert.Equal(true, data.Children["2"].Value);
            Assert.Null(data.Children["3"].Value);
        }

        #endregion

        #region Deserialization - Root array

        [Fact]
        public void Read_RootArray_MapsToIndexedExtensions()
        {
            var json = @"[1, 2, 3, 4]";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Null(result.Type);
            Assert.Null(result.Title);
            Assert.Equal(0, result.Status);
            Assert.Equal(4, result.Extensions.Count);
            Assert.Equal(1L, result.Extensions["0"].Value);
            Assert.Equal(2L, result.Extensions["1"].Value);
            Assert.Equal(3L, result.Extensions["2"].Value);
            Assert.Equal(4L, result.Extensions["3"].Value);
        }

        [Fact]
        public void Read_RootArrayOfObjects_MapsToIndexedExtensions()
        {
            var json = @"[
                { ""field"": ""name"", ""error"": ""required"" },
                { ""field"": ""email"", ""error"": ""invalid"" }
            ]";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Equal(2, result.Extensions.Count);

            var first = result.Extensions["0"];
            Assert.Equal("name", first.Children["field"].Value);
            Assert.Equal("required", first.Children["error"].Value);

            var second = result.Extensions["1"];
            Assert.Equal("email", second.Children["field"].Value);
            Assert.Equal("invalid", second.Children["error"].Value);
        }

        #endregion

        #region Deserialization - Root scalar

        [Fact]
        public void Read_RootString_MapsToExtensionZero()
        {
            var json = @"""some error message""";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Single(result.Extensions);
            Assert.Equal("some error message", result.Extensions["0"].Value);
        }

        [Fact]
        public void Read_RootNumber_MapsToExtensionZero()
        {
            var json = @"42";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Single(result.Extensions);
            Assert.Equal(42L, result.Extensions["0"].Value);
        }

        [Fact]
        public void Read_RootBoolean_MapsToExtensionZero()
        {
            var json = @"true";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Single(result.Extensions);
            Assert.Equal(true, result.Extensions["0"].Value);
        }

        [Fact]
        public void Read_RootNull_ReturnsNull()
        {
            var json = @"null";

            var result = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);

            Assert.Null(result);
        }

        #endregion

        #region Serialization

        [Fact]
        public void Write_StandardFields_ProducesCorrectJson()
        {
            var problem = new ProblemDetails(
                "https://example.com/error",
                "Error",
                500,
                "Something went wrong",
                "/requests/1"
            );

            var json = JsonConvert.SerializeObject(problem, Settings);
            var obj = JObject.Parse(json);

            Assert.Equal("https://example.com/error", (string)obj["type"]);
            Assert.Equal("Error", (string)obj["title"]);
            Assert.Equal(500, (int)obj["status"]);
            Assert.Equal("Something went wrong", (string)obj["detail"]);
            Assert.Equal("/requests/1", (string)obj["instance"]);
        }

        [Fact]
        public void Write_WithExtensions_IncludesExtensionFields()
        {
            var extensions = new Dictionary<string, ProblemExtensionNode>
            {
                ["traceId"] = new ProblemExtensionNode("abc-123"),
                ["retryAfter"] = new ProblemExtensionNode(30L)
            };

            var problem = new ProblemDetails("about:blank", "Error", 400, extensions: extensions);

            var json = JsonConvert.SerializeObject(problem, Settings);
            var obj = JObject.Parse(json);

            Assert.Equal("abc-123", (string)obj["traceId"]);
            Assert.Equal(30L, (long)obj["retryAfter"]);
        }

        [Fact]
        public void Write_WithNestedObjectExtension_ProducesNestedJson()
        {
            var children = new Dictionary<string, ProblemExtensionNode>
            {
                ["name"] = new ProblemExtensionNode("required"),
                ["email"] = new ProblemExtensionNode("invalid")
            };

            var extensions = new Dictionary<string, ProblemExtensionNode>
            {
                ["errors"] = new ProblemExtensionNode(children)
            };

            var problem = new ProblemDetails("about:blank", "Error", 422, extensions: extensions);

            var json = JsonConvert.SerializeObject(problem, Settings);
            var obj = JObject.Parse(json);
            var errors = (JObject)obj["errors"];

            Assert.Equal("required", (string)errors["name"]);
            Assert.Equal("invalid", (string)errors["email"]);
        }

        [Fact]
        public void Write_WithArrayExtension_ProducesJsonArray()
        {
            var items = new Dictionary<string, ProblemExtensionNode>
            {
                ["0"] = new ProblemExtensionNode(1L),
                ["1"] = new ProblemExtensionNode(2L),
                ["2"] = new ProblemExtensionNode(3L)
            };

            var extensions = new Dictionary<string, ProblemExtensionNode>
            {
                ["items"] = new ProblemExtensionNode(items)
            };

            var problem = new ProblemDetails("about:blank", "Error", 400, extensions: extensions);

            var json = JsonConvert.SerializeObject(problem, Settings);
            var obj = JObject.Parse(json);
            var arr = (JArray)obj["items"];

            Assert.Equal(3, arr.Count);
            Assert.Equal(1L, (long)arr[0]);
            Assert.Equal(2L, (long)arr[1]);
            Assert.Equal(3L, (long)arr[2]);
        }

        [Fact]
        public void Write_NullExtensionValue_ProducesJsonNull()
        {
            var extensions = new Dictionary<string, ProblemExtensionNode>
            {
                ["extra"] = new ProblemExtensionNode((object)null)
            };

            var problem = new ProblemDetails("about:blank", "Error", 400, extensions: extensions);

            var json = JsonConvert.SerializeObject(problem, Settings);
            var obj = JObject.Parse(json);

            Assert.Equal(JTokenType.Null, obj["extra"].Type);
        }

        #endregion

        #region Roundtrip

        [Fact]
        public void Roundtrip_FullProblemDetails_PreservesAllData()
        {
            var json = @"{
                ""type"": ""https://example.com/validation"",
                ""title"": ""Validation Error"",
                ""status"": 422,
                ""detail"": ""One or more fields are invalid."",
                ""instance"": ""/users/register"",
                ""traceId"": ""trace-xyz"",
                ""errors"": [
                    { ""field"": ""name"", ""message"": ""required"" }
                ]
            }";

            var deserialized = JsonConvert.DeserializeObject<ProblemDetails>(json, Settings);
            var serialized = JsonConvert.SerializeObject(deserialized, Settings);
            var roundtrip = JsonConvert.DeserializeObject<ProblemDetails>(serialized, Settings);

            Assert.Equal(deserialized.Type, roundtrip.Type);
            Assert.Equal(deserialized.Title, roundtrip.Title);
            Assert.Equal(deserialized.Status, roundtrip.Status);
            Assert.Equal(deserialized.Detail, roundtrip.Detail);
            Assert.Equal(deserialized.Instance, roundtrip.Instance);
            Assert.Equal("trace-xyz", roundtrip.Extensions["traceId"].Value);

            var error = roundtrip.Extensions["errors"].Children["0"];
            Assert.Equal("name", error.Children["field"].Value);
            Assert.Equal("required", error.Children["message"].Value);
        }

        #endregion
    }
}
