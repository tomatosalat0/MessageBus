using System;
using System.Threading.Tasks;

namespace MessageBus
{
    public static class MessageBusDelegateExtensions
    {
        public static IDisposable RegisterEventDelegate<TEvent>(
            this IMessageBusHandler subscriptionHandler,
            Action<TEvent> eventHandler,
            Func<IMessageEventHandler<TEvent>, IMessageEventHandler<TEvent>>? configure = null)
            where TEvent : IMessageEvent
        {
            IMessageEventHandler<TEvent> handler = new DelegateEventHandler<TEvent>(eventHandler);
            if (configure is not null)
                handler = configure(handler);
            return subscriptionHandler.RegisterEventHandler(handler);
        }

        public static IDisposable RegisterEventDelegateAsync<TEvent>(
            this IMessageBusHandler subscriptionHandler,
            Func<TEvent, Task> eventHandler,
            Func<IAsyncMessageEventHandler<TEvent>, IAsyncMessageEventHandler<TEvent>>? configure = null)
            where TEvent : IMessageEvent
        {
            IAsyncMessageEventHandler<TEvent> handler = new AsyncDelegateEventHandler<TEvent>(eventHandler);
            if (configure is not null)
                handler = configure(handler);
            return subscriptionHandler.RegisterEventHandler(handler);
        }

        public static IDisposable RegisterQueryDelegate<TQuery, TQueryResult>(
            this IMessageBusHandler subscriptionHandler,
            Func<TQuery, TQueryResult> queryHandler,
            Func<IMessageQueryHandler<TQuery, TQueryResult>, IMessageQueryHandler<TQuery, TQueryResult>>? configure = null)
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            IMessageQueryHandler<TQuery, TQueryResult> handler = new QueryHandler<TQuery, TQueryResult>(queryHandler);
            if (configure is not null)
                handler = configure(handler);
            return subscriptionHandler.RegisterQueryHandler(handler);
        }

        public static IDisposable RegisterQueryDelegateAsync<TQuery, TQueryResult>(
            this IMessageBusHandler subscriptionHandler,
            Func<TQuery, Task<TQueryResult>> queryHandler,
            Func<IAsyncMessageQueryHandler<TQuery, TQueryResult>, IAsyncMessageQueryHandler<TQuery, TQueryResult>>? configure = null)
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            IAsyncMessageQueryHandler<TQuery, TQueryResult> handler = new AsyncQueryHandler<TQuery, TQueryResult>(queryHandler);
            if (configure is not null)
                handler = configure(handler);
            return subscriptionHandler.RegisterQueryHandler(handler);
        }

        public static IDisposable RegisterCommandDelegate<TCommand>(
            this IMessageBusHandler subscriptionHandler,
            Action<TCommand> commandHandler,
            Func<IMessageCommandHandler<TCommand>, IMessageCommandHandler<TCommand>>? configure = null)
            where TCommand : IMessageCommand
        {
            IMessageCommandHandler<TCommand> handler = new CommandHandler<TCommand>(commandHandler);
            if (configure is not null)
                handler = configure(handler);
            return subscriptionHandler.RegisterCommandHandler(handler);
        }

        public static IDisposable RegisterCommandDelegateAsync<TCommand>(
            this IMessageBusHandler subscriptionHandler,
            Func<TCommand, Task> commandHandler,
            Func<IAsyncMessageCommandHandler<TCommand>, IAsyncMessageCommandHandler<TCommand>>? configure = null)
            where TCommand : IMessageCommand
        {
            IAsyncMessageCommandHandler<TCommand> handler = new AsyncCommandHandler<TCommand>(commandHandler);
            if (configure is not null)
                handler = configure(handler);
            return subscriptionHandler.RegisterCommandHandler(handler);
        }

        public static IDisposable RegisterRpcDelegate<TRpc, TRpcResult>(
            this IMessageBusHandler subscriptionHandler,
            Func<TRpc, TRpcResult> queryHandler,
            Func<IMessageRpcHandler<TRpc, TRpcResult>, IMessageRpcHandler<TRpc, TRpcResult>>? configure = null)
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            IMessageRpcHandler<TRpc, TRpcResult> handler = new RpcHandler<TRpc, TRpcResult>(queryHandler);
            if (configure is not null)
                handler = configure(handler);
            return subscriptionHandler.RegisterRpcHandler(new RpcHandler<TRpc, TRpcResult>(queryHandler));
        }

        public static IDisposable RegisterRpcDelegateAsync<TRpc, TRpcResult>(
            this IMessageBusHandler subscriptionHandler,
            Func<TRpc, Task<TRpcResult>> queryHandler,
            Func<IAsyncMessageRpcHandler<TRpc, TRpcResult>, IAsyncMessageRpcHandler<TRpc, TRpcResult>>? configure = null)
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            IAsyncMessageRpcHandler<TRpc, TRpcResult> handler = new AsyncRpcHandler<TRpc, TRpcResult>(queryHandler);
            if (configure is not null)
                handler = configure(handler);
            return subscriptionHandler.RegisterRpcHandler(handler);
        }

        private class DelegateEventHandler<TEvent> : IMessageEventHandler<TEvent> where TEvent : IMessageEvent
        {
            private readonly Action<TEvent> _handler;

            public DelegateEventHandler(Action<TEvent> handler)
            {
                _handler = handler;
            }

            public void Handle(TEvent @event)
            {
                _handler(@event);
            }
        }

        private class AsyncDelegateEventHandler<TEvent> : IAsyncMessageEventHandler<TEvent>
            where TEvent : IMessageEvent
        {
            private readonly Func<TEvent, Task> _handler;

            public AsyncDelegateEventHandler(Func<TEvent, Task> handler)
            {
                _handler = handler;
            }

            public Task HandleAsync(TEvent @event)
            {
                return _handler(@event);
            }
        }

        private class QueryHandler<TQuery, TQueryResult> : IMessageQueryHandler<TQuery, TQueryResult>
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            private readonly Func<TQuery, TQueryResult> _handler;

            public QueryHandler(Func<TQuery, TQueryResult> handler)
            {
                _handler = handler;
            }

            public TQueryResult Handle(TQuery query)
            {
                return _handler(query);
            }
        }

        private class AsyncQueryHandler<TQuery, TQueryResult> : IAsyncMessageQueryHandler<TQuery, TQueryResult>
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            private readonly Func<TQuery, Task<TQueryResult>> _handler;

            public AsyncQueryHandler(Func<TQuery, Task<TQueryResult>> handler)
            {
                _handler = handler;
            }

            public Task<TQueryResult> HandleAsync(TQuery query)
            {
                return _handler(query);
            }
        }

        private class RpcHandler<TRcp, TRpcResult> : IMessageRpcHandler<TRcp, TRpcResult>
            where TRcp : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            private readonly Func<TRcp, TRpcResult> _handler;

            public RpcHandler(Func<TRcp, TRpcResult> handler)
            {
                _handler = handler;
            }

            public TRpcResult Handle(TRcp query)
            {
                return _handler(query);
            }
        }

        private class AsyncRpcHandler<TRpc, TRpcResult> : IAsyncMessageRpcHandler<TRpc, TRpcResult>
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            private readonly Func<TRpc, Task<TRpcResult>> _handler;

            public AsyncRpcHandler(Func<TRpc, Task<TRpcResult>> handler)
            {
                _handler = handler;
            }

            public Task<TRpcResult> HandleAsync(TRpc query)
            {
                return _handler(query);
            }
        }

        private class CommandHandler<TCommand> : IMessageCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly Action<TCommand> _handler;

            public CommandHandler(Action<TCommand> handler)
            {
                _handler = handler;
            }

            public void Handle(TCommand query)
            {
                _handler(query);
            }
        }

        private class AsyncCommandHandler<TCommand> : IAsyncMessageCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly Func<TCommand, Task> _handler;

            public AsyncCommandHandler(Func<TCommand, Task> handler)
            {
                _handler = handler;
            }

            public Task HandleAsync(TCommand query)
            {
                return _handler(query);
            }
        }
    }
}
