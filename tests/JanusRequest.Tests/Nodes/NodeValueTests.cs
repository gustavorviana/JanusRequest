using JanusRequest.Nodes;

namespace JanusRequest.Tests.Nodes
{
    public class NodeValueTests
    {
        [Fact]
        public void Constructor_SetsAllProperties()
        {
            var member = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            var type = typeof(string);
            var pathName = "Parent.Name";
            var value = (object)"TestValue";

            var nodeValue = new NodeValue(member, type, pathName, value);

            Assert.Same(member, nodeValue.Member);
            Assert.Equal(type, nodeValue.Type);
            Assert.Equal(pathName, nodeValue.PathName);
            Assert.Equal(value, nodeValue.Value);
        }

        [Fact]
        public void Constructor_WithNullValue_SetsNull()
        {
            var member = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;

            var nodeValue = new NodeValue(member, typeof(string), "Name", null);

            Assert.Null(nodeValue.Value);
        }

        [Fact]
        public void ToString_ReturnsPathName()
        {
            var member = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;

            var nodeValue = new NodeValue(member, typeof(string), "Parent.Name", "TestValue");

            Assert.Equal("Parent.Name", nodeValue.ToString());
        }

        [Fact]
        public void ToString_WithSimplePath_ReturnsPath()
        {
            var member = typeof(SampleModel).GetProperty(nameof(SampleModel.Age))!;

            var nodeValue = new NodeValue(member, typeof(int), "Age", 42);

            Assert.Equal("Age", nodeValue.ToString());
        }

        private class SampleModel
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
