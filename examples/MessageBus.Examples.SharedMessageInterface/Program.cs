using System.Threading.Tasks;
using System;
using MessageBus.Serialization.Json;
using MessageBus.Examples.SharedMessageInterface.ModuleA;
using MessageBus.Examples.SharedMessageInterface.ModuleB;

namespace MessageBus.Examples.SharedMessageInterface
{
    internal class Program
    {
        /*
        This example demonstrates how the interface deserialization support can help decoupling
        events across module boundaries. 
        
        Within the Common folder, an event is defined as an interface. 

        ModuleA fires that event.

        ModuleB listens to that event without actually implementing the interface defined in the
        Common module.

        Note that the interface deserialization doesn't support nested interface implementations.
         */
        static async Task Main(string[] args)
        {
            using IMessageBus messageBus = new MessageBrokerMessageBus(
                MemoryMessageBrokerBuilder.InProcessBroker()
                   .UseMessageSerialization(new JsonMessageSerializer().WithInterfaceDeserializer()),
                NoExceptionNotification.Instance
            );

            MyModuleA moduleA = new MyModuleA(messageBus);
            MyOtherModuleB otherModule = new MyOtherModuleB(messageBus);

            await moduleA.PerfomSomeWork();

            // because the event might not get be processed yet, just wait 
            // a tiny little bit. 
            await Task.Delay(100);

            // Within the console window, there should now be an output from 
            // both modules. 
            Console.WriteLine();
            Console.WriteLine("\tExample complete - press ENTER to exit.");
            Console.ReadLine();
        }
    }
}