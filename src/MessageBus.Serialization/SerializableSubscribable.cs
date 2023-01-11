using System;
using MessageBus.Messaging;

namespace MessageBus.Serialization
{
    internal sealed  class SerializableSubscribable : ISubscribable
    {
        private readonly IMessageSerializer _serializer;
        private readonly ISubscribable _inner;

        public SerializableSubscribable(ISubscribable inner, IMessageSerializer serializer)
        {
            _inner = inner;
            _serializer = serializer;
        }

        public IDisposable Subscribe<T>(Action<IMessage<T>> messageHandler) where T : notnull
        {
            IMessageSerializer serializer = _serializer;
            return _inner.Subscribe<byte[]>((data) =>
            {
                IMessage<T> deserialized = new DeserializedMessage<T>(data, serializer);
                messageHandler(deserialized);
            });
        }

        public IDisposable Subscribe(Action<IMessage> messageHandler)
        {
            IMessageSerializer serializer = _serializer;
            return _inner.Subscribe<byte[]>((data) =>
            {
                IMessage deserialized = new DeserializedAnonymousMessage(data, serializer);
                messageHandler(deserialized);
            });
        }

        private sealed class DeserializedAnonymousMessage : IMessage
        {
            private readonly IMessage<byte[]> _source;

            public DeserializedAnonymousMessage(IMessage<byte[]> source, IMessageSerializer serializer)
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
            private readonly IMessage<byte[]> _source;

            public DeserializedMessage(IMessage<byte[]> source, IMessageSerializer serializer)
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
