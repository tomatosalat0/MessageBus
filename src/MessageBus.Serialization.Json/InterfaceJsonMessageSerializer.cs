using System;
using MessageBus.Serialization.Json.DynamicTypes;

namespace MessageBus.Serialization.Json
{
    internal class InterfaceJsonMessageSerializer : IMessageSerializer
    {
        private readonly IIntermediateInterfaceImplementer _interfaceImplementer;
        private readonly ITypeMessageSerializer _inner;

        private static IIntermediateInterfaceImplementer CreateDefaultImplementer(TypeCreationOptions options)
        {
            return new RuntimeTypeCache(new RuntimeTypeCreator(options));
        }

        public InterfaceJsonMessageSerializer(ITypeMessageSerializer inner, TypeCreationOptions options)
            : this(inner, CreateDefaultImplementer(options))
        {
        }

        public InterfaceJsonMessageSerializer(ITypeMessageSerializer inner, IIntermediateInterfaceImplementer interfaceImplementer)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _interfaceImplementer = interfaceImplementer ?? throw new ArgumentNullException(nameof(interfaceImplementer));
        }

        public T Deserialize<T>(byte[] data)
        {
            if (typeof(T).IsInterface)
                return DeserializeFromInterface<T>(data);

            return _inner.Deserialize<T>(data);
        }

        private T DeserializeFromInterface<T>(byte[] data) 
        {
            Type proxyType = _interfaceImplementer.CreateType(typeof(T));
            return _inner.Deserialize<T>(data, proxyType);
        }

        public object DeserializeAnonymous(byte[] data)
        {
            return _inner.DeserializeAnonymous(data);
        }

        public byte[] Serialize<T>(T message)
        {
            return _inner.Serialize(message);
        }
    }
}
