using System;
using System.Threading.Tasks;
using MessageBus.Decorators;

namespace MessageBus
{
    public static class OnlyHandleWhenMessageHandlerExtensions
    {
        /// <summary>
        /// Limit the underlying <paramref name="handler"/> to only receive messages when the provided <paramref name="precondition"/>
        /// returns true.
        /// </summary>
        public static IAsyncMessageEventHandler<TEvent> OnlyWhen<TEvent>(this IAsyncMessageEventHandler<TEvent> handler, Func<TEvent, bool> precondition)
            where TEvent : IMessageEvent
        {
            if (precondition is null) throw new ArgumentNullException(nameof(precondition));
            return new FilteredAsyncEventHandler<TEvent>(handler, precondition);
        }

        /// <summary>
        /// Limit the underlying <paramref name="handler"/> to only receive messages when the provided async <paramref name="precondition"/>
        /// returns true.
        /// </summary>
        public static IAsyncMessageEventHandler<TEvent> OnlyWhen<TEvent>(this IAsyncMessageEventHandler<TEvent> handler, Func<TEvent, Task<bool>> precondition)
            where TEvent : IMessageEvent
        {
            if (precondition is null) throw new ArgumentNullException(nameof(precondition));
            return new AsyncFilteredAsyncEventHandler<TEvent>(handler, precondition);
        }

        /// <summary>
        /// Limit the underlying <paramref name="handler"/> to only receive messages when the provided <paramref name="precondition"/>
        /// returns true.
        /// </summary>
        public static IMessageEventHandler<TEvent> OnlyWhen<TEvent>(this IMessageEventHandler<TEvent> handler, Func<TEvent, bool> precondition)
            where TEvent : IMessageEvent
        {
            if (precondition is null) throw new ArgumentNullException(nameof(precondition));
            return new FilteredEventHandler<TEvent>(handler, precondition);
        }

        private sealed class AsyncFilteredAsyncEventHandler<TEvent> : DecoratedAsyncEventHandler<TEvent>
            where TEvent : IMessageEvent
        {
            private readonly Func<TEvent, Task<bool>> _precondition;

            public AsyncFilteredAsyncEventHandler(IAsyncMessageEventHandler<TEvent> inner, Func<TEvent, Task<bool>> precondition)
                : base(inner)
            {
                _precondition = precondition;
            }

            public override async Task HandleAsync(TEvent @event)
            {
                if (await _precondition(@event).ConfigureAwait(false))
                    await base.HandleAsync(@event).ConfigureAwait(false);
            }
        }

        private sealed class FilteredAsyncEventHandler<TEvent> : DecoratedAsyncEventHandler<TEvent>
            where TEvent : IMessageEvent
        {
            private readonly Func<TEvent, bool> _precondition;

            public FilteredAsyncEventHandler(IAsyncMessageEventHandler<TEvent> inner, Func<TEvent, bool> precondition)
                : base(inner)
            {
                _precondition = precondition;
            }

            public override Task HandleAsync(TEvent @event)
            {
                if (!_precondition(@event))
                    return Task.CompletedTask;
                return base.HandleAsync(@event);
            }
        }

        private sealed class FilteredEventHandler<TEvent> : DecoratedEventHandler<TEvent>
            where TEvent : IMessageEvent
        {
            private readonly Func<TEvent, bool> _precondition;

            public FilteredEventHandler(IMessageEventHandler<TEvent> inner, Func<TEvent, bool> precondition)
                : base(inner)
            {
                _precondition = precondition;
            }

            public override void Handle(TEvent @event)
            {
                if (!_precondition(@event))
                    return;
                base.Handle(@event);
            }
        }
    }
}
