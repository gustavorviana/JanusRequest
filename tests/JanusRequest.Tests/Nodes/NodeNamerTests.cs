using JanusRequest.Nodes;

namespace JanusRequest.Tests.Nodes
{
    public class NodeNamerTests
    {
        private readonly NodeNamer _namer = new NodeNamer();

        [Fact]
        public void GetName_Initially_ReturnsEmptyString()
        {
            Assert.Equal(string.Empty, _namer.GetName());
        }

        [Fact]
        public void OnEnter_OnLeave_PushesAndPopsPath()
        {
            var property = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;

            _namer.OnEnter(property);
            Assert.Equal("Name", _namer.GetName());

            _namer.OnLeave();
            Assert.Equal(string.Empty, _namer.GetName());
        }

        [Fact]
        public void OnEnter_MultipleLevels_ProducesDottedPath()
        {
            var nameProperty = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            var ageProperty = typeof(SampleModel).GetProperty(nameof(SampleModel.Age))!;

            _namer.OnEnter(nameProperty);
            _namer.OnEnter(ageProperty);

            Assert.Equal("Name.Age", _namer.GetName());
        }

        [Fact]
        public void GetFullName_WithEmptyPath_ReturnsJustName()
        {
            Assert.Equal("MyField", _namer.GetFullName("MyField"));
        }

        [Fact]
        public void GetFullName_WithPath_ReturnsDottedPath()
        {
            var property = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            _namer.OnEnter(property);

            Assert.Equal("Name.MyField", _namer.GetFullName("MyField"));
        }

        [Fact]
        public void CanEnter_ReturnsTrue_ByDefault()
        {
            var property = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            Assert.True(_namer.CanEnter(property));
        }

        [Fact]
        public void CanMap_ReturnsTrue_ByDefault()
        {
            var property = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            Assert.True(_namer.CanMap(property));
        }

        [Fact]
        public void GetName_WithMemberInfo_ReturnsMemberName()
        {
            var property = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            Assert.Equal("Name", _namer.GetName(property));
        }

        [Fact]
        public void ToString_ReturnsSameAsGetName()
        {
            var property = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            _namer.OnEnter(property);

            Assert.Equal(_namer.GetName(), _namer.ToString());
        }

        [Fact]
        public void MultipleEnterLeave_Cycles_ProduceCorrectPaths()
        {
            var nameProperty = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            var ageProperty = typeof(SampleModel).GetProperty(nameof(SampleModel.Age))!;

            _namer.OnEnter(nameProperty);
            Assert.Equal("Name", _namer.GetName());
            _namer.OnLeave();

            _namer.OnEnter(ageProperty);
            Assert.Equal("Age", _namer.GetName());
            _namer.OnLeave();

            Assert.Equal(string.Empty, _namer.GetName());
        }

        [Fact]
        public void NestedEnterLeave_UnwindsCorrectly()
        {
            var nameProperty = typeof(SampleModel).GetProperty(nameof(SampleModel.Name))!;
            var ageProperty = typeof(SampleModel).GetProperty(nameof(SampleModel.Age))!;

            _namer.OnEnter(nameProperty);
            _namer.OnEnter(ageProperty);
            Assert.Equal("Name.Age", _namer.GetName());

            _namer.OnLeave();
            Assert.Equal("Name", _namer.GetName());

            _namer.OnLeave();
            Assert.Equal(string.Empty, _namer.GetName());
        }

        private class SampleModel
        {
            public string Name { get; set; } = "Test";
            public int Age { get; set; } = 25;
        }
    }
}
