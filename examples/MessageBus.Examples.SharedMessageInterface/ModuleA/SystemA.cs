using System;
using System.Threading.Tasks;
using MessageBus.Examples.SharedMessageInterface.Common;

namespace MessageBus.Examples.SharedMessageInterface.ModuleA
{
    internal static class SystemA
    {
        public static ICallable Register(IMessageBus bus)
        {
            return ModuleA.Start(bus);
        }

        public interface ICallable
        {
            Task PerfomSomeWork();
        }

        private class ModuleA : ICallable
        {
            private readonly IMessageBus _messageBus;

            private ModuleA(IMessageBus messageBus)
            {
                _messageBus = messageBus;
            }

            public static ModuleA Start(IMessageBus messageBus)
            {
                return new ModuleA(messageBus);
            }

            public async Task PerfomSomeWork()
            {
                Console.WriteLine($"[{nameof(ModuleA)}] Fetching current time");
                DateTime valueToBroadcast = (await _messageBus.FireQuery<IGetCurrentDateTimeQuery, IGetCurrentDateTimeQuery.IResult>(new Query(universalTime: false), TimeSpan.FromSeconds(1))).Value;

                // note that the type is defined explicitely to IMyEvent. Thats because
                // the actual topic is defined within the interface and not in the concrete
                // implementation "ModuleAEvent"
                Console.WriteLine($"[{nameof(ModuleA)}] Broadcasting event");
                await _messageBus.FireEvent<IMyEvent>(new ModuleAEvent(DateTime.Now));
            }

            private class Query : IGetCurrentDateTimeQuery
            {
                public Query(bool universalTime)
                {
                    UniversalTime = universalTime;
                }

                public bool UniversalTime { get; }

                public MessageId MessageId { get; } = MessageId.NewId();
            }
        }

        private class ModuleAEvent : IMyEvent
        {
            public ModuleAEvent(DateTime firedOn)
            {
                FiredOn = firedOn.ToString("o");
            }

            public string FiredOn { get; }

            public MessageId MessageId { get; } = MessageId.NewId();
        }
    }
}
