using System;

namespace MessageBus.Examples.CrossModulesCommunication.ModuleB
{
    internal class MyOtherModuleB : IMessageEventHandler<StateChangedEvent>
    {
        private readonly IMessageBus _messageBus;

        public MyOtherModuleB(IMessageBus messageBus)
        {
            _messageBus = messageBus;
            _messageBus.RegisterEventHandler(this);
        }

        public void Handle(StateChangedEvent @event)
        {
            Console.WriteLine($"[{nameof(MyOtherModuleB)}] Latest state is now {@event.LatestState}");
        }
    }
}
