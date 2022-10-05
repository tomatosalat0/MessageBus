using MessageBus.Serialization.Json;
using MessageBus.Examples.CrossModulesCommunication.ModuleA;
using MessageBus.Examples.CrossModulesCommunication.ModuleB;
using System.Threading.Tasks;
using System;

namespace MessageBus.Examples.CrossModulesCommunication
{
    internal class Program
    {
        /*
        This examples shows to do cross modules communication without introducting a shared
        dependency on the messages itself. To make it clear, the class names of the events
        are different in both modules as well - however this might not be a good idea.

        The events are only connected through the topic name defined in the Topic-attribute.

        While it might look unneccessary in the first place, it can actually help to make each module
        more independent from each other. 

        Important implementation detail: Each module doesn't know anything about each other here
        - even the Event Classes itself aren't shared. This makes it a lot harder to actually
        change the event itself later on. But but would be the case never the less if both modules
        would live in different projects/processes anyway. If you go with this route, your events
        will become an API.

        You can see the effect if you rename the property "LatestState" in ModuleA but don't do the
        same rename within ModuleB.SomeStateChangedEvent
         */
        static async Task Main(string[] args)
        {
            using IMessageBus messageBus = new MessageBrokerMessageBus(
                MemoryMessageBrokerBuilder.InProcessBroker()
                    .UseMessageSerialization(new JsonMessageSerializer()),
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