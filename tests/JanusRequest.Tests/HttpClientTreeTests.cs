using JanusRequest.Nodes;

namespace JanusRequest.Tests
{
    public class HttpClientTreeTests
    {
        [Fact]
        public void GetValueByPath_ShouldReturnSimpleProperty()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var value = tree.GetValue(person, "Name");

            Assert.Equal("John Doe", value);
        }

        [Fact]
        public void GetValueByPath_ShouldReturnNestedProperty()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var value = tree.GetValue(person, "Address.Street");

            Assert.Equal("Main St", value);
        }

        [Fact]
        public void GetValueByPath_ShouldCallMethod()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var value = tree.GetValue(person, "GetGreeting()");

            Assert.Equal("Hello, John Doe", value);
        }

        [Fact]
        public void GetValueByPath_ShouldThrow_WhenMultipleMethodsInPath()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(person, "GetGreeting().GetGreeting()"));

            Assert.Contains("Multiple method calls", ex.Message);
        }

        [Fact]
        public void GetValueByPath_ShouldThrow_WhenPropertyNotFound()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(person, "InvalidProp"));

            Assert.Equal("Property \"InvalidProp\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void GetValueByPath_ShouldThrow_WhenMethodNotFound()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(person, "InvalidMethod()"));

            Assert.Equal("Method \"InvalidMethod\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void GetValueByPath_ShouldThrow_WhenPathIsNull()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            Assert.Throws<ArgumentNullException>(() => tree.GetValue(person, null));
        }

        [Fact]
        public void GetValueByPath_ShouldThrow_WhenPathIsEmpty()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            Assert.Throws<ArgumentNullException>(() => tree.GetValue(person, string.Empty));
        }

        [Fact]
        public void GetValueByPath_ShouldIgnore_GenericMethod_Echo()
        {
            var obj = new WithGenerics();
            var tree = new HttpClientTree(typeof(WithGenerics));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(obj, "Echo()"));

            Assert.Equal("Method \"Echo\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void GetValueByPath_ShouldIgnore_GenericMethod_CreateInstance()
        {
            var obj = new WithGenerics();
            var tree = new HttpClientTree(typeof(WithGenerics));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(obj, "CreateInstance()"));

            Assert.Equal("Method \"CreateInstance\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void GetValueByPath_ShouldIgnore_MethodWithSingleParameter()
        {
            var obj = new WithParams();
            var tree = new HttpClientTree(typeof(WithParams));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(obj, "Format()"));

            Assert.Equal("Method \"Format\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void GetValueByPath_ShouldIgnore_MethodWithMultipleParameters()
        {
            var obj = new WithParams();
            var tree = new HttpClientTree(typeof(WithParams));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(obj, "Combine()"));

            Assert.Equal("Method \"Combine\" not found in path. Invalid segment at position 0.", ex.Message);
        }
        [Fact]
        public void GetValueByPath_ShouldIgnore_VoidMethods()
        {
            var obj = new WithVoidMethods();
            var tree = new HttpClientTree(typeof(WithVoidMethods));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(obj, "DoSomething()"));

            Assert.Equal("Method \"DoSomething\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void GetValueByPath_ShouldIgnore_PropertyGetters()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var ex = Assert.Throws<ArgumentException>(() => tree.GetValue(person, "()"));

            Assert.Equal("Method \"\" not found in path. Invalid segment at position 0.", ex.Message);
        }

        [Fact]
        public void GetValueByPath_ShouldHandleNullIntermediateValues()
        {
            var person = new Person { Address = null };
            var tree = new HttpClientTree(typeof(Person));

            Assert.Null(tree.GetValue(person, "Address.Street"));
        }

        [Fact]
        public void GetValueByPath_ShouldReturnNullForNullableProperties()
        {
            var obj = new WithNullableProperty { NullableValue = null };
            var tree = new HttpClientTree(typeof(WithNullableProperty));

            var value = tree.GetValue(obj, "NullableValue");

            Assert.Null(value);
        }

        [Fact]
        public void GetValueByPath_ShouldReturnValueForNullableProperties()
        {
            var obj = new WithNullableProperty { NullableValue = 42 };
            var tree = new HttpClientTree(typeof(WithNullableProperty));

            var value = tree.GetValue(obj, "NullableValue");

            Assert.Equal(42, value);
        }

        [Fact]
        public void GetValueByPath_ShouldHandleDeeplyNestedPaths()
        {
            var company = new Company();
            var tree = new HttpClientTree(typeof(Company));

            var value = tree.GetValue(company, "Owner.Address.Street");

            Assert.Equal("Main St", value);
        }

        [Fact]
        public void GetValueByPath_ShouldBeCaseInsensitive()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var value = tree.GetValue(person, "Name");

            Assert.Equal("John Doe", value);
        }

        [Fact]
        public void GetValueByPath_ShouldHandlePrivateMethods()
        {
            var obj = new WithPrivateMethod();
            var tree = new HttpClientTree(typeof(WithPrivateMethod));

            var value = tree.GetValue(obj, "GetSecret()");

            Assert.Equal("Secret Value", value);
        }

        [Fact]
        public void MethodNode_ShouldNotSupportMemberValueType()
        {
            var person = new Person();
            IGetNode tree = new HttpClientTree(typeof(Person));
            var methodNode = tree.GetNode("GetGreeting()");

            Assert.Throws<NotSupportedException>(() => _ = methodNode.MemberValueType);
        }

        [Fact]
        public void MethodNode_ShouldNotSupportGetNode()
        {
            var person = new Person();
            IGetNode tree = new HttpClientTree(typeof(Person));
            var methodNode = tree.GetNode("GetGreeting()");

            Assert.Throws<NotSupportedException>(() => methodNode.GetNode("anything"));
        }

        [Fact]
        public void MethodNode_ShouldNotSupportAddMember()
        {
            var person = new Person();
            IGetNode tree = new HttpClientTree(typeof(Person));
            var methodNode = tree.GetNode("GetGreeting()");
            var propertyInfo = typeof(Person).GetProperty("Name");

            Assert.Throws<NotSupportedException>(() => methodNode.Add(propertyInfo));
        }

        #region GetAllValues Tests

        [Fact]
        public void GetAllValues_ReturnsAllLeafPropertyValues()
        {
            var person = new Person();
            var tree = new HttpClientTree(typeof(Person));

            var values = tree.GetAllValues(person).ToList();

            Assert.Contains(values, v => v.PathName == "Name" && (string)v.Value == "John Doe");
            Assert.Contains(values, v => v.PathName == "Age" && (int)v.Value == 30);
            Assert.Contains(values, v => v.PathName == "Address.Street" && (string)v.Value == "Main St");
        }

        [Fact]
        public void GetAllValues_WithNestedObject_ProducesDottedPaths()
        {
            var company = new Company();
            var tree = new HttpClientTree(typeof(Company));

            var values = tree.GetAllValues(company).ToList();

            Assert.Contains(values, v => v.PathName == "Owner.Name");
            Assert.Contains(values, v => v.PathName == "Owner.Address.Street");
        }

        #endregion

        #region IsValidMethod Tests

        [Fact]
        public void IsValidMethod_ParameterlessNonVoid_ReturnsTrue()
        {
            var method = typeof(Person).GetMethod("GetGreeting")!;
            Assert.True(HttpClientTree.IsValidMethod(method));
        }

        [Fact]
        public void IsValidMethod_VoidMethod_ReturnsFalse()
        {
            var method = typeof(WithVoidMethods).GetMethod("DoSomething")!;
            Assert.False(HttpClientTree.IsValidMethod(method));
        }

        [Fact]
        public void IsValidMethod_MethodWithParameters_ReturnsFalse()
        {
            var method = typeof(WithParams).GetMethod("Format")!;
            Assert.False(HttpClientTree.IsValidMethod(method));
        }

        [Fact]
        public void IsValidMethod_GenericMethod_ReturnsFalse()
        {
            var method = typeof(WithGenerics).GetMethod("Echo")!;
            Assert.False(HttpClientTree.IsValidMethod(method));
        }

        [Fact]
        public void IsValidMethod_PropertyGetter_ReturnsFalse()
        {
            var method = typeof(Person).GetMethod("get_Name")!;
            Assert.False(HttpClientTree.IsValidMethod(method));
        }

        [Fact]
        public void IsValidMethod_ObjectToString_IsValid()
        {
            var method = typeof(object).GetMethod("ToString")!;
            Assert.True(HttpClientTree.IsValidMethod(method));
        }

        [Fact]
        public void IsValidMethod_ObjectGetHashCode_ReturnsFalse()
        {
            var method = typeof(object).GetMethod("GetHashCode")!;
            Assert.False(HttpClientTree.IsValidMethod(method));
        }

        #endregion

        [Fact]
        public void GetValue_WithCircularReference_ShouldNotCauseStackOverflow()
        {
            // Arrange
            var tree = new HttpClientTree(typeof(CircularTypeA));

            var obj = new CircularTypeA { Name = "Test", Nested = new CircularTypeB { Value = 42 } };

            // Act
            var result = tree.GetValue(obj, "Name");

            // Assert
            Assert.Equal("Test", result);
        }

        #region Models
        private class Address
        {
            public string Street { get; set; } = "Main St";
        }

        private class Person
        {
            public string Name { get; set; } = "John Doe";
            public int Age { get; set; } = 30;
            public Address Address { get; set; } = new Address();

            public string GetGreeting() => $"Hello, {Name}";
        }

        private class WithGenerics
        {
            public string Name { get; set; } = "Generic Tester";

            // Método genérico simples
            public T Echo<T>(T value) => value;

            // Método genérico com restrição
            public T CreateInstance<T>() where T : new() => new T();

            // Método normal sem parâmetros (válido, só pra contraste)
            public string GetName() => Name;
        }

        private class WithParams
        {
            public string Name { get; set; } = "Param Tester";

            public string Format(string prefix) => $"{prefix} {Name}";

            public string Combine(string a, string b) => $"{a}-{b}";

            public int GetAge() => 42;
        }
        private class WithVoidMethods
        {
            public string Name { get; set; } = "Void Tester";

            public void DoSomething() { /* faz algo */ }

            public string GetName() => Name;
        }

        private class WithNullableProperty
        {
            public int? NullableValue { get; set; }
        }

        private class Company
        {
            public Person Owner { get; set; } = new Person();
        }

        private class WithPrivateMethod
        {
            public string Name { get; set; } = "Private Tester";

            private string GetSecret() => "Secret Value";

            public string GetName() => Name;
        }

        private class CircularTypeA
        {
            public string Name { get; set; }
            public CircularTypeB Nested { get; set; }
        }

        private class CircularTypeB
        {
            public int Value { get; set; }
            public CircularTypeA Back { get; set; }
        }
        #endregion
    }
}
