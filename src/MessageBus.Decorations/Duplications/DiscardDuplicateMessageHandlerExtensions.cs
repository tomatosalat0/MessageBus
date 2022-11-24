using System.Threading.Tasks;
using MessageBus.Decorations.Duplications;
using MessageBus.Decorators;

namespace MessageBus
{
    public static class DiscardDuplicateMessageHandlerExtensions
    {
        /// <summary>
        /// Returns a new async handler which will detect already received messages. Already received message will get dropped silently. The default
        /// duplicate detection implementation will get used.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageEventHandler<TEvent> WithDuplicateMessageDetection<TEvent>(this IAsyncMessageEventHandler<TEvent> handler)
            where TEvent : IMessageEvent
        {
            return handler.WithDuplicateMessageDetection(new MemoryDuplicateMessageDetection());
        }

        /// <summary>
        /// Returns a new async handler which will detect already received messages. Already received message will get dropped silently. The provided
        /// <paramref name="duplicateDetection"/> will get used to detect duplicates.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageEventHandler<TEvent> WithDuplicateMessageDetection<TEvent>(this IAsyncMessageEventHandler<TEvent> handler, IDuplicateDetection duplicateDetection)
            where TEvent : IMessageEvent
        {
            return new DuplicateAsyncEventHandler<TEvent>(handler, duplicateDetection);
        }

        /// <summary>
        /// Returns a new async handler which will detect already received messages. Already received message will get dropped silently. The default
        /// duplicate detection implementation will get used.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageEventHandler<TEvent> WithDuplicateMessageDetection<TEvent>(this IMessageEventHandler<TEvent> handler)
            where TEvent : IMessageEvent
        {
            return handler.WithDuplicateMessageDetection(new MemoryDuplicateMessageDetection());
        }

        /// <summary>
        /// Returns a new async handler which will detect already received messages. Already received message will get dropped silently. The provided
        /// <paramref name="duplicateDetection"/> will get used to detect duplicates.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageEventHandler<TEvent> WithDuplicateMessageDetection<TEvent>(this IMessageEventHandler<TEvent> handler, IDuplicateDetection duplicateDetection)
            where TEvent : IMessageEvent
        {
            return new DuplicateEventHandler<TEvent>(handler, duplicateDetection);
        }

        /// <summary>
        /// Returns a new async handler which will detect already received messages. Already received message will get dropped silently. The default
        /// duplicate detection implementation will get used.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageCommandHandler<TCommand> WithDuplicateMessageDetection<TCommand>(this IAsyncMessageCommandHandler<TCommand> handler)
            where TCommand : IMessageCommand
        {
            return handler.WithDuplicateMessageDetection(new MemoryDuplicateMessageDetection());
        }

        /// <summary>
        /// Returns a new async handler which will detect already received messages. Already received message will get dropped silently. The provided
        /// <paramref name="duplicateDetection"/> will get used to detect duplicates.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageCommandHandler<TCommand> WithDuplicateMessageDetection<TCommand>(this IAsyncMessageCommandHandler<TCommand> handler, IDuplicateDetection duplicateDetection)
            where TCommand : IMessageCommand
        {
            return new DuplicateAsyncCommandHandler<TCommand>(handler, duplicateDetection);
        }

        /// <summary>
        /// Returns a new handler which will detect already received messages. Already received message will get dropped silently. The default
        /// duplicate detection implementation will get used.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageCommandHandler<TCommand> WithDuplicateMessageDetection<TCommand>(this IMessageCommandHandler<TCommand> handler)
            where TCommand : IMessageCommand
        {
            return handler.WithDuplicateMessageDetection(new MemoryDuplicateMessageDetection());
        }

        /// <summary>
        /// Returns a new handler which will detect already received messages. Already received message will silently get dropped. The provided
        /// <paramref name="duplicateDetection"/> will get used to detect duplicates.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageCommandHandler<TCommand> WithDuplicateMessageDetection<TCommand>(this IMessageCommandHandler<TCommand> handler, IDuplicateDetection duplicateDetection)
            where TCommand : IMessageCommand
        {
            return new DuplicateCommandHandler<TCommand>(handler, duplicateDetection);
        }

        private sealed class DuplicateAsyncEventHandler<TEvent> : DecoratedAsyncEventHandler<TEvent>
            where TEvent : IMessageEvent
        {
            private readonly IDuplicateDetection _duplicationDetection;

            public DuplicateAsyncEventHandler(IAsyncMessageEventHandler<TEvent> inner, IDuplicateDetection duplicationDetection)
                : base(inner)
            {
                _duplicationDetection = duplicationDetection;
            }

            public override async Task HandleAsync(TEvent @event)
            {
                if (!_duplicationDetection.HandleReceivedMessage(@event.MessageId))
                    return;

                try
                {
                    await base.HandleAsync(@event).ConfigureAwait(false);
                }
                catch (System.Exception)
                {
                    _duplicationDetection.ForgetMessage(@event.MessageId);
                    throw;
                }
            }
        }

        private sealed class DuplicateEventHandler<TEvent> : DecoratedEventHandler<TEvent>
            where TEvent : IMessageEvent
        {
            private readonly IDuplicateDetection _duplicationDetection;

            public DuplicateEventHandler(IMessageEventHandler<TEvent> inner, IDuplicateDetection duplicationDetection)
                : base(inner)
            {
                _duplicationDetection = duplicationDetection;
            }

            public override void Handle(TEvent @event)
            {
                if (!_duplicationDetection.HandleReceivedMessage(@event.MessageId))
                    return;

                try
                {
                    base.Handle(@event);
                }
                catch (System.Exception)
                {
                    _duplicationDetection.ForgetMessage(@event.MessageId);
                    throw;
                }
            }
        }

        private sealed class DuplicateAsyncCommandHandler<TCommand> : DecoratedAsyncCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly IDuplicateDetection _duplicationDetection;

            public DuplicateAsyncCommandHandler(IAsyncMessageCommandHandler<TCommand> inner, IDuplicateDetection duplicationDetection)
                : base(inner)
            {
                _duplicationDetection = duplicationDetection;
            }

            public override async Task HandleAsync(TCommand command)
            {
                if (!_duplicationDetection.HandleReceivedMessage(command.MessageId))
                    return;

                try
                {
                    await base.HandleAsync(command).ConfigureAwait(false);
                }
                catch (System.Exception)
                {
                    _duplicationDetection.ForgetMessage(command.MessageId);
                    throw;
                }
            }
        }

        private sealed class DuplicateCommandHandler<TCommand> : DecoratedCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly IDuplicateDetection _duplicationDetection;

            public DuplicateCommandHandler(IMessageCommandHandler<TCommand> inner, IDuplicateDetection duplicationDetection)
                : base(inner)
            {
                _duplicationDetection = duplicationDetection;
            }

            public override void Handle(TCommand command)
            {
                if (!_duplicationDetection.HandleReceivedMessage(command.MessageId))
                    return;

                try
                {
                    base.Handle(command);
                }
                catch (System.Exception)
                {
                    _duplicationDetection.ForgetMessage(command.MessageId);
                    throw;
                }
            }
        }
    }
}
