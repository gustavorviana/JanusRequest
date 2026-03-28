namespace JanusRequest.Tests
{
    public class MediaTypeMapTests
    {
        [Fact]
        public void IDictionaryAdd_NormalizesKey_RetrievableByNormalizedKey()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            var dict = (IDictionary<string, string>)map;

            // Act - add with non-normalized key (uppercase)
            dict.Add("APPLICATION/JSON", "json-translator");

            // Assert - retrievable by lowercase
            Assert.True(map.TryGetValue("application/json", out var value));
            Assert.Equal("json-translator", value);
        }

        [Fact]
        public void IDictionaryAdd_WithMediaTypeParameters_NormalizesKey()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            var dict = (IDictionary<string, string>)map;

            // Act - add with charset parameter (should be stripped by normalizer)
            dict.Add("Application/Json; charset=utf-8", "json-translator");

            // Assert
            Assert.True(map.TryGetValue("application/json", out var value));
            Assert.Equal("json-translator", value);
        }

        [Fact]
        public void IDictionaryAdd_DuplicateKey_ThrowsArgumentException()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            var dict = (IDictionary<string, string>)map;
            dict.Add("application/json", "first");

            // Act & Assert
            Assert.Throws<ArgumentException>(() => dict.Add("application/json", "second"));
        }

        [Fact]
        public void ICollectionAdd_NormalizesKey_RetrievableByNormalizedKey()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            var collection = (ICollection<KeyValuePair<string, string>>)map;

            // Act
            collection.Add(new KeyValuePair<string, string>("TEXT/PLAIN", "text-translator"));

            // Assert
            Assert.True(map.TryGetValue("text/plain", out var value));
            Assert.Equal("text-translator", value);
        }

        [Fact]
        public void CopyTo_CopiesAllEntries()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            map.Set("application/json", "json");
            map.Set("text/xml", "xml");

            var collection = (ICollection<KeyValuePair<string, string>>)map;
            var array = new KeyValuePair<string, string>[2];

            // Act
            collection.CopyTo(array, 0);

            // Assert
            Assert.Equal(2, array.Count(x => x.Key != null));
            Assert.Contains(array, x => x.Key == "application/json" && x.Value == "json");
            Assert.Contains(array, x => x.Key == "text/xml" && x.Value == "xml");
        }

        [Fact]
        public void CopyTo_WithOffset_CopiesAtCorrectPosition()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            map.Set("application/json", "json");

            var collection = (ICollection<KeyValuePair<string, string>>)map;
            var array = new KeyValuePair<string, string>[3];

            // Act
            collection.CopyTo(array, 1);

            // Assert
            Assert.Equal(default, array[0]);
            Assert.Equal("application/json", array[1].Key);
            Assert.Equal("json", array[1].Value);
        }

        [Fact]
        public void CopyTo_EmptyMap_DoesNotThrow()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            var collection = (ICollection<KeyValuePair<string, string>>)map;
            var array = new KeyValuePair<string, string>[0];

            // Act & Assert - should not throw
            collection.CopyTo(array, 0);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            map.Set("application/json", "json");

            // Act & Assert - should not throw
            map.Dispose();
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var map = new MediaTypeMap<string>();

            // Act & Assert - double dispose should not throw
            map.Dispose();
            map.Dispose();
        }

        [Fact]
        public void Set_And_TryGetValue_WorkCorrectly()
        {
            // Arrange
            var map = new MediaTypeMap<string>();

            // Act
            map.Set("application/json", "json-value");

            // Assert
            Assert.True(map.TryGetValue("application/json", out var value));
            Assert.Equal("json-value", value);
        }

        [Fact]
        public void Set_CaseInsensitive_TryGetValueFindsIt()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            map.Set("application/json", "json-value");

            // Act & Assert
            Assert.True(map.TryGetValue("APPLICATION/JSON", out var value));
            Assert.Equal("json-value", value);
        }

        [Fact]
        public void Remove_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            map.Set("application/json", "json");

            // Act
            var removed = map.Remove("application/json");

            // Assert
            Assert.True(removed);
            Assert.False(map.TryGetValue("application/json", out _));
        }

        [Fact]
        public void Clear_RemovesAllEntries()
        {
            // Arrange
            var map = new MediaTypeMap<string>();
            map.Set("application/json", "json");
            map.Set("text/xml", "xml");

            // Act
            map.Clear();

            // Assert
            Assert.Equal(0, map.Count);
        }
    }
}
