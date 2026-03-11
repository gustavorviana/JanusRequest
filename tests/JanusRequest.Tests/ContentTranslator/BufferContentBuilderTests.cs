using JanusRequest.ContentTranslator;
using NSubstitute;

namespace JanusRequest.Tests.ContentTranslator
{
    public class BufferContentBuilderTests
    {
        private readonly BufferContentBuilder _bufferContentBuilder;

        public BufferContentBuilderTests()
        {
            _bufferContentBuilder = new BufferContentBuilder();
        }

        [Fact]
        public void CanWork_WhenTypeIsBuffer_ReturnsTrue()
        {
            // Arrange
            var bufferType = typeof(byte[]);

            // Act
            var result = _bufferContentBuilder.CanWork(bufferType);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanWork_WhenTypeIsStream_ReturnsTrue()
        {
            // Arrange
            var streamType = typeof(Stream);

            // Act
            var result = _bufferContentBuilder.CanWork(streamType);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ToHttpContent_WhenValueIsStream_ReturnsStreamContent()
        {
            // Arrange
            var stream = new MemoryStream([1, 2, 3, 4, 5]);

            // Act
            var result = _bufferContentBuilder.ToHttpContent(stream);

            // Assert
            Assert.IsType<StreamContent>(result);
        }

        [Fact]
        public void ToHttpContent_WhenValueIsSeekableStream_SetsContentLength()
        {
            // Arrange
            var data = new byte[] { 1, 2, 3, 4, 5 };
            var stream = new MemoryStream(data);

            // Act
            var result = _bufferContentBuilder.ToHttpContent(stream);

            // Assert
            Assert.Equal(data.Length, result.Headers.ContentLength);
        }

        [Fact]
        public void ToHttpContent_WhenValueIsNonSeekableStream_DoesNotSetContentLength()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(false);

            // Act
            var result = _bufferContentBuilder.ToHttpContent(mockStream);

            // Assert
            Assert.Null(result.Headers.ContentLength);
        }

        [Fact]
        public void ToHttpContent_WhenValueIsByteArray_ReturnsByteArrayContent()
        {
            // Arrange
            var byteArray = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = _bufferContentBuilder.ToHttpContent(byteArray);

            // Assert
            Assert.IsType<ByteArrayContent>(result);
        }

        [Fact]
        public void TryGetStreamSize_WhenStreamCanSeek_ReturnsTrueWithCorrectSize()
        {
            // Arrange
            var data = new byte[] { 1, 2, 3, 4, 5 };
            var stream = new MemoryStream(data);

            // Act
            var result = _bufferContentBuilder.TryGetStreamSize(stream, out var size);

            // Assert
            Assert.True(result);
            Assert.Equal(data.Length, size);
        }

        [Fact]
        public void TryGetStreamSize_WhenStreamCannotSeek_ReturnsFalse()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(false);

            // Act
            var result = _bufferContentBuilder.TryGetStreamSize(mockStream, out var size);

            // Assert
            Assert.False(result);
            Assert.Equal(0, size);
        }

        [Fact]
        public void TryGetStreamSize_WhenStreamThrowsNotSupportedException_ReturnsFalse()
        {
            // Arrange
            var mockStream = Substitute.For<Stream>();
            mockStream.CanSeek.Returns(true);
            mockStream.When(x => { var _ = x.Length; }).Do(x => throw new NotSupportedException());

            // Act
            var result = _bufferContentBuilder.TryGetStreamSize(mockStream, out var size);

            // Assert
            Assert.False(result);
            Assert.Equal(0, size);
        }
    }
}