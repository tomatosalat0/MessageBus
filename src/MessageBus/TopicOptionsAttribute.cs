using System;

namespace MessageBus
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class TopicOptionsAttribute : Attribute
    {
        public TopicOptionsAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}
