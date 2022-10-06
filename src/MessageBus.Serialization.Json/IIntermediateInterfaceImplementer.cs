using System;

namespace MessageBus.Serialization.Json
{
    internal interface IIntermediateInterfaceImplementer
    {
        Type CreateType(Type interfaceType);
    }
}
