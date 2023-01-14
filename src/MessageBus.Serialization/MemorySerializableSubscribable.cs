using System;
using MessageBus.Messaging;

namespace MessageBus.Serialization
{
    internal sealed class MemorySerializableSubscribable : ISubscribable
    {
        private readonly IMessageMemorySerializer _serializer;
        private readonly ISubscribable _inner;

        public MemorySerializableSubscribable(ISubscribable inner, IMessageMemorySerializer serializer)
        {
            _inner = inner;
            _serializer = serializer;
        }

        public IDisposable Subscribe<T>(Action<IMessage<T>> messageHandler) where T : notnull
        {
            IMessageMemorySerializer serializer = _serializer;
            return _inner.Subscribe<ReadOnlyMemory<byte>>((data) =>
            {
                IMessage<T> deserialized = new DeserializedMessage<T>(data, serializer);
                messageHandler(deserialized);
            });
        }

        public IDisposable Subscribe(Action<IMessage> messageHandler)
        {
            IMessageMemorySerializer serializer = _serializer;
            return _inner.Subscribe<ReadOnlyMemory<byte>>((data) =>
            {
                IMessage deserialized = new DeserializedAnonymousMessage(data, serializer);
                messageHandler(deserialized);
            });
        }

        private sealed class DeserializedAnonymousMessage : IMessage
        {
            private readonly IMessage<ReadOnlyMemory<byte>> _source;

            public DeserializedAnonymousMessage(IMessage<ReadOnlyMemory<byte>> source, IMessageMemorySerializer serializer)
            {
                _source = source;
                Payload = serializer.DeserializeAnonymous(source.Payload);
            }

            public object Payload { get; }

            public MessageState State => _source.State;

            public void Ack()
            {
                _source.Ack();
            }

            public void Nack()
            {
                _source.Nack();
            }
        }

        private sealed class DeserializedMessage<T> : IMessage<T>, IMessageSupportsRejection
        {
            private readonly IMessage<ReadOnlyMemory<byte>> _source;

            public DeserializedMessage(IMessage<ReadOnlyMemory<byte>> source, IMessageMemorySerializer serializer)
            {
                _source = source;
                Payload = serializer.Deserialize<T>(source.Payload);
            }

            public T Payload { get; }

            public MessageState State => _source.State;

            public void Ack()
            {
                _source.Ack();
            }

            public void Nack()
            {
                _source.Nack();
            }

            public void Reject()
            {
                (_source as IMessageSupportsRejection)?.Reject();
            }
        }
    }
}