using System;
using System.Collections.Generic;
using Xunit;
using AgentCommon.AgentPluginCommon;
using Newtonsoft.Json.Linq;

namespace RemoteAgent.Tests
{
    public class PluginArgumentsTests
    {
        [Fact]
        public void PluginArguments_DefaultConstructor_ShouldCreateEmptyArguments()
        {
            // Arrange & Act
            var args = new PluginArguments();

            // Assert
            Assert.False(args.HasArgument("test"));
            Assert.Equal("{}", args.ToJson());
        }

        [Fact]
        public void PluginArguments_DictionaryConstructor_ShouldCopyArguments()
        {
            // Arrange
            var dict = new Dictionary<string, object>
            {
                {"key1", "value1"},
                {"key2", 42},
                {"key3", true}
            };

            // Act
            var args = new PluginArguments(dict);

            // Assert
            Assert.True(args.HasArgument("key1"));
            Assert.True(args.HasArgument("key2"));
            Assert.True(args.HasArgument("key3"));
            Assert.Equal("value1", args.GetArgument<string>("key1"));
            Assert.Equal(42, args.GetArgument<int>("key2"));
            Assert.True(args.GetArgument<bool>("key3"));
        }

        [Fact]
        public void FromJson_ValidJson_ShouldCreateCorrectArguments()
        {
            // Arrange
            var json = "{\"path\":\"C:\\\\test\",\"count\":100,\"enabled\":true}";

            // Act
            var args = PluginArguments.FromJson(json);

            // Assert
            Assert.Equal("C:\\test", args.GetArgument<string>("path"));
            Assert.Equal(100, args.GetArgument<int>("count"));
            Assert.True(args.GetArgument<bool>("enabled"));
        }

        [Fact]
        public void FromJson_EmptyOrNullJson_ShouldReturnEmptyArguments()
        {
            // Act
            var args1 = PluginArguments.FromJson("");
            var args2 = PluginArguments.FromJson(null);
            var args3 = PluginArguments.FromJson("   ");

            // Assert
            Assert.False(args1.HasArgument("test"));
            Assert.False(args2.HasArgument("test"));
            Assert.False(args3.HasArgument("test"));
        }

        [Fact]
        public void FromJObject_ValidJObject_ShouldCreateCorrectArguments()
        {
            // Arrange
            var jObject = JObject.Parse("{\"name\":\"test\",\"value\":123}");

            // Act
            var args = PluginArguments.FromJObject(jObject);

            // Assert
            Assert.Equal("test", args.GetArgument<string>("name"));
            Assert.Equal(123, args.GetArgument<int>("value"));
        }

        [Fact]
        public void FromJObject_NullJObject_ShouldReturnEmptyArguments()
        {
            // Act
            var args = PluginArguments.FromJObject(null);

            // Assert
            Assert.False(args.HasArgument("test"));
        }

        [Fact]
        public void GetArgument_ExistingKey_ShouldReturnCorrectValue()
        {
            // Arrange
            var args = new PluginArguments();
            args.AddArgument("string", "test");
            args.AddArgument("int", 42);
            args.AddArgument("bool", true);

            // Act & Assert
            Assert.Equal("test", args.GetArgument<string>("string"));
            Assert.Equal(42, args.GetArgument<int>("int"));
            Assert.True(args.GetArgument<bool>("bool"));
        }

        [Fact]
        public void GetArgument_NonExistingKey_ShouldReturnDefault()
        {
            // Arrange
            var args = new PluginArguments();

            // Act & Assert
            Assert.Equal(default(string), args.GetArgument<string>("missing"));
            Assert.Equal(0, args.GetArgument<int>("missing"));
            Assert.False(args.GetArgument<bool>("missing"));
            Assert.Equal("default", args.GetArgument("missing", "default"));
        }

        [Fact]
        public void GetArgument_CaseInsensitive_ShouldWork()
        {
            // Arrange
            var args = new PluginArguments();
            args.AddArgument("TestKey", "value");

            // Act & Assert
            Assert.Equal("value", args.GetArgument<string>("testkey"));
            Assert.Equal("value", args.GetArgument<string>("TESTKEY"));
            Assert.Equal("value", args.GetArgument<string>("TestKey"));
        }

        [Fact]
        public void TryGetArgument_ExistingKey_ShouldReturnTrueAndValue()
        {
            // Arrange
            var args = new PluginArguments();
            args.AddArgument("test", "value");

            // Act
            var result = args.TryGetArgument<string>("test", out var value);

            // Assert
            Assert.True(result);
            Assert.Equal("value", value);
        }

        [Fact]
        public void TryGetArgument_NonExistingKey_ShouldReturnFalse()
        {
            // Arrange
            var args = new PluginArguments();

            // Act
            var result = args.TryGetArgument<string>("missing", out var value);

            // Assert
            Assert.False(result);
            Assert.Equal(default(string), value);
        }

        [Fact]
        public void AddArgument_ShouldAddArgument()
        {
            // Arrange
            var args = new PluginArguments();

            // Act
            args.AddArgument("test", "value");

            // Assert
            Assert.True(args.HasArgument("test"));
            Assert.Equal("value", args.GetArgument<string>("test"));
        }

        [Fact]
        public void RemoveArgument_ExistingKey_ShouldRemoveArgument()
        {
            // Arrange
            var args = new PluginArguments();
            args.AddArgument("test", "value");

            // Act
            args.RemoveArgument("test");

            // Assert
            Assert.False(args.HasArgument("test"));
        }

        [Fact]
        public void GetArgumentKeys_ShouldReturnAllKeys()
        {
            // Arrange
            var args = new PluginArguments();
            args.AddArgument("key1", "value1");
            args.AddArgument("key2", "value2");

            // Act
            var keys = new List<string>(args.GetArgumentKeys());

            // Assert
            Assert.Contains("key1", keys);
            Assert.Contains("key2", keys);
            Assert.Equal(2, keys.Count);
        }

        [Fact]
        public void ToJson_ShouldSerializeCorrectly()
        {
            // Arrange
            var args = new PluginArguments();
            args.AddArgument("string", "test");
            args.AddArgument("number", 42);
            args.AddArgument("boolean", true);

            // Act
            var json = args.ToJson();

            // Assert
            Assert.Contains("string", json);
            Assert.Contains("test", json);
            Assert.Contains("number", json);
            Assert.Contains("42", json);
            Assert.Contains("boolean", json);
            Assert.Contains("true", json);
        }

        [Fact]
        public void ToJObject_ShouldCreateCorrectJObject()
        {
            // Arrange
            var args = new PluginArguments();
            args.AddArgument("test", "value");
            args.AddArgument("number", 123);

            // Act
            var jObject = args.ToJObject();

            // Assert
            Assert.Equal("value", jObject["test"].ToString());
            Assert.Equal(123, jObject["number"].Value<int>());
        }
    }
}
