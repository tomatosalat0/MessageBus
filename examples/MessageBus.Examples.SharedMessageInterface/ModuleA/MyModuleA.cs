using System;
using System.Threading.Tasks;
using MessageBus.Examples.SharedMessageInterface.Common;

namespace MessageBus.Examples.SharedMessageInterface.ModuleA
{
    internal class MyModuleA
    {
        private readonly IMessageBus _messageBus;

        public MyModuleA(IMessageBus messageBus)
        {
            _messageBus = messageBus;
        }

        public Task PerfomSomeWork()
        {
            // broadcast the result
            Console.WriteLine($"[{nameof(MyModuleA)}] Broadcasting event");

            // note that the type is defined explicitely to IMyEvent. Thats because
            // the actual topic is defined within the interface and not in the concrete
            // implementation "ModuleAEvent"
            return _messageBus.FireEvent<IMyEvent>(new ModuleAEvent(DateTime.Now));
        }
    }
}
