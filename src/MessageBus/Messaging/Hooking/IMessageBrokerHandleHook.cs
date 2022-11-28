using System;

namespace MessageBus.Messaging.Hooking
{
    public interface IMessageBrokerHandleHook
    {
        /// <summary>
        /// This hook gets executed before a message gets passed to a handler. The provided <paramref name="message"/>
        /// contains the information available. The returned value gets passed to the handler.
        /// </summary>
        /// <remarks>The default behaviour would be to simply return the value of <see cref="HookedMessage{T}.Message"/>.</remarks>
        IMessage<T> BeforeHandleMessage<T>(HookedMessage<T> message);

        /// <summary>
        /// This hook gets executed after the handler was executed. The provided <paramref name="message"/> is the same
        /// as the one which was provided in <see cref="BeforeHandleMessage{T}(HookedMessage{T})"/>.
        /// </summary>
        void AfterHandleMessage<T>(HookedMessage<T> message, TimeSpan elapsedTime);

        /// <summary>
        /// This hook gets executed when an unhandled exception occured within the handler. The provided <paramref name="message"/> is the same
        /// as the one which was provided in <see cref="BeforeHandleMessage{T}(HookedMessage{T})"/>. The exception
        /// will get rethrown automatically.
        /// </summary>
        void ExceptionDuringHandle<T>(HookedMessage<T> message, TimeSpan elapsedTime, Exception exception);

        /// <summary>
        /// This hook gets executed before a message gets passed to a handler. The provided <paramref name="message"/>
        /// contains the information available. The returned value gets passed to the handler.
        /// </summary>
        /// <remarks>The default behaviour would be to simply return the value of <see cref="HookedMessage.Message"/>.</remarks>
        IMessage BeforeHandleMessage(HookedMessage message);

        /// <summary>
        /// This hook gets executed after the handler was executed. The provided <paramref name="message"/> is the same
        /// as the one which was provided in <see cref="BeforeHandleMessage(HookedMessage)"/>.
        /// </summary>
        void AfterHandleMessage(HookedMessage message, TimeSpan elapsedTime);

        /// <summary>
        /// This hook gets executed when an unhandled exception occured within the handler. The provided <paramref name="message"/> is the same
        /// as the one which was provided in <see cref="BeforeHandleMessage(HookedMessage)"/>. The exception
        /// will get rethrown automatically.
        /// </summary>
        void ExceptionDuringHandle(HookedMessage message, TimeSpan elapsedTime, Exception exception);
    }

    public readonly ref struct HookedMessage<T>
    {
        public HookedMessage(IMessage<T> message, TopicName topic, string messageDescription)
        {
            Message = message;
            Topic = topic;
            MessageDescription = messageDescription;
        }

        /// <summary>
        /// The message to process.
        /// </summary>
        public IMessage<T> Message { get; }

        /// <summary>
        /// The topic the <see cref="Message"/> was sent in.
        /// </summary>
        public TopicName Topic { get; }

        /// <summary>
        /// A human readable description for the <see cref="Message"/>. This can be useful
        /// for logging.
        /// </summary>
        public string MessageDescription { get; }
    }

    public readonly ref struct HookedMessage
    {
        public HookedMessage(IMessage message, TopicName topic, string messageDescription)
        {
            Message = message;
            Topic = topic;
            MessageDescription = messageDescription;
        }

        /// <summary>
        /// The message to process.
        /// </summary>
        public IMessage Message { get; }

        /// <summary>
        /// The topic the <see cref="Message"/> was sent in.
        /// </summary>
        public TopicName Topic { get; }

        /// <summary>
        /// A human readable description for the <see cref="Message"/>. This can be useful
        /// for logging.
        /// </summary>
        public string MessageDescription { get; }
    }
}
