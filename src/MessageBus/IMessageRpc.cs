using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMessageRpc<out TRpcResult> : IBusMessage
            where TRpcResult : IMessageRpcResult
    {
    }

    public interface IMessageRpcResult : IBusMessage
    {
        public sealed class FailureResult : IMessageRpcResult
        {
            public FailureResult(MessageId messageId, IFailureDetails details)
            {
                MessageId = messageId;
                Details = details;
            }

            public MessageId MessageId { get; }

            public IFailureDetails Details { get; }
        }
    }

    public interface IMessageRpcHandler<in TRpc, out TRpcResult>
        where TRpc : IMessageRpc<TRpcResult>
        where TRpcResult : IMessageRpcResult
    {
        TRpcResult Handle(TRpc query);
    }

    public interface IAsyncMessageRpcHandler<in TRpc, TRpcResult>
        where TRpc : IMessageRpc<TRpcResult>
        where TRpcResult : IMessageRpcResult
    {
        Task<TRpcResult> HandleAsync(TRpc query);
    }
}
