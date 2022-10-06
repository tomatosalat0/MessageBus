using System;
using System.Collections.Concurrent;

namespace MessageBus.Serialization.Json.DynamicTypes
{
    internal class RuntimeTypeCache : IIntermediateInterfaceImplementer
    {
        private readonly ConcurrentDictionary<Type, Type> _generatedTypes = new ConcurrentDictionary<Type, Type>();
        private readonly IIntermediateInterfaceImplementer _inner;

        public RuntimeTypeCache(IIntermediateInterfaceImplementer inner)
        {
            _inner = inner;
        }

        public Type CreateType(Type interfaceType)
        {
            return _generatedTypes.GetOrAdd(interfaceType, CreateFromType);
        }

        private Type CreateFromType(Type interfaceType)
        {
            return _inner.CreateType(interfaceType);
        }
    }
}
