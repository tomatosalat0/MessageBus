using System;

namespace MessageBus.Examples.SharedMessageInterface.Common
{
    [Topic("Queries/GetCurrentDateTime")]
    public interface IGetCurrentDateTimeQuery : IMessageQuery<IGetCurrentDateTimeQuery.IResult>
    {
        bool UniversalTime { get; }

        public interface IResult : IMessageQueryResult
        {
            DateTime Value { get; }
        }
    }
}
