using System;
using MessageBus.Examples.SharedMessageInterface.Common;

namespace MessageBus.Examples.SharedMessageInterface.ModuleB
{
    internal static class SystemB
    {
        public static void Register(IMessageBus bus)
        {
            ModuleB.Start(bus);
        }

        private class ModuleB : IMessageEventHandler<IMyEvent>
        {
            private readonly IMessageBus _messageBus;

            private ModuleB(IMessageBus messageBus)
            {
                _messageBus = messageBus;
                _messageBus.RegisterEventHandler(this);
            }

            public static ModuleB Start(IMessageBus messageBus)
            {
                return new ModuleB(messageBus);
            }

            public void Handle(IMyEvent @event)
            {
                Console.WriteLine($"[{nameof(ModuleB)}] Received event which was fired on {@event.FiredOn} (id: {@event.MessageId})");
            }
        }
    }
}
