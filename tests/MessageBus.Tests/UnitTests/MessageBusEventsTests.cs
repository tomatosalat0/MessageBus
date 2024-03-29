﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MessageBus.Messaging.InProcess;
using MessageBus.Messaging.InProcess.Scheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class MessageBusEventsTests
    {
        [TestMethod]
        public async Task EventHandlerIsCalledForEvents()
        {
            ManualScheduler scheduler = new ManualScheduler();
            using IMessageBus bus = new MessageBrokerMessageBus(new InProcessMessageBroker(MessageBrokerOptions.BlockingManual(scheduler)), NoExceptionNotification.Instance);

            int numberOfCalls = 0;
            bus.RegisterEventDelegate<TestEventA>(m => numberOfCalls++);
            TestEventA @event = new TestEventA();

            await bus.FireEvent(@event);
            scheduler.Drain();

            Assert.AreEqual(1, numberOfCalls);
        }

        [TestMethod]
        public async Task EventHandlerIsOnlyCalledForRegisteredEvents()
        {
            ManualScheduler scheduler = new ManualScheduler();
            using IMessageBus bus = new MessageBrokerMessageBus(new InProcessMessageBroker(MessageBrokerOptions.BlockingManual(scheduler)), NoExceptionNotification.Instance);

            int numberOfCalls = 0;
            bus.RegisterEventDelegate<TestEventA>(m => numberOfCalls++);
            TestEventB @event = new TestEventB();

            await bus.FireEvent(@event);
            scheduler.Drain();

            Assert.AreEqual(0, numberOfCalls);
        }

        [TestMethod]
        public async Task ExecuteEventWithExceptionWillNotPropagateException()
        {
            Exception raisedException = null;
            MessageId failureMessage = default;
            ExceptionLogger logger = new ExceptionLogger((messageId, message, ex) =>
            {
                raisedException = ex;
                failureMessage = messageId;
            });
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), logger);

            bus.RegisterEventDelegate<TestEventA>(m => throw new NotSupportedException());
            TestEventA @event = new TestEventA();

            await bus.FireEvent(@event);
            await Task.Delay(1000);

            Assert.IsInstanceOfType(raisedException, typeof(NotSupportedException));
            Assert.AreEqual(failureMessage, @event.MessageId);
        }

        [TestMethod]
        public async Task EventHandlerSupportsAsync()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);
            using SemaphoreSlim semaphore = new SemaphoreSlim(1);

            await semaphore.WaitAsync();
            int numberOfCalls = 0;
            bus.RegisterEventDelegateAsync<TestEventA>(async m =>
            {
                await Task.Delay(1);
                numberOfCalls++;
                semaphore.Release();
            });
            TestEventA @event = new TestEventA();

            await bus.FireEvent(@event);
            await semaphore.WaitAsync();
            Assert.AreEqual(1, numberOfCalls);
        }

        [TestMethod]
        public async Task EventHandlerWithConfigurationIsApplied()
        {
            ManualScheduler scheduler = new ManualScheduler();
            using IMessageBus bus = new MessageBrokerMessageBus(new InProcessMessageBroker(MessageBrokerOptions.BlockingManual(scheduler)), NoExceptionNotification.Instance);

            MonitorDuplicationHandler duplicationHandler = new MonitorDuplicationHandler();
            int numberOfCalls = 0;
            bus.RegisterEventDelegate<TestEventA>(
                m => numberOfCalls++, 
                (handler) => handler.WithDuplicateMessageDetection(duplicationHandler)
            );
            TestEventA @event = new TestEventA();

            await bus.FireEvent(@event);
            scheduler.Drain();

            Assert.AreEqual(1, numberOfCalls);
            Assert.AreEqual(1, duplicationHandler.SeenMessages.Count);
            Assert.AreEqual(@event.MessageId, duplicationHandler.SeenMessages[0]);
        }

        [Topic("Events/TestEventA")]
        private class TestEventA : IMessageEvent
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        [Topic("Events/TestEventB")]
        private class TestEventB : IMessageEvent
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        [TestMethod]
        public void SubscriptionAreHandlerGetsSubscription()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            SubscriptionAwareEventHandler handler = new SubscriptionAwareEventHandler();
            bus.RegisterEventHandler(handler);

            Assert.IsNotNull(handler.Subscription);
        }

        private class MonitorDuplicationHandler : Decorations.Duplications.IDuplicateDetection
        {
            public List<MessageId> SeenMessages { get; } = new List<MessageId>();

            public void ForgetMessage(MessageId messageId)
            {
            }

            public bool HandleReceivedMessage(MessageId messageId)
            {
                SeenMessages.Add(messageId);
                return true;
            }
        }

        private class SubscriptionAwareEventHandler : IMessageEventHandler<TestEventA>, ISubscriptionAwareHandler
        {
            public IDisposable Subscription { get; private set; }

            public void Handle(TestEventA @event)
            {
            }

            public void RegisterSubscription(IDisposable subscription)
            {
                if (Subscription is not null)
                    throw new InvalidOperationException("Subscription called more than once");
                Subscription = subscription;
            }
        }
    }
}
