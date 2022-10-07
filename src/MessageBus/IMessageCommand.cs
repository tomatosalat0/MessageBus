using System;
using System.Threading.Tasks;

namespace MessageBus
{
    public interface IMessageCommand : IBusMessage
    {
    }

    public interface IMessageCommandHandler<in TCommand>
        where TCommand : IMessageCommand
    {
        void Handle(TCommand command);
    }

    public interface IAsyncMessageCommandHandler<in TCommand>
        where TCommand : IMessageCommand
    {
        Task HandleAsync(TCommand command);
    }

    public sealed class MessageCommandOutcome : IBusMessage
    {
        public MessageCommandOutcome(MessageId messageId, IFailureDetails? failureDetails)
        {
            MessageId = messageId;
            FailureDetails = failureDetails;
        }

        public static MessageCommandOutcome Success(MessageId id)
        {
            return new MessageCommandOutcome(id, null);
        }

        public static MessageCommandOutcome Failure(MessageId id, Exception exception)
        {
            return new MessageCommandOutcome(id, new ExceptionFailure(exception));
        }

        public MessageId MessageId { get; }

        public IFailureDetails? FailureDetails { get; }
    }
}
