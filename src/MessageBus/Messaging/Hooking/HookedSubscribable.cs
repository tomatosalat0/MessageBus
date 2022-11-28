using System;
using System.Diagnostics;

namespace MessageBus.Messaging.Hooking
{
    internal readonly struct HookedSubscribable : ISubscribable
    {
        private readonly IMessageBrokerHandleHook _hook;
        private readonly ISubscribable _inner;
        private readonly TopicName _topic;

        public HookedSubscribable(ISubscribable inner, TopicName topic, IMessageBrokerHandleHook hook)
        {
            _inner = inner;
            _topic = topic;
            _hook = hook;
        }

        public IDisposable Subscribe<T>(Action<IMessage<T>> messageHandler) where T : notnull
        {
            IMessageBrokerHandleHook hook = _hook;
            TopicName topic = _topic;
            return _inner.Subscribe<T>((data) =>
            {
                string messageType = data.BuildHumanReadableDescription();
                HookedMessage<T> hookedMessage = new HookedMessage<T>(data, topic, messageType);
                Stopwatch watch = Stopwatch.StartNew();
                IMessage<T> executeMessage = hook.BeforeHandleMessage<T>(hookedMessage);
                try
                {
                    messageHandler(executeMessage);
                    hook.AfterHandleMessage<T>(hookedMessage, watch.Elapsed);
                }
                catch (Exception ex)
                {
                    hook.ExceptionDuringHandle<T>(hookedMessage, watch.Elapsed, ex);
                    throw;
                }
            });
        }

        public IDisposable Subscribe(Action<IMessage> messageHandler)
        {
            IMessageBrokerHandleHook hook = _hook;
            TopicName topic = _topic;
            return _inner.Subscribe((data) =>
            {
                string messageType = data.BuildHumanReadableDescription();
                HookedMessage hookedMessage = new HookedMessage(data, topic, messageType);
                Stopwatch watch = Stopwatch.StartNew();
                IMessage executeMessage = hook.BeforeHandleMessage(hookedMessage);
                try
                {
                    messageHandler(executeMessage);
                    hook.AfterHandleMessage(hookedMessage, watch.Elapsed);
                }
                catch (Exception ex)
                {
                    hook.ExceptionDuringHandle(hookedMessage, watch.Elapsed, ex);
                    throw;
                }
            });
        }
    }
}
