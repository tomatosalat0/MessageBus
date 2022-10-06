using System;
using MessageBus.Examples.SharedMessageInterface.Common;

namespace MessageBus.Examples.SharedMessageInterface.ModuleB
{
    internal class MyOtherModuleB : IMessageEventHandler<IMyEvent>
    {
        private readonly IMessageBus _messageBus;

        public MyOtherModuleB(IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _messageBus.RegisterEventHandler(this);
        }

        public void Handle(IMyEvent @event)
        {
            Console.WriteLine($"[{nameof(MyOtherModuleB)}] Received event which was fired on {@event.FiredOn} (id: {@event.MessageId})");
        }
    }
}
