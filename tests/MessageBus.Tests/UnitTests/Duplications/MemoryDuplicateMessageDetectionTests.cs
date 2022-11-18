using System;
using System.Collections.Generic;
using System.Linq;
using MessageBus.Decorations.Duplications;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests.Duplications
{
    [TestClass]
    public class MemoryDuplicateMessageDetectionTests
    {
        [TestMethod]
        public void DuplicationDetectionWillOnlyKeepTheLatestItems()
        {
            MemoryDuplicateMessageDetection detection = new MemoryDuplicateMessageDetection(maxIdsToRemember: 2, TimeSpan.FromMinutes(2));
            List<MessageId> ids = Enumerable.Range(0, 4).Select(p => MessageId.NewId()).ToList();

            foreach (var p in ids)
                Assert.IsTrue(detection.HandleReceivedMessage(p));

            // expected outcome: the last two are remembered and will return false
            int secondHalfSecondPass = ids.Skip(2)
                .Where(detection.HandleReceivedMessage)
                .Count();
            Assert.AreEqual(0, secondHalfSecondPass);

            // expected outcome: the first two already were forgotten and so they will return true
            int firstHalfSecondPass = ids.Take(2)
                .Where(detection.HandleReceivedMessage)
                .Count();
            Assert.AreEqual(2, firstHalfSecondPass);
        }

        [TestMethod]
        public void DuplicationDetectionWillForgetItemsWhichAreTooOld()
        {
            MemoryDuplicateMessageDetection detection = new MemoryDuplicateMessageDetection(maxIdsToRemember: 4, maxDurationToRemember: TimeSpan.FromMinutes(-2));
            List<MessageId> ids = Enumerable.Range(0, 4).Select(p => MessageId.NewId()).ToList();

            foreach (var p in ids)
                Assert.IsTrue(detection.HandleReceivedMessage(p));

            // expected outcome: everything has been forgotten already because of the 
            // negative max. duration
            foreach (var p in ids)
                Assert.IsTrue(detection.HandleReceivedMessage(p));
        }
    }
}
