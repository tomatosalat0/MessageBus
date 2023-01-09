using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace MessageBus.Serialization.Json.Tests.UnitTests
{
    [TestClass]
    public class InterfaceDeserializeTests
    {
        [TestMethod]
        public void InterfaceEnabledDeserializationStillWorksWithConcreteClass()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":\"test value\"}");
            ConcreteClass instance = serializer.Deserialize<ConcreteClass>(inputJson);

            Assert.IsNotNull(instance);
            Assert.AreEqual("test value", instance.StringValue);
        }

        [TestMethod]
        public void InterfaceDeserializationDoesntWorkWithoutCustomDeserializer()
        {
            IMessageSerializer serializer = new JsonMessageSerializer();
            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":\"test value\", \"intValue\": 42, \"boolValue\": true}");

            Assert.ThrowsException<NotSupportedException>(() => serializer.Deserialize<ITest>(inputJson));
        }

        [TestMethod]
        public void InterfaceDeserializationPossible()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":\"test value\", \"intValue\": 42, \"boolValue\": true}");
            ITest instance = serializer.Deserialize<ITest>(inputJson);

            Assert.IsNotNull(instance);
            Assert.AreEqual("test value", instance.StringValue);
            Assert.AreEqual(42, instance.IntValue);
            Assert.AreEqual(true, instance.BoolValue);
        }

        [TestMethod]
        public void GeneratedPropertyWillThrowExceptionIfWritingNullToNotNullableProperty()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":null}");
            Assert.ThrowsException<ArgumentNullException>(() => serializer.Deserialize<ITest>(inputJson));
        }

        [TestMethod]
        public void GeneratedPropertyWillNotThrowExceptionIfNullWritingDisabled()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer(new TypeCreationOptions()
                {
                    ThrowExceptionGettingUnsetNotNullProperty = false,
                    ThrowExceptionSettingNullToNotNullProperty = false
                });

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":null}");
            ITest instance = serializer.Deserialize<ITest>(inputJson);

            Assert.IsNull(instance.StringValue);
        }

        [TestMethod]
        public void GeneratedPropertyWillThrowExceptionIfNotNotNullableAndNotSetByJson()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{}");
            ITest instance = serializer.Deserialize<ITest>(inputJson);

            Assert.ThrowsException<InvalidOperationException>(() => instance.StringValue);
        }

        [TestMethod]
        public void GeneratedPropertyWillNotThrowExceptionIfOptionsDisableGetterExceptions()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer(new TypeCreationOptions() { ThrowExceptionGettingUnsetNotNullProperty = false });

            byte[] inputJson = Encoding.UTF8.GetBytes("{}");
            ITest instance = serializer.Deserialize<ITest>(inputJson);

            Assert.IsNull(instance.StringValue);
        }

        [TestMethod]
        public void GeneratedPropertyWillNotThrowExceptionIfNullableAndNotSetByJson()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{}");
            ITest instance = serializer.Deserialize<ITest>(inputJson);

            Assert.IsNull(instance.NullableString);
        }

        [TestMethod]
        public void GeneratedTypeWorksWithReadWriteProperty()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":\"test value\"}");
            IReadWrite instance = serializer.Deserialize<IReadWrite>(inputJson);

            Assert.AreEqual("test value", instance.StringValue);

            instance.StringValue = "def";
            Assert.AreEqual("def", instance.StringValue);
        }

        [TestMethod]
        public void GeneratedTypeWorksWithWriteOnlyProperty()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":\"test value\"}");
            IWriteOnly instance = serializer.Deserialize<IWriteOnly>(inputJson);

            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void GeneratedTypeSupportsDefaultPropertyImplementation()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":\"test value\"}");
            IWithDefaultValue instance = serializer.Deserialize<IWithDefaultValue>(inputJson);

            Assert.AreEqual("test value", instance.StringValue);
            Assert.AreEqual("TEST VALUE", instance.UpperCaseValue);
        }

        [TestMethod]
        public void InterfaceWithDefaultPropertyImplementationWillGetIgnoredInJson()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":\"test value\", \"upperCaseValue\": \"something else\"}");
            IWithDefaultValue instance = serializer.Deserialize<IWithDefaultValue>(inputJson);

            Assert.AreEqual("test value", instance.StringValue);
            Assert.AreEqual("TEST VALUE", instance.UpperCaseValue);
        }

        [TestMethod]
        public void InterfaceWithGenericListGetsFilled()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"values\":[\"test value\", \"something else\"]}");
            IWithGenericList instance = serializer.Deserialize<IWithGenericList>(inputJson);

            Assert.AreEqual(2, instance.Values.Count);
            Assert.AreEqual("test value", instance.Values[0]);
            Assert.AreEqual("something else", instance.Values[1]);
        }

        [TestMethod]
        public void InterfaceWithGenericDictionaryGetsFilled()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"properties\":{\"test value\": \"something else\"}}");
            IWithGenericDictionary instance = serializer.Deserialize<IWithGenericDictionary>(inputJson);

            Assert.AreEqual(1, instance.Properties.Count);
            Assert.AreEqual("something else", instance.Properties["test value"]?.ToString());
        }

        [TestMethod]
        public void InterfaceWithInheritanceAllGetFilled()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"stringValue\":\"test value\", \"subType\": 42, \"additional\": \"extra\"}");
            ISubType instance = serializer.Deserialize<ISubType>(inputJson);

            Assert.AreEqual("test value", instance.StringValue);
            Assert.AreEqual(42, instance.SubType);
            Assert.AreEqual("extra", instance.Additional);
        }

        [TestMethod]
        public void InterfaceWithInhertanceSerializeAndDeserializeSupport()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            SubTypeImpl original = new SubTypeImpl() { Additional = "extra", StringValue = "test value", SubType = 42 };
            byte[] inputJson = serializer.Serialize(original);
            ISubType instance = serializer.Deserialize<ISubType>(inputJson);

            Assert.AreNotSame(original, instance);
            Assert.AreEqual("test value", instance.StringValue);
            Assert.AreEqual(42, instance.SubType);
            Assert.AreEqual("extra", instance.Additional);
        }

        [TestMethod]
        public void InterfaceSerializationSupportsReadOnlyStructProperty()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer(new TypeCreationOptions());

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"messageId\":\"myMessageId\"}");
            IHasMessageId instance = serializer.Deserialize<IHasMessageId>(inputJson);

            Assert.AreEqual("myMessageId", instance.MessageId.Value);
        }

        [TestMethod]
        public void InterfaceSerializationSupportsCustomTypes()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"messageId\":\"myMessageId\", \"value\": \"extra\"}");
            IExampleEvent instance = serializer.Deserialize<IExampleEvent>(inputJson);

            Assert.AreEqual("myMessageId", instance.MessageId.Value);
            Assert.AreEqual("extra", instance.Value);
        }

        [TestMethod]
        public void CompleteSerializationWorkflowTest()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();
            ExampleEventImplementation @event = new ExampleEventImplementation();

            byte[] serialized = serializer.Serialize<IExampleEvent>(@event);
            IExampleEvent deserialized = serializer.Deserialize<IExampleEvent>(serialized);

            Assert.AreEqual(@event.MessageId, deserialized.MessageId);
            Assert.AreEqual(@event.Value, deserialized.Value);
        }

        [TestMethod]
        public void NestedInterfaceGetsProperlyDeserialized()
        {
            IMessageSerializer serializer = new JsonMessageSerializer()
                .WithInterfaceDeserializer();

            byte[] inputJson = Encoding.UTF8.GetBytes("{\"value\": \"extra\"}");
            IParent.INested instance = serializer.Deserialize<IParent.INested>(inputJson);

            Assert.AreEqual("extra", instance.Value);
        }

        public class ConcreteClass
        {
            public string StringValue { get; set; } = null!;
        }

        public interface ITest
        {
            string StringValue { get; }

            int IntValue { get; }

            bool BoolValue { get; }

            string? NullableString { get; }
        }

        public interface IReadWrite
        {
            string StringValue { get; set; }
        }

        public interface IWriteOnly
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1044:Properties should not be write only", Justification = "Test Case")]
            string StringValue { set; }
        }

        public interface IWithDefaultValue
        {
            string StringValue { get; }

            string UpperCaseValue { get => StringValue.ToUpperInvariant(); }
        }

        public interface IAdditionalData
        {
            public string Additional { get; }
        }

        public interface ISubType : IReadWrite, IAdditionalData
        {
            public int SubType { get; }
        }

        public class SubTypeImpl : ISubType
        {
            public int SubType { get; init; }

            public string StringValue { get; set; } = null!;

            public string Additional { get; init; } = null!;
        }

        public interface IWithGenericList
        {
            IReadOnlyList<string> Values { get; }
        }

        public interface IWithGenericDictionary
        {
            IReadOnlyDictionary<string, object?> Properties { get; }
        }

        public interface IExampleEvent : IMessageEvent
        {
            string Value { get; }
        }

        public class ExampleEventImplementation : IExampleEvent
        {
            public string Value { get; } = "Implementation Value";

            public MessageId MessageId { get; } = MessageId.NewId();
        }

        public interface IParent
        {
            public interface INested
            {
                string Value { get; }
            }
        }
    }
}
