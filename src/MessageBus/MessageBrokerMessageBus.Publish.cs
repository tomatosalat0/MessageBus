using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MessageBus.Messaging;

namespace MessageBus
{
    /// <summary>
    /// This part of the event bus is responsible for the <see cref="IMessageBusPublishing"/>
    /// implementation.
    /// </summary>
    public sealed partial class MessageBrokerMessageBus : IMessageBusPublishing, IDisposable
    {
        /// <inheritdoc/>
        public Task FireEvent<TEvent>(TEvent @event)
            where TEvent : IMessageEvent
        {
            ThrowDisposed();
            return _broker.PublishEvent(@event, GetTopicNameFromType(typeof(TEvent)));
        }

        /// <inheritdoc/>
        public Task FireCommand<TCommand>(TCommand command)
            where TCommand : IMessageCommand
        {
            ThrowDisposed();
            return _broker.PublishCommand(command, GetTopicNameFromType(typeof(TCommand)));
        }

        /// <inheritdoc/>
        public Task FireCommandAndWait<TCommand>(TCommand command, CancellationToken cancellationToken)
            where TCommand : IMessageCommand
        {
            ThrowDisposed();

            TopicName fireTopic = GetTopicNameFromType(typeof(TCommand));
            TopicName successTopic = GetSuccessOutcomeTopicName(command);
            TopicName failureTopic = GetFailureOutcomeTopicName(command);

            return FireAndAwait<TCommand, Nothing, IHasMessageId, MessageCommandOutcome>(
                fireTopic,
                successTopic,
                failureTopic,
                command,
                (outcome) =>
                {
                    if (outcome.HasFailed)
                    {
                        if (outcome.FailureMessage.FailureDetails is not null)
                            throw new MessageOperationFailedException(outcome.FailureMessage.FailureDetails);
                        else
                            throw new MessageOperationFailedException($"The command '{typeof(TCommand).Name}' failed without any failure details");
                    }

                    return new Nothing();
                },
                cancellationToken
            );
        }

        /// <inheritdoc/>
        public Task<TQueryResult> FireQuery<TQuery, TQueryResult>(TQuery query, CancellationToken cancellationToken)
            where TQuery : IMessageQuery<TQueryResult>
            where TQueryResult : IMessageQueryResult
        {
            ThrowDisposed();

            TopicName fireTopic = GetTopicNameFromType(typeof(TQuery));
            TopicName successTopic = GetSuccessOutcomeTopicName(query);
            TopicName failureTopic = GetFailureOutcomeTopicName(query);

            return FireAndAwait<TQuery, TQueryResult, TQueryResult, IMessageQueryResult.FailureResult>(
                fireTopic,
                successTopic,
                failureTopic,
                query,
                (outcome) =>
                {
                    if (outcome.HasFailed)
                    {
                        throw new MessageOperationFailedException(outcome.FailureMessage.Details);
                    }

                    return outcome.SuccessMessage;
                },
                cancellationToken
            );
        }

        /// <inheritdoc/>
        public Task<TRpcResult> FireRpc<TRpc, TRpcResult>(TRpc rpcParameter, CancellationToken cancellationToken)
            where TRpc : IMessageRpc<TRpcResult>
            where TRpcResult : IMessageRpcResult
        {
            ThrowDisposed();

            TopicName fireTopic = GetTopicNameFromType(typeof(TRpc));
            TopicName successTopic = GetSuccessOutcomeTopicName(rpcParameter);
            TopicName failureTopic = GetFailureOutcomeTopicName(rpcParameter);

            return FireAndAwait<TRpc, TRpcResult, TRpcResult, IMessageRpcResult.FailureResult>(
                fireTopic,
                successTopic,
                failureTopic,
                rpcParameter,
                (outcome) =>
                {
                    if (outcome.HasFailed)
                    {
                        throw new MessageOperationFailedException(outcome.FailureMessage.Details);
                    }

                    return outcome.SuccessMessage;
                },
                cancellationToken
            );
        }

        private async Task<TOutcome> FireAndAwait<TRequest, TOutcome, TSuccess, TFailure>(
            TopicName fireTopic,
            TopicName successTopic,
            TopicName failureTopic,
            TRequest request,
            Func<ResponseOutcome<TSuccess, TFailure>, TOutcome> transformResult,
            CancellationToken cancellationToken)
            where TRequest : IHasMessageId
            where TSuccess : IHasMessageId
            where TFailure : IHasMessageId
        {
            var channel = System.Threading.Channels.Channel.CreateUnbounded<ResponseOutcome<TSuccess, TFailure>>(new System.Threading.Channels.UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = false
            });

            using var successSubscription = RegisterCompleteHandler<TSuccess>(successTopic, (o) => channel.Writer.TryWrite(ResponseOutcome<TSuccess, TFailure>.Success(o)));
            using var failureSubscription = RegisterCompleteHandler<TFailure>(failureTopic, (o) => channel.Writer.TryWrite(ResponseOutcome<TSuccess, TFailure>.Failure(o)));

            await _broker.PublishCommand(request, fireTopic).ConfigureAwait(false);
            await foreach (var p in channel.Reader.ReadAllAsync(cancellationToken))
            {
                if (p.MessageId == request.MessageId)
                    return transformResult(p);
            }

            throw new InvalidOperationException($"Did not receive any response");
        }

        private IDisposable RegisterCompleteHandler<TResponse>(TopicName topic, Action<TResponse> onReceived)
            where TResponse : notnull
        {
            if (onReceived is null) throw new ArgumentNullException(nameof(onReceived));

            IDisposable result = _broker
                .Events(topic, EventsOptions.Temporary)
                .Subscribe<TResponse>(m =>
                {
                    m.Ack();
                    onReceived(m.Payload);
                });

            return result;
        }

        private readonly struct Nothing
        {
        }

        private readonly struct ResponseOutcome<TSuccessOutcome, TFailureOutcome>
            where TSuccessOutcome : IHasMessageId
            where TFailureOutcome : IHasMessageId
        {
            public static ResponseOutcome<TSuccessOutcome, TFailureOutcome> Failure(TFailureOutcome outcome)
            {
                return new ResponseOutcome<TSuccessOutcome, TFailureOutcome>()
                {
                    HasFailed = true,
                    FailureMessage = outcome
                };
            }

            public static ResponseOutcome<TSuccessOutcome, TFailureOutcome> Success(TSuccessOutcome outcome)
            {
                return new ResponseOutcome<TSuccessOutcome, TFailureOutcome>()
                {
                    HasFailed = false,
                    SuccessMessage = outcome
                };
            }

            [MemberNotNullWhen(true, nameof(FailureMessage))]
            [MemberNotNullWhen(false, nameof(SuccessMessage))]
            public bool HasFailed { get; private init; }

            public TSuccessOutcome? SuccessMessage { get; private init; }

            public TFailureOutcome? FailureMessage { get; private init; }

            public MessageId MessageId => HasFailed ? FailureMessage.MessageId : SuccessMessage.MessageId;
        }
    }
}
