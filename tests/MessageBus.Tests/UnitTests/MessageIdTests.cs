using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class MessageIdTests
    {
        [TestMethod]
        public void MessageIdCanNotBeCreatedWithNoId()
        {
            Assert.ThrowsException<ArgumentException>(() => new MessageId(null));
            Assert.ThrowsException<ArgumentException>(() => new MessageId(string.Empty));
        }

        [TestMethod]
        public void NewMessageIdHasNoCausation()
        {
            MessageId id = MessageId.NewId();
            Assert.IsNull(id.CausationId);
        }

        [TestMethod]
        public void MessageWithCorrelationSetsCausation()
        {
            MessageId causation = MessageId.NewId();
            MessageId id = MessageId.CausedBy(causation);

            Assert.AreEqual(id.CausationId, causation.Value);
        }

        [TestMethod]
        public void MessageIdEqualityCheck()
        {
            MessageId idA = new MessageId("Test");
            MessageId idB = new MessageId("Test");

            Assert.AreEqual(idA, idB);
            Assert.IsTrue(idA.Equals(idB));
            Assert.IsFalse(idA != idB);
            Assert.IsTrue(idA == idB);
        }

        [TestMethod]
        public void MessageIdEqualityOnlyAcceptsOtherMessageIds()
        {
            MessageId id = new MessageId();
            Assert.IsFalse(id.Equals(id.Value));
        }

        [TestMethod]
        public void MessageIdStringContainsCausation()
        {
            MessageId causation = MessageId.NewId();
            MessageId id = MessageId.CausedBy(causation);

            Assert.IsTrue(id.ToString().Contains(causation.Value));
            Assert.IsTrue(id.ToString().Contains(id.Value));
        }

        [TestMethod]
        public void MessageIdOnlyContainsIdWithNoCausation()
        {
            MessageId id = MessageId.NewId();
            Assert.AreEqual(id.Value, id.ToString());
        }

        [TestMethod]
        public void MessageIdEqualityWorksAsExpected()
        {
            MessageId idA = MessageId.NewId();
            MessageId idB = MessageId.NewId();

            HashSet<MessageId> messages = new HashSet<MessageId>();
            Assert.IsTrue(messages.Add(idA));
            Assert.IsTrue(messages.Add(idB));
            Assert.IsFalse(messages.Add(idA));
            Assert.IsFalse(messages.Add(idB));
        }

        [TestMethod]
        public void ParsingInvalidMessageWllThrowAnException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => MessageId.Parse(null));
            Assert.ThrowsException<ArgumentException>(() => MessageId.Parse(string.Empty));
            Assert.ThrowsException<ArgumentException>(() => MessageId.Parse("->"));
            Assert.ThrowsException<ArgumentException>(() => MessageId.Parse("test->"));
            Assert.ThrowsException<ArgumentException>(() => MessageId.Parse("->test"));
        }

        [TestMethod]
        public void ParsingMessageIdHandlesNoCausationId()
        {
            MessageId message = new MessageId("<message>");

            string asString = message.ToString();
            MessageId parsed = MessageId.Parse(asString);

            Assert.AreEqual(message.Value, parsed.Value);
            Assert.IsNull(parsed.CausationId);
        }

        [TestMethod]
        public void ParsingMessageIdHandlesCausationId()
        {
            MessageId message = MessageId.CausedBy(new MessageId("<causation>"));

            string asString = message.ToString();
            MessageId parsed = MessageId.Parse(asString);

            Assert.AreEqual(message.Value, parsed.Value);
            Assert.AreEqual(message.CausationId, parsed.CausationId);
        }
    }
}
