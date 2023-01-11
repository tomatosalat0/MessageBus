using System;
using System.Threading.Tasks;
using MessageBus.Decorators;

namespace MessageBus
{
    public static class AutomaticRetryMessageHandlerExtensions
    {
        /// <summary>
        /// Returns a new handler which will detect capture exceptions thrown by previous <paramref name="handler"/>.
        /// If an exception occurs, the command will get scheduled again. Note that there is no maximum retry count.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageCommandHandler<TCommand> WithRetryOnException<TCommand>(this IAsyncMessageCommandHandler<TCommand> handler)
            where TCommand : IMessageCommand
        {
            return handler.WithRetryOnException(AlwaysRetry);
        }

        /// <summary>
        /// Returns a new handler which will detect capture exceptions thrown by previous <paramref name="handler"/>.
        /// If an exception occurs and the provided <paramref name="shouldRetry"/> function returns true, the command will get scheduled again.
        /// Note that there is no maximum retry count.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IAsyncMessageCommandHandler<TCommand> WithRetryOnException<TCommand>(this IAsyncMessageCommandHandler<TCommand> handler, Func<Exception, bool> shouldRetry)
            where TCommand : IMessageCommand
        {
            return new RetryAsyncCommandHandler<TCommand>(handler, shouldRetry);
        }

        /// <summary>
        /// Returns a new handler which will detect capture exceptions thrown by previous <paramref name="handler"/>.
        /// If an exception occurs, the command will get scheduled again. Note that there is no maximum retry count.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageCommandHandler<TCommand> WithRetryOnException<TCommand>(this IMessageCommandHandler<TCommand> handler)
            where TCommand : IMessageCommand
        {
            return handler.WithRetryOnException(AlwaysRetry);
        }

        /// <summary>
        /// Returns a new handler which will detect capture exceptions thrown by previous <paramref name="handler"/>.
        /// If an exception occurs and the provided <paramref name="shouldRetry"/> function returns true, the command will get scheduled again.
        /// Note that there is no maximum retry count.
        /// </summary>
        /// <remarks>If the provided <paramref name="handler"/> implements <see cref="ISubscriptionAwareHandler"/>, the result
        /// will implement that interface as well.</remarks>
        public static IMessageCommandHandler<TCommand> WithRetryOnException<TCommand>(this IMessageCommandHandler<TCommand> handler, Func<Exception, bool> shouldRetry)
            where TCommand : IMessageCommand
        {
            return new RetryCommandHandler<TCommand>(handler, shouldRetry);
        }

        private sealed class RetryAsyncCommandHandler<TCommand> : DecoratedAsyncCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly Func<Exception, bool> _shouldRetry;

            public RetryAsyncCommandHandler(IAsyncMessageCommandHandler<TCommand> inner, Func<Exception, bool> shouldRetry)
                : base(inner)
            {
                _shouldRetry = shouldRetry;
            }

            public override async Task HandleAsync(TCommand command)
            {
                try
                {
                    await base.HandleAsync(command).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    if (ex is HandlerUnavailableException || !_shouldRetry(ex))
                        throw;

                    throw new HandlerUnavailableException(ex.Message, ex);
                }
            }
        }

        private sealed class RetryCommandHandler<TCommand> : DecoratedCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly Func<Exception, bool> _shouldRetry;

            public RetryCommandHandler(IMessageCommandHandler<TCommand> inner, Func<Exception, bool> shouldRetry)
                : base(inner)
            {
                _shouldRetry = shouldRetry;
            }

            public override void Handle(TCommand command)
            {
                try
                {
                    base.Handle(command);
                }
                catch (Exception ex)
                {
                    if (ex is HandlerUnavailableException || !_shouldRetry(ex))
                        throw;

                    throw new HandlerUnavailableException(ex.Message, ex);
                }
            }
        }

        private static bool AlwaysRetry(Exception ex) => true;
    }
}
