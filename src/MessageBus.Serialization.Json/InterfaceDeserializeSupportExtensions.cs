﻿namespace MessageBus.Serialization.Json
{
    public static class InterfaceDeserializeSupportExtensions
    {
        public static IMessageSerializer WithInterfaceDeserializer(this ITypeMessageSerializer serializer, TypeCreationOptions? options = null)
        {
            return new InterfaceJsonMessageSerializer(serializer, options ?? new TypeCreationOptions());
        }

        public static IMessageMemorySerializer WithInterfaceDeserializer(this ITypeMessageMemorySerializer serializer, TypeCreationOptions? options = null)
        {
            return new InterfaceJsonMemoryMessageSerializer(serializer, options ?? new TypeCreationOptions());
        }
    }
}
