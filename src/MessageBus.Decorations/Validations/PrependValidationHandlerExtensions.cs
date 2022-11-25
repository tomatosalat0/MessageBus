using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessageBus.Decorations.Validations;
using MessageBus.Decorators;

namespace MessageBus
{
    public static class PrependValidationHandlerExtensions
    {
        /// <summary>
        /// Prepends the provided async <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IAsyncMessageCommandHandler<TCommand> WithValidation<TCommand>(
            this IAsyncMessageCommandHandler<TCommand> handler,
            params IAsyncMessageValidator<TCommand>[] validators)
            where TCommand : IMessageCommand
        {
            if (validators.Length == 0)
                return handler;

            return new AsyncMessageCommandAsyncValidationDecorator<TCommand>(validators, handler);
        }

        /// <summary>
        /// Prepends the provided <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IAsyncMessageCommandHandler<TCommand> WithValidation<TCommand>(
            this IAsyncMessageCommandHandler<TCommand> handler,
            params IMessageValidator<TCommand>[] validators)
            where TCommand : IMessageCommand
        {
            if (validators.Length == 0)
                return handler;

            return new AsyncMessageCommandValidationDecorator<TCommand>(validators, handler);
        }

        /// <summary>
        /// Prepends the provided <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IMessageCommandHandler<TCommand> WithValidation<TCommand>(
            this IMessageCommandHandler<TCommand> handler,
            params IMessageValidator<TCommand>[] validators)
            where TCommand : IMessageCommand
        {
            if (validators.Length == 0)
                return handler;

            return new MessageCommandValidationDecorator<TCommand>(validators, handler);
        }

        /// <summary>
        /// Prepends the provided <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IAsyncMessageQueryHandler<TQuery, TQueryResult> WithValidation<TQuery, TQueryResult>(
            this IAsyncMessageQueryHandler<TQuery, TQueryResult> handler,
            params IAsyncMessageValidator<TQuery>[] validators)
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            if (validators.Length == 0)
                return handler;

            return new AsyncMessageQueryAsyncValidationDecorator<TQuery, TQueryResult>(validators, handler);
        }

        /// <summary>
        /// Prepends the provided <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IAsyncMessageQueryHandler<TQuery, TQueryResult> WithValidation<TQuery, TQueryResult>(
            this IAsyncMessageQueryHandler<TQuery, TQueryResult> handler,
            params IMessageValidator<TQuery>[] validators)
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            if (validators.Length == 0)
                return handler;

            return new AsyncMessageQueryValidationDecorator<TQuery, TQueryResult>(validators, handler);
        }

        /// <summary>
        /// Prepends the provided <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IMessageQueryHandler<TQuery, TQueryResult> WithValidation<TQuery, TQueryResult>(
            this IMessageQueryHandler<TQuery, TQueryResult> handler,
            params IMessageValidator<TQuery>[] validators)
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            if (validators.Length == 0)
                return handler;

            return new MessageQueryValidationDecorator<TQuery, TQueryResult>(validators, handler);
        }

        /// <summary>
        /// Prepends the provided <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IAsyncMessageRpcHandler<TRpc, TRpcResult> WithValidation<TRpc, TRpcResult>(
            this IAsyncMessageRpcHandler<TRpc, TRpcResult> handler,
            params IAsyncMessageValidator<TRpc>[] validators)
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            if (validators.Length == 0)
                return handler;

            return new AsyncMessageRpcAsyncValidationDecorator<TRpc, TRpcResult>(validators, handler);
        }

        /// <summary>
        /// Prepends the provided <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IAsyncMessageRpcHandler<TRpc, TRpcResult> WithValidation<TRpc, TRpcResult>(
            this IAsyncMessageRpcHandler<TRpc, TRpcResult> handler,
            params IMessageValidator<TRpc>[] validators)
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            if (validators.Length == 0)
                return handler;

            return new AsyncMessageRpcValidationDecorator<TRpc, TRpcResult>(validators, handler);
        }

        /// <summary>
        /// Prepends the provided <paramref name="validators"/> to the current <paramref name="handler"/>. These validators
        /// will get executed before the underlying <paramref name="handler"/> will get called. If any validator
        /// indicates an error, an <see cref="MessageValidationException"/> will get thrown and the underlying handler
        /// won't get executed.
        /// </summary>
        /// <remarks>If the list of <paramref name="validators"/> is empty, the provided <paramref name="handler"/> will get returned.</remarks>
        public static IMessageRpcHandler<TRpc, TRpcResult> WithValidation<TRpc, TRpcResult>(
            this IMessageRpcHandler<TRpc, TRpcResult> handler,
            params IMessageValidator<TRpc>[] validators)
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            if (validators.Length == 0)
                return handler;

            return new MessageRpcValidationDecorator<TRpc, TRpcResult>(validators, handler);
        }

        private sealed class AsyncMessageCommandAsyncValidationDecorator<TCommand> : DecoratedAsyncCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly IReadOnlyList<IAsyncMessageValidator<TCommand>> _validators;

            public AsyncMessageCommandAsyncValidationDecorator(IReadOnlyList<IAsyncMessageValidator<TCommand>> validators, IAsyncMessageCommandHandler<TCommand> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override async Task HandleAsync(TCommand command)
            {
                await ValidateAndThrowAsync(command, _validators).ConfigureAwait(false);
                await base.HandleAsync(command).ConfigureAwait(false);
            }
        }

        private sealed class AsyncMessageCommandValidationDecorator<TCommand> : DecoratedAsyncCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly IReadOnlyList<IMessageValidator<TCommand>> _validators;

            public AsyncMessageCommandValidationDecorator(IReadOnlyList<IMessageValidator<TCommand>> validators, IAsyncMessageCommandHandler<TCommand> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override Task HandleAsync(TCommand command)
            {
                ValidateAndThrow(command, _validators);
                return base.HandleAsync(command);
            }
        }

        private sealed class MessageCommandValidationDecorator<TCommand> : DecoratedCommandHandler<TCommand>
            where TCommand : IMessageCommand
        {
            private readonly IReadOnlyList<IMessageValidator<TCommand>> _validators;

            public MessageCommandValidationDecorator(IReadOnlyList<IMessageValidator<TCommand>> validators, IMessageCommandHandler<TCommand> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override void Handle(TCommand command)
            {
                ValidateAndThrow(command, _validators);
                base.Handle(command);
            }
        }

        private sealed class AsyncMessageQueryAsyncValidationDecorator<TQuery, TQueryResult> : DecoratedAsyncQueryHandler<TQuery, TQueryResult>
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            private readonly IReadOnlyList<IAsyncMessageValidator<TQuery>> _validators;

            public AsyncMessageQueryAsyncValidationDecorator(IReadOnlyList<IAsyncMessageValidator<TQuery>> validators, IAsyncMessageQueryHandler<TQuery, TQueryResult> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override async Task<TQueryResult> HandleAsync(TQuery query)
            {
                await ValidateAndThrowAsync(query, _validators).ConfigureAwait(false);
                return await base.HandleAsync(query).ConfigureAwait(false);
            }
        }

        private sealed class AsyncMessageQueryValidationDecorator<TQuery, TQueryResult> : DecoratedAsyncQueryHandler<TQuery, TQueryResult>
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            private readonly IReadOnlyList<IMessageValidator<TQuery>> _validators;

            public AsyncMessageQueryValidationDecorator(IReadOnlyList<IMessageValidator<TQuery>> validators, IAsyncMessageQueryHandler<TQuery, TQueryResult> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override Task<TQueryResult> HandleAsync(TQuery query)
            {
                ValidateAndThrow(query, _validators);
                return base.HandleAsync(query);
            }
        }

        private sealed class MessageQueryValidationDecorator<TQuery, TQueryResult> : DecoratedQueryHandler<TQuery, TQueryResult>
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            private readonly IReadOnlyList<IMessageValidator<TQuery>> _validators;

            public MessageQueryValidationDecorator(IReadOnlyList<IMessageValidator<TQuery>> validators, IMessageQueryHandler<TQuery, TQueryResult> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override TQueryResult Handle(TQuery query)
            {
                ValidateAndThrow(query, _validators);
                return base.Handle(query);
            }
        }

        private sealed class AsyncMessageRpcAsyncValidationDecorator<TRpc, TRpcResult> : DecoratedAsyncRpcHandler<TRpc, TRpcResult>
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            private readonly IReadOnlyList<IAsyncMessageValidator<TRpc>> _validators;

            public AsyncMessageRpcAsyncValidationDecorator(IReadOnlyList<IAsyncMessageValidator<TRpc>> validators, IAsyncMessageRpcHandler<TRpc, TRpcResult> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override async Task<TRpcResult> HandleAsync(TRpc query)
            {
                await ValidateAndThrowAsync(query, _validators).ConfigureAwait(false);
                return await base.HandleAsync(query).ConfigureAwait(false);
            }
        }

        private sealed class AsyncMessageRpcValidationDecorator<TRpc, TRpcResult> : DecoratedAsyncRpcHandler<TRpc, TRpcResult>
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            private readonly IReadOnlyList<IMessageValidator<TRpc>> _validators;

            public AsyncMessageRpcValidationDecorator(IReadOnlyList<IMessageValidator<TRpc>> validators, IAsyncMessageRpcHandler<TRpc, TRpcResult> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override Task<TRpcResult> HandleAsync(TRpc query)
            {
                ValidateAndThrow(query, _validators);
                return base.HandleAsync(query);
            }
        }

        private sealed class MessageRpcValidationDecorator<TRpc, TRpcResult> : DecoratedRpcHandler<TRpc, TRpcResult>
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            private readonly IReadOnlyList<IMessageValidator<TRpc>> _validators;

            public MessageRpcValidationDecorator(IReadOnlyList<IMessageValidator<TRpc>> validators, IMessageRpcHandler<TRpc, TRpcResult> inner)
                : base(inner)
            {
                _validators = validators;
            }

            public override TRpcResult Handle(TRpc query)
            {
                ValidateAndThrow(query, _validators);
                return base.Handle(query);
            }
        }

        private static async Task ValidateAndThrowAsync<TMessage>(TMessage message, IReadOnlyList<IAsyncMessageValidator<TMessage>> validators)
        {
            IReadOnlyList<IValidationError> errors =
                (await Task.WhenAll(validators.Select(p => p.ValidateAsync(message))).ConfigureAwait(false))
                .SelectMany(p => p.Errors)
                .ToArray();
            ThrowOnError(errors);
        }

        private static void ValidateAndThrow<TMessage>(TMessage message, IReadOnlyList<IMessageValidator<TMessage>> validators)
        {
            IReadOnlyList<IValidationError> errors = validators
                .Select(p => p.Validate(message))
                .SelectMany(p => p.Errors)
                .ToArray();
            ThrowOnError(errors);
        }

        private static void ThrowOnError(IReadOnlyList<IValidationError> errors)
        {
            if (errors.Count > 0)
                throw new MessageValidationException(string.Join('\n', errors.Select(p => p.ErrorMessage)));
        }
    }
}
