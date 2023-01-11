using System;
using System.Threading.Tasks;

namespace MessageBus.Examples.CrossModulesCommunication.ModuleA
{
    internal class MyModuleA
    {
        private readonly IMessageBus _messageBus;
        private string? _someState;

        public MyModuleA(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public Task PerformSomeWork()
        {
            // actually do something
            _someState = DateTime.Now.ToString("o");

            // broadcast the result
            Console.WriteLine($"[{nameof(MyModuleA)}] Broadcasting state change");
            return _messageBus.FireEvent(new SomeStateChangedEvent(_someState));
        }
    }
}
