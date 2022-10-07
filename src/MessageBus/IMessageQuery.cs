using System;
using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMessageQuery<out TQueryResult> : IBusMessage
            where TQueryResult : IMessageQueryResult
    {
    }

    public interface IMessageQueryResult : IBusMessage
    {
        public sealed class FailureResult : IMessageQueryResult
        {
            public FailureResult(MessageId messageId, IFailureDetails details)
            {
                MessageId = messageId;
                Details = details ?? throw new ArgumentNullException(nameof(details));
            }

            public MessageId MessageId { get; }

            public IFailureDetails Details { get; }
        }
    }

    public interface IMessageQueryHandler<in TQuery, out TQueryResult>
        where TQuery : IMessageQuery<TQueryResult>
        where TQueryResult : IMessageQueryResult
    {
        TQueryResult Handle(TQuery query);
    }

    public interface IAsyncMessageQueryHandler<in TQuery, TQueryResult>
        where TQuery : IMessageQuery<TQueryResult>
        where TQueryResult : IMessageQueryResult
    {
        Task<TQueryResult> HandleAsync(TQuery query);
    }
}
