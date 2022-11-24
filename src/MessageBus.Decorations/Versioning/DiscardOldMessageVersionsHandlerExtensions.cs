using System;
using System.Threading.Tasks;
using MessageBus.Decorations.Versioning;
using MessageBus.Decorators;

namespace MessageBus
{
    public static class DiscardOldMessageVersionsHandlerExtensions
    {
        /// <summary>
        /// Returns a new async handler which will detect outdated messages. Messages which have a lower or equal version compared to the latest one
        /// will get dropped silently. The default version handler will get used.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageEventHandler<TEvent> WithDiscardOldMessageVersion<TEvent, TVersionIndicator>(this IAsyncMessageEventHandler<TEvent> handler)
            where TEvent : IMessageEvent, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            return handler.WithDiscardOldMessageVersion(new MemoryMessageVersionDetection<TVersionIndicator>());
        }

        /// <summary>
        /// Returns a new async handler which will detect outdated messages. Messages which have a lower or equal version compared to the latest one
        /// will get dropped silently. The provided <paramref name="versionDetection"/> will get used to detect outdated messages.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageEventHandler<TEvent> WithDiscardOldMessageVersion<TEvent, TVersionIndicator>(this IAsyncMessageEventHandler<TEvent> handler, IVersionDetection<TVersionIndicator> versionDetection)
            where TEvent : IMessageEvent, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            return new VersionAsyncEventHandler<TEvent, TVersionIndicator>(handler, versionDetection);
        }

        /// <summary>
        /// Returns a new handler which will detect outdated messages. Messages which have a lower or equal version compared to the latest one
        /// will get dropped silently. The default version handler will get used.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageEventHandler<TEvent> WithDiscardOldMessageVersion<TEvent, TVersionIndicator>(this IMessageEventHandler<TEvent> handler)
            where TEvent : IMessageEvent, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            return handler.WithDiscardOldMessageVersion(new MemoryMessageVersionDetection<TVersionIndicator>());
        }

        /// <summary>
        /// Returns a new handler which will detect outdated messages. Messages which have a lower or equal version compared to the latest one
        /// will get dropped silently. The provided <paramref name="versionDetection"/> will get used to detect outdated messages.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageEventHandler<TEvent> WithDiscardOldMessageVersion<TEvent, TVersionIndicator>(this IMessageEventHandler<TEvent> handler, IVersionDetection<TVersionIndicator> versionDetection)
            where TEvent : IMessageEvent, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            return new VersionEventHandler<TEvent, TVersionIndicator>(handler, versionDetection);
        }

        /// <summary>
        /// Returns a new async handler which will detect outdated messages. Messages which have a lower or equal version compared to the latest one
        /// will get dropped silently. The default version handler will get used.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageCommandHandler<TCommand> WithDiscardOldMessageVersion<TCommand, TVersionIndicator>(this IAsyncMessageCommandHandler<TCommand> handler)
            where TCommand : IMessageCommand, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            return handler.WithDiscardOldMessageVersion(new MemoryMessageVersionDetection<TVersionIndicator>());
        }

        /// <summary>
        /// Returns a new async handler which will detect outdated messages. Messages which have a lower or equal version compared to the latest one
        /// will get dropped silently. The provided <paramref name="versionDetection"/> will get used to detect outdated messages.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageCommandHandler<TCommand> WithDiscardOldMessageVersion<TCommand, TVersionIndicator>(this IAsyncMessageCommandHandler<TCommand> handler, IVersionDetection<TVersionIndicator> versionDetection)
            where TCommand : IMessageCommand, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            return new VersionAsyncCommandHandler<TCommand, TVersionIndicator>(handler, versionDetection);
        }

        /// <summary>
        /// Returns a new handler which will detect outdated messages. Messages which have a lower or equal version compared to the latest one
        /// will get dropped silently. The default version handler will get used.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageCommandHandler<TCommand> WithDiscardOldMessageVersion<TCommand, TVersionIndicator>(this IMessageCommandHandler<TCommand> handler)
            where TCommand : IMessageCommand, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            return handler.WithDiscardOldMessageVersion(new MemoryMessageVersionDetection<TVersionIndicator>());
        }

        /// <summary>
        /// Returns a new handler which will detect outdated messages. Messages which have a lower or equal version compared to the latest one
        /// will get dropped silently. The provided <paramref name="versionDetection"/> will get used to detect outdated messages.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageCommandHandler<TCommand> WithDiscardOldMessageVersion<TCommand, TVersionIndicator>(this IMessageCommandHandler<TCommand> handler, IVersionDetection<TVersionIndicator> versionDetection)
            where TCommand : IMessageCommand, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            return new VersionCommandHandler<TCommand, TVersionIndicator>(handler, versionDetection);
        }

        private sealed class VersionAsyncEventHandler<TEvent, TVersionIndicator> : DecoratedAsyncEventHandler<TEvent>
            where TEvent : IMessageEvent, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            private readonly IVersionDetection<TVersionIndicator> _versionDetection;

            public VersionAsyncEventHandler(IAsyncMessageEventHandler<TEvent> inner, IVersionDetection<TVersionIndicator> versionDetection)
                : base(inner)
            {
                _versionDetection = versionDetection;
            }

            public override async Task HandleAsync(TEvent @event)
            {
                if (!_versionDetection.HandleMessageVersion(@event.MessageVersion))
                    return;

                try
                {
                    await base.HandleAsync(@event).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    _versionDetection.ForgetVersion(@event.MessageVersion);
                    throw;
                }
            }
        }

        private sealed class VersionEventHandler<TEvent, TVersionIndicator> : DecoratedEventHandler<TEvent>
            where TEvent : IMessageEvent, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            private readonly IVersionDetection<TVersionIndicator> _versionDetection;

            public VersionEventHandler(IMessageEventHandler<TEvent> inner, IVersionDetection<TVersionIndicator> versionDetection)
                : base(inner)
            {
                _versionDetection = versionDetection;
            }

            public override void Handle(TEvent @event)
            {
                if (!_versionDetection.HandleMessageVersion(@event.MessageVersion))
                    return;

                try
                {
                    base.Handle(@event);
                }
                catch (System.Exception)
                {
                    _versionDetection.ForgetVersion(@event.MessageVersion);
                    throw;
                }
            }
        }

        private sealed class VersionAsyncCommandHandler<TCommand, TVersionIndicator> : DecoratedAsyncCommandHandler<TCommand>
            where TCommand : IMessageCommand, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            private readonly IVersionDetection<TVersionIndicator> _versionDetection;

            public VersionAsyncCommandHandler(IAsyncMessageCommandHandler<TCommand> inner, IVersionDetection<TVersionIndicator> versionDetection)
                : base(inner)
            {
                _versionDetection = versionDetection;
            }

            public override async Task HandleAsync(TCommand command)
            {
                if (!_versionDetection.HandleMessageVersion(command.MessageVersion))
                    return;

                try
                {
                    await base.HandleAsync(command).ConfigureAwait(false);
                }
                catch (System.Exception)
                {
                    _versionDetection.ForgetVersion(command.MessageVersion);
                    throw;
                }
            }
        }

        private sealed class VersionCommandHandler<TCommand, TVersionIndicator> : DecoratedCommandHandler<TCommand>
            where TCommand : IMessageCommand, IHasMessageVersion<TVersionIndicator>
            where TVersionIndicator : IComparable<TVersionIndicator>
        {
            private readonly IVersionDetection<TVersionIndicator> _versionDetection;

            public VersionCommandHandler(IMessageCommandHandler<TCommand> inner, IVersionDetection<TVersionIndicator> versionDetection)
                : base(inner)
            {
                _versionDetection = versionDetection;
            }

            public override void Handle(TCommand command)
            {
                if (!_versionDetection.HandleMessageVersion(command.MessageVersion))
                    return;

                try
                {
                    base.Handle(command);
                }
                catch (System.Exception)
                {
                    _versionDetection.ForgetVersion(command.MessageVersion);
                    throw;
                }
            }
        }
    }
}
