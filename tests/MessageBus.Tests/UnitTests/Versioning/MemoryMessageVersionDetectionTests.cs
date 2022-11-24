using System.Linq;
using MessageBus.Decorations.Versioning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests.Versioning
{
    [TestClass]
    public class MemoryMessageVersionDetectionTests
    {
        [TestMethod]
        public void EmptyDetectorWillAllowNewVersion()
        {
            MemoryMessageVersionDetection<int> detector = new MemoryMessageVersionDetection<int>();
            Assert.IsTrue(detector.HandleMessageVersion(0));
        }

        [TestMethod]
        public void SameVersionWillGetDiscarded()
        {
            MemoryMessageVersionDetection<int> detector = new MemoryMessageVersionDetection<int>();
            detector.HandleMessageVersion(0);

            Assert.IsFalse(detector.HandleMessageVersion(0));
        }

        [TestMethod]
        public void OldVersionWillGetDiscarded()
        {
            MemoryMessageVersionDetection<int> detector = new MemoryMessageVersionDetection<int>();
            detector.HandleMessageVersion(1);

            Assert.IsFalse(detector.HandleMessageVersion(0));
        }

        [TestMethod]
        public void HighestVersionWillGetRemebered()
        {
            MemoryMessageVersionDetection<int> detector = new MemoryMessageVersionDetection<int>();

            foreach (var version in Enumerable.Range(0, 1000))
                detector.HandleMessageVersion(version);

            Assert.IsFalse(detector.HandleMessageVersion(0));
            Assert.IsFalse(detector.HandleMessageVersion(999));
            Assert.IsTrue(detector.HandleMessageVersion(1000));
        }

        [TestMethod]
        public void ForgettingVersionWillResetAllowThatVersionAgain()
        {
            MemoryMessageVersionDetection<int> detector = new MemoryMessageVersionDetection<int>();
            detector.HandleMessageVersion(0);
            detector.ForgetVersion(0);

            Assert.IsTrue(detector.HandleMessageVersion(0));
        }

        [TestMethod]
        public void ForgettingVersionWillResetToThePreviousKnown()
        {
            MemoryMessageVersionDetection<int> detector = new MemoryMessageVersionDetection<int>();
            detector.HandleMessageVersion(1);
            detector.HandleMessageVersion(2);
            detector.ForgetVersion(2);

            Assert.IsFalse(detector.HandleMessageVersion(1));
            Assert.IsTrue(detector.HandleMessageVersion(2));
        }

        [TestMethod]
        public void ForgettingVersionWillRestorePreviousGaps()
        {
            MemoryMessageVersionDetection<int> detector = new MemoryMessageVersionDetection<int>();
            detector.HandleMessageVersion(1);
            detector.HandleMessageVersion(200);
            detector.ForgetVersion(200);

            Assert.IsFalse(detector.HandleMessageVersion(1));
            Assert.IsTrue(detector.HandleMessageVersion(2));
            Assert.IsTrue(detector.HandleMessageVersion(200));
        }
    }
}
