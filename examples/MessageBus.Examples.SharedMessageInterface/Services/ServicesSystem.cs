using System;
using MessageBus.Examples.SharedMessageInterface.Common;

namespace MessageBus.Examples.SharedMessageInterface.Services
{
    internal static class ServicesSystem
    {
        public static void Register(IMessageBus bus)
        {
            DateTimeService.Start(bus);
        }

        private class DateTimeService : IMessageQueryHandler<IGetCurrentDateTimeQuery, IGetCurrentDateTimeQuery.IResult>
        {
            private readonly IMessageBus _messageBus;

            private DateTimeService(IMessageBus messageBus)
            {
                _messageBus = messageBus;
                _messageBus.RegisterQueryHandler(this);
            }

            public static DateTimeService Start(IMessageBus messageBus)
            {
                return new DateTimeService(messageBus);
            }

            public IGetCurrentDateTimeQuery.IResult Handle(IGetCurrentDateTimeQuery query)
            {
                Console.WriteLine($"[{nameof(DateTimeService)}] Current time requested");
                DateTime result = query.UniversalTime ? DateTime.UtcNow : DateTime.Now;
                return new Result(result, query.MessageId);
            }

            private class Result : IGetCurrentDateTimeQuery.IResult
            {
                public Result(DateTime value, MessageId originalMessage)
                {
                    Value = value;
                    MessageId = originalMessage;
                }

                public DateTime Value { get; }

                public MessageId MessageId { get; }
            }
        }
    }
}
