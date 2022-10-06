using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageBus.Examples.SharedMessageInterface.Common;

namespace MessageBus.Examples.SharedMessageInterface.ModuleA
{
    internal class ModuleAEvent : IMyEvent
    {
        public ModuleAEvent(DateTime firedOn)
        {
            FiredOn = firedOn.ToString("o");
        }

        public string FiredOn { get; }

        public MessageId MessageId { get; } = MessageId.NewId();
    }
}
