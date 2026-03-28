using System.IO;

namespace JanusRequest.Tests
{
    public class ReflectionUtilsTests
    {
        #region IsNative

        [Fact]
        public void IsNative_WithNullType_ReturnsTrue()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(null, ignoreBuffer: false);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        public void IsNative_WithPrimitiveType_ReturnsTrue(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(type, ignoreBuffer: false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNative_WithEnumType_ReturnsTrue()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(typeof(SampleEnum), ignoreBuffer: false);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(Guid))]
        [InlineData(typeof(TimeSpan))]
        [InlineData(typeof(DateTimeOffset))]
        public void IsNative_WithKnownNativeType_ReturnsTrue(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(type, ignoreBuffer: false);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(int?))]
        [InlineData(typeof(bool?))]
        [InlineData(typeof(DateTime?))]
        [InlineData(typeof(Guid?))]
        [InlineData(typeof(double?))]
        [InlineData(typeof(decimal?))]
        public void IsNative_WithNullableOfNativeType_ReturnsTrue(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(type, ignoreBuffer: false);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        public void IsNative_WithNumericDecimalType_ReturnsTrue(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(type, ignoreBuffer: false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNative_WithByteArray_AndIgnoreBufferFalse_ReturnsTrue()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(typeof(byte[]), ignoreBuffer: false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNative_WithStream_AndIgnoreBufferFalse_ReturnsTrue()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(typeof(Stream), ignoreBuffer: false);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNative_WithByteArray_AndIgnoreBufferTrue_ReturnsFalse()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(typeof(byte[]), ignoreBuffer: true);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNative_WithStream_AndIgnoreBufferTrue_ReturnsFalse()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(typeof(Stream), ignoreBuffer: true);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(List<string>))]
        [InlineData(typeof(Dictionary<string, int>))]
        public void IsNative_WithComplexType_ReturnsFalse(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNative(type, ignoreBuffer: false);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsBuffer

        [Fact]
        public void IsBuffer_WithByteArray_ReturnsTrue()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsBuffer(typeof(byte[]));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsBuffer_WithStream_ReturnsTrue()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsBuffer(typeof(Stream));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsBuffer_WithMemoryStream_ReturnsTrue()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsBuffer(typeof(MemoryStream));

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsBuffer_WithFileStream_ReturnsTrue()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsBuffer(typeof(FileStream));

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(int[]))]
        public void IsBuffer_WithNonBufferType_ReturnsFalse(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsBuffer(type);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsNumeric

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        public void IsNumeric_WithNumericType_ReturnsTrue(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumeric(type);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(object))]
        [InlineData(typeof(char))]
        public void IsNumeric_WithNonNumericType_ReturnsFalse(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumeric(type);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsNumberWithDecimal

        [Theory]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        public void IsNumberWithDecimal_WithDecimalType_ReturnsTrue(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumberWithDecimal(type);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        [InlineData(typeof(string))]
        [InlineData(typeof(bool))]
        public void IsNumberWithDecimal_WithNonDecimalType_ReturnsFalse(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumberWithDecimal(type);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsNumberWithoutDecimal

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(long))]
        [InlineData(typeof(byte))]
        [InlineData(typeof(sbyte))]
        [InlineData(typeof(short))]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        public void IsNumberWithoutDecimal_WithIntegralType_ReturnsTrue(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumberWithoutDecimal(type);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(typeof(decimal))]
        [InlineData(typeof(float))]
        [InlineData(typeof(double))]
        [InlineData(typeof(string))]
        [InlineData(typeof(bool))]
        [InlineData(typeof(char))]
        public void IsNumberWithoutDecimal_WithNonIntegralType_ReturnsFalse(Type type)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumberWithoutDecimal(type);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsNumericString

        [Theory]
        [InlineData("123")]
        [InlineData("0")]
        [InlineData("999999")]
        public void IsNumericString_WithIntegerString_ReturnsTrue(string value)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(value);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("1.23")]
        [InlineData("0.5")]
        [InlineData("100.0")]
        public void IsNumericString_WithDotDecimalString_ReturnsTrue(string value)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(value);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("1,23")]
        [InlineData("0,5")]
        [InlineData("100,0")]
        public void IsNumericString_WithCommaDecimalString_ReturnsTrue(string value)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(value);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsNumericString_WithNullValue_ReturnsFalse()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(null);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsNumericString_WithEmptyString_ReturnsFalse()
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(".5")]
        [InlineData(",5")]
        public void IsNumericString_WithLeadingDecimalSeparator_ReturnsFalse(string value)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(value);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("5.")]
        [InlineData("5,")]
        public void IsNumericString_WithTrailingDecimalSeparator_ReturnsFalse(string value)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(value);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("1.2.3")]
        [InlineData("1,2,3")]
        [InlineData("1.2,3")]
        public void IsNumericString_WithMultipleDecimalSeparators_ReturnsFalse(string value)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(value);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData("12a3")]
        [InlineData("abc")]
        [InlineData("1 2")]
        [InlineData("-1")]
        [InlineData("+1")]
        [InlineData("1e5")]
        public void IsNumericString_WithNonNumericCharacters_ReturnsFalse(string value)
        {
            // Arrange & Act
            var result = ReflectionUtils.IsNumericString(value);

            // Assert
            Assert.False(result);
        }

        #endregion

        private enum SampleEnum
        {
            ValueA,
            ValueB
        }
    }
}
