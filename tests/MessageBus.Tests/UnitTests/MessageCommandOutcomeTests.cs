﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class MessageCommandOutcomeTests
    {
        [TestMethod]
        public void SuccessOutcomeHasNoException()
        {
            MessageId id = MessageId.NewId();
            MessageCommandOutcome outcome = MessageCommandOutcome.Success(id);

            Assert.IsNull(outcome.FailureDetails);
            Assert.AreEqual(id, outcome.MessageId);
        }

        [TestMethod]
        public void FailureOutcomeHasException()
        {
            MessageId id = MessageId.NewId();
            Exception ex = new Exception();
            MessageCommandOutcome outcome = MessageCommandOutcome.Failure(id, ex);

            Assert.IsNotNull(outcome.FailureDetails);
            Assert.AreEqual(id, outcome.MessageId);
        }
    }
}
