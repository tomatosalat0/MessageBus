using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MessageBus.Tests.UnitTests
{
    [TestClass]
    public class MultipleEventTypesTests
    {
        [TestMethod]
        public void RegisterAllThrowsExceptionOnNull()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            Assert.ThrowsException<ArgumentNullException>(() => bus.RegisterAllEventHandlers(null));
        }

        [TestMethod]
        public void RegisterAllThrowsExceptionIfObjectDoesNotImplementAnyHandler()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            Assert.ThrowsException<InvalidOperationException>(() => bus.RegisterAllEventHandlers(new object()));
        }

        [TestMethod]
        public void RegisterSucceedsIfMessageBusIsExplicitelyImplemented()
        {
            IMessageBusHandler bus = new ExplicitMessageHandlerImplementation();

            IReadOnlyList<IDisposable> subscriptions = bus.RegisterAllEventHandlers(new MyMultiEventHandler());
            Assert.AreEqual(3, subscriptions.Count);
        }

        [TestMethod]
        public void RegisterSucceedsIfMessageBusIsImplicitelyImplemented()
        {
            IMessageBusHandler bus = new ImplicitMessageHandlerImplementation();

            IReadOnlyList<IDisposable> subscriptions = bus.RegisterAllEventHandlers(new MyMultiEventHandler());
            Assert.AreEqual(3, subscriptions.Count);
        }

        [TestMethod]
        public void RegisterThrowsTargetInvokationExceptionIfMessageBusFails()
        {
            IMessageBusHandler bus = new AlwaysFailMessageHandler();

            Assert.ThrowsException<TargetInvocationException>(() => bus.RegisterAllEventHandlers(new MyMultiEventHandler()));
        }

        [TestMethod]
        public async Task HandlerWithMultipleTypesCanGetAdded()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            MyMultiEventHandler handler = new MyMultiEventHandler();
            bus.RegisterAllEventHandlers(handler);

            await bus.FireEvent(new MyEventA());
            await Task.Delay(100);

            Assert.AreEqual(1, handler.MyEvent_A_Calls);
            Assert.AreEqual(0, handler.MyEvent_B_Calls);
            Assert.AreEqual(0, handler.MyEvent_C_Calls);

            await bus.FireEvent(new MyEventB());
            await Task.Delay(100);

            Assert.AreEqual(1, handler.MyEvent_A_Calls);
            Assert.AreEqual(1, handler.MyEvent_B_Calls);
            Assert.AreEqual(0, handler.MyEvent_C_Calls);

            await bus.FireEvent(new MyEventC());
            await Task.Delay(100);

            Assert.AreEqual(1, handler.MyEvent_A_Calls);
            Assert.AreEqual(1, handler.MyEvent_B_Calls);
            Assert.AreEqual(1, handler.MyEvent_C_Calls);
        }

        [TestMethod]
        public async Task MultipleCallsOfRegisterAllWillRegisterEverythingAgain()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            MyMultiEventHandler handler = new MyMultiEventHandler();
            bus.RegisterAllEventHandlers(handler);
            bus.RegisterAllEventHandlers(handler);

            await bus.FireEvent(new MyEventA());
            await bus.FireEvent(new MyEventB());
            await bus.FireEvent(new MyEventC());
            await Task.Delay(100);

            Assert.AreEqual(2, handler.MyEvent_A_Calls);
            Assert.AreEqual(2, handler.MyEvent_B_Calls);
            Assert.AreEqual(2, handler.MyEvent_C_Calls);
        }

        [TestMethod]
        public void RegisterEventHandlerReturnsAllSubscriptionDisposables()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            MyMultiEventHandler handler = new MyMultiEventHandler();
            IReadOnlyList<IDisposable> subscriptions = bus.RegisterAllEventHandlers(handler);
            Assert.AreEqual(3, subscriptions.Count);
            Assert.AreEqual(3, subscriptions.Distinct().Count());
        }

        [TestMethod]
        public async Task ReturnedSubscriptionsDisposablesBelongToEventHandler()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            MyMultiEventHandler handler = new MyMultiEventHandler();
            IReadOnlyList<IDisposable> subscriptions = bus.RegisterAllEventHandlers(handler);

            foreach (var p in subscriptions)
                p.Dispose();

            await bus.FireEvent(new MyEventA());
            await bus.FireEvent(new MyEventB());
            await bus.FireEvent(new MyEventC());
            await Task.Delay(100);

            Assert.AreEqual(0, handler.MyEvent_A_Calls);
            Assert.AreEqual(0, handler.MyEvent_B_Calls);
            Assert.AreEqual(0, handler.MyEvent_C_Calls);
        }

        [TestMethod]
        public async Task RegistrationExceptionWillRollbackPreviouslyRegisteredHandlers()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            MyMultiHandlerWithAnIncompleteEvent handler = new MyMultiHandlerWithAnIncompleteEvent();
            Assert.ThrowsException<TargetInvocationException>(() => bus.RegisterAllEventHandlers(handler));

            await bus.FireEvent(new MyEventA());
            await Task.Delay(100);

            Assert.AreEqual(0, handler.MyEvent_A_Calls);
            Assert.AreEqual(0, handler.MyEvent_Incomplete_Calls);
        }

        [TestMethod]
        public async Task RegisterationExceptionWillOnlyRollbackWhatHappenedWithinThisMethod()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            MyMultiHandlerWithAnIncompleteEvent handler = new MyMultiHandlerWithAnIncompleteEvent();
            // this registration is still valid.
            bus.RegisterEventHandler<MyEventA>(handler);

            Assert.ThrowsException<TargetInvocationException>(() => bus.RegisterAllEventHandlers(handler));

            await bus.FireEvent(new MyEventA());
            await Task.Delay(100);

            // will be "1" because of the previous manual registration
            Assert.AreEqual(1, handler.MyEvent_A_Calls);
            Assert.AreEqual(0, handler.MyEvent_Incomplete_Calls);
        }

        [TestMethod]
        public async Task RegistrationSupportsQueryHandler()
        {
            using IMessageBus bus = new MessageBrokerMessageBus(MemoryMessageBrokerBuilder.InProcessBroker(), NoExceptionNotification.Instance);

            MyMultiQueryHandler handler = new MyMultiQueryHandler();
            bus.RegisterAllQueryHandlers(handler);

            MyQueryAResult resultA = await bus.FireQuery<MyQueryA, MyQueryAResult>(new MyQueryA(), TimeSpan.FromSeconds(2));
            MyQueryBResult resultB = await bus.FireQuery<MyQueryB, MyQueryBResult>(new MyQueryB(), TimeSpan.FromSeconds(2));

            Assert.AreEqual(10, resultA.ResultA);
            Assert.AreEqual(42, resultB.ResultB);
        }

        [Topic("Events/EventA")]
        public class MyEventA : IMessageEvent
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        [Topic("Events/EventB")]
        public class MyEventB : IMessageEvent
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        [Topic("Events/EventC")]
        public class MyEventC : IMessageEvent
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        // the missing topic attribute is intential
        public class MyIncompleteConfiguredEvent : IMessageEvent
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        private class MyMultiEventHandler :
            IMessageEventHandler<MyEventA>,
            IMessageEventHandler<MyEventB>,
            IAsyncMessageEventHandler<MyEventC>
        {
            public int MyEvent_A_Calls { get; private set; }
            public int MyEvent_B_Calls { get; private set; }
            public int MyEvent_C_Calls { get; private set; }

            public void Handle(MyEventA @event)
            {
                MyEvent_A_Calls++;
            }

            public void Handle(MyEventB @event)
            {
                MyEvent_B_Calls++;
            }

            public Task HandleAsync(MyEventC @event)
            {
                MyEvent_C_Calls++;
                return Task.CompletedTask;
            }
        }

        private class MyMultiHandlerWithAnIncompleteEvent :
            IMessageEventHandler<MyEventA>,
            IMessageEventHandler<MyIncompleteConfiguredEvent>
        {
            public int MyEvent_A_Calls { get; private set; }
            public int MyEvent_Incomplete_Calls { get; private set; }

            public void Handle(MyEventA @event)
            {
                MyEvent_A_Calls++;
            }

            public void Handle(MyIncompleteConfiguredEvent @event)
            {
                MyEvent_Incomplete_Calls++;
            }
        }

        [Topic("Queries/MyQueryA")]
        public class MyQueryA : IMessageQuery<MyQueryAResult>
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        public class MyQueryAResult : IMessageQueryResult
        {
            public MyQueryAResult(MessageId messageId)
            {
                MessageId = messageId;
            }

            public MessageId MessageId { get; }

            public int ResultA { get; } = 10;
        }

        [Topic("Queries/MyQueryB")]
        public class MyQueryB : IMessageQuery<MyQueryBResult>
        {
            public MessageId MessageId { get; } = MessageId.NewId();
        }

        public class MyQueryBResult : IMessageQueryResult
        {
            public MyQueryBResult(MessageId messageId)
            {
                MessageId = messageId;
            }

            public MessageId MessageId { get; }

            public int ResultB { get; } = 42;
        }

        private class MyMultiQueryHandler :
            IMessageQueryHandler<MyQueryA, MyQueryAResult>,
            IMessageQueryHandler<MyQueryB, MyQueryBResult>
        {
            public MyQueryAResult Handle(MyQueryA query)
            {
                return new MyQueryAResult(query.MessageId);
            }

            public MyQueryBResult Handle(MyQueryB query)
            {
                return new MyQueryBResult(query.MessageId);
            }
        }

        private class NoopDispose : IDisposable
        {
            public void Dispose()
            {
            }
        }

        private class ExplicitMessageHandlerImplementation : IMessageBusHandler
        {
            IDisposable IMessageBusHandler.RegisterCommandHandler<TCommand>(IMessageCommandHandler<TCommand> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterCommandHandler<TCommand>(IAsyncMessageCommandHandler<TCommand> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterEventHandler<TEvent>(IMessageEventHandler<TEvent> handler)
            {
                return new NoopDispose();
            }

            IDisposable IMessageBusHandler.RegisterEventHandler<TEvent>(IAsyncMessageEventHandler<TEvent> handler)
            {
                return new NoopDispose();
            }

            IDisposable IMessageBusHandler.RegisterQueryHandler<TQuery, TQueryResult>(IMessageQueryHandler<TQuery, TQueryResult> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterQueryHandler<TQuery, TQueryResult>(IAsyncMessageQueryHandler<TQuery, TQueryResult> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterRpcHandler<TRpc, TRpcResult>(IMessageRpcHandler<TRpc, TRpcResult> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterRpcHandler<TRpc, TRpcResult>(IAsyncMessageRpcHandler<TRpc, TRpcResult> handler) => throw new NotImplementedException();
        }

        private class ImplicitMessageHandlerImplementation : IMessageBusHandler
        {
            public IDisposable RegisterCommandHandler<TCommand>(IMessageCommandHandler<TCommand> handler) where TCommand : IMessageCommand => throw new NotImplementedException();

            public IDisposable RegisterCommandHandler<TCommand>(IAsyncMessageCommandHandler<TCommand> handler) where TCommand : IMessageCommand => throw new NotImplementedException();

            public IDisposable RegisterEventHandler<TEvent>(IMessageEventHandler<TEvent> handler) where TEvent : IMessageEvent
            {
                return new NoopDispose();
            }

            public IDisposable RegisterEventHandler<TEvent>(IAsyncMessageEventHandler<TEvent> handler) where TEvent : IMessageEvent
            {
                return new NoopDispose();
            }

            public IDisposable RegisterQueryHandler<TQuery, TQueryResult>(IMessageQueryHandler<TQuery, TQueryResult> handler)
                where TQuery : IMessageQuery<TQueryResult>
                where TQueryResult : IMessageQueryResult => throw new NotImplementedException();

            public IDisposable RegisterQueryHandler<TQuery, TQueryResult>(IAsyncMessageQueryHandler<TQuery, TQueryResult> handler)
                where TQuery : IMessageQuery<TQueryResult>
                where TQueryResult : IMessageQueryResult => throw new NotImplementedException();

            public IDisposable RegisterRpcHandler<TRpc, TRpcResult>(IMessageRpcHandler<TRpc, TRpcResult> handler)
                where TRpc : IMessageRpc<TRpcResult>
                where TRpcResult : IMessageRpcResult => throw new NotImplementedException();

            public IDisposable RegisterRpcHandler<TRpc, TRpcResult>(IAsyncMessageRpcHandler<TRpc, TRpcResult> handler)
                where TRpc : IMessageRpc<TRpcResult>
                where TRpcResult : IMessageRpcResult => throw new NotImplementedException();
        }

        private class AlwaysFailMessageHandler : IMessageBusHandler
        {
            IDisposable IMessageBusHandler.RegisterCommandHandler<TCommand>(IMessageCommandHandler<TCommand> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterCommandHandler<TCommand>(IAsyncMessageCommandHandler<TCommand> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterEventHandler<TEvent>(IMessageEventHandler<TEvent> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterEventHandler<TEvent>(IAsyncMessageEventHandler<TEvent> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterQueryHandler<TQuery, TQueryResult>(IMessageQueryHandler<TQuery, TQueryResult> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterQueryHandler<TQuery, TQueryResult>(IAsyncMessageQueryHandler<TQuery, TQueryResult> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterRpcHandler<TRpc, TRpcResult>(IMessageRpcHandler<TRpc, TRpcResult> handler) => throw new NotImplementedException();

            IDisposable IMessageBusHandler.RegisterRpcHandler<TRpc, TRpcResult>(IAsyncMessageRpcHandler<TRpc, TRpcResult> handler) => throw new NotImplementedException();
        }
    }
}
