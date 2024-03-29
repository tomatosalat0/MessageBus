﻿using System;

namespace MessageBus.Serialization
{
    /// <summary>
    /// Converts message bodies to a byte array for sending messages and 
    /// deserializes them back from a byte array on receiving.
    /// </summary>
    public interface IMessageSerializer
    {
        /// <summary>
        /// Converts the provided <paramref name="message"/> to a byte array. The result
        /// will then go to the receiver.
        /// </summary>
        byte[] Serialize<T>(T message);

        /// <summary>
        /// Converts the provided <paramref name="data"/> back to an object of <typeparamref name="T"/>.
        /// </summary>
        T Deserialize<T>(byte[] data);

        /// <summary>
        /// Converts the provided <paramref name="data"/> back to an anonymous object.
        /// </summary>
        object DeserializeAnonymous(byte[] data);
    }

    /// <summary>
    /// Converts message bodies to a ReadOnlyMemory<byte> for sending messages and 
    /// deserializes them back from a ReadOnlyMemory<byte> on receiving.
    /// </summary>
    public interface IMessageMemorySerializer
    {
        /// <summary>
        /// Converts the provided <paramref name="message"/> to a ReadOnlyMemory. The result
        /// will then go to the receiver.
        /// </summary>
        ReadOnlyMemory<byte> Serialize<T>(T message);

        /// <summary>
        /// Converts the provided <paramref name="data"/> back to an object of <typeparamref name="T"/>.
        /// </summary>
        T Deserialize<T>(ReadOnlyMemory<byte> data);
        
        /// <summary>
        /// Converts the provided <paramref name="data"/> back to an anonymous object.
        /// </summary>
        object DeserializeAnonymous(ReadOnlyMemory<byte> data);
    }
}
