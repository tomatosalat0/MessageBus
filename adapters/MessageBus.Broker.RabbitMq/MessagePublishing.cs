using System;
using System.Threading;
using System.Threading.Tasks;
using MessageBus.Messaging;
using RabbitMQ.Client;

namespace MessageBus.Broker.RabbitMq
{
    internal class MessagePublishing : IDisposable
    {
        private readonly SemaphoreSlim _channelLock = new SemaphoreSlim(1, 1);
        private readonly IModel _channel;
        private bool _disposedValue;

        public MessagePublishing(IConnection connection)
        {
            _channel = connection.CreateModel();
        }

        public async Task PublishByteArray(string exchange, TopicName topic, byte[] payload)
        {
            await _channelLock.WaitAsync();
            try
            {
                ReadOnlyMemory<byte> buffer = new ReadOnlyMemory<byte>(payload);

                _channel.BasicPublish(
                    exchange: exchange,
                    routingKey: topic.ToString(),
                    basicProperties: null,
                    body: buffer
                );
            }
            finally
            {
                _channelLock.Release();
            }
        }

        public async Task PublishReadOnlyMemory(string exchange, TopicName topic, ReadOnlyMemory<byte> payload)
        {
            await _channelLock.WaitAsync();
            try
            {
                _channel.BasicPublish(
                    exchange: exchange,
                    routingKey: topic.ToString(),
                    basicProperties: null,
                    body: payload
                );
            }
            finally
            {
                _channelLock.Release();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _channel.Dispose();
                    _channelLock.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
