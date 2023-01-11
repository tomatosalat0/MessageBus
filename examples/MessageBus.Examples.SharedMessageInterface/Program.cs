using System.Threading.Tasks;
using System;
using MessageBus.Serialization.Json;
using MessageBus.Examples.SharedMessageInterface.ModuleA;
using MessageBus.Examples.SharedMessageInterface.ModuleB;
using MessageBus.Examples.SharedMessageInterface.Services;

namespace MessageBus.Examples.SharedMessageInterface
{
    internal class Program
    {
        /*
        This example demonstrates how the interface deserialization support can help decoupling
        events across module boundaries. 
        
        Within the Common folder all events and queries are defined. The flow of messages is
        1) Main steps into SystemA by calling "PerformSomeWork()"
        2) SystemA will execute a query to fetch the current time
        3) The ServiceSystem handles the time query and sends back a response.
        4) SystemA awaits that time response. After it received it, it will broadcast IMyEvent
        5) SystemB receives that event and simply writes something to the console

        While the class structure might be a bit confusing in the first place (nested private classes),
        it is only there to demonstrate that each module only knows about the events and queries - not 
        about any implementation or any other module within the system. 

        This example uses message serialization. While the broker is still the InProcess broker, it could 
        get replaced by an external system without and adjustments within any module or interface. After that,
        each module could even live within its own process.

        Because events are used, you can remove SystemB or add a SystemC without needing to change SystemA. 
         */
        static async Task Main(string[] args)
        {
            using IMessageBus messageBus = new MessageBrokerMessageBus(
                MemoryMessageBrokerBuilder.InProcessBroker()
                   .UseMessageSerialization(new JsonMessageSerializer().WithInterfaceDeserializer()),
                NoExceptionNotification.Instance
            );

            ServicesSystem.Register(messageBus);
            SystemA.ICallable systemA = SystemA.Register(messageBus);
            SystemB.Register(messageBus);

            await systemA.PerformSomeWork();

            // because the events might not get be processed yet, just wait 
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