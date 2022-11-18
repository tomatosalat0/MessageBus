namespace MessageBus.Decorations.Duplications
{
    public interface IDuplicateDetection
    {
        /// <summary>
        /// Gets a <see cref="MessageId"/> from an incomming message. This id
        /// gets stored. Returns true if the <paramref name="messageId"/>
        /// has not been seen before, otherwise false.
        /// </summary>
        bool HandleReceivedMessage(MessageId messageId);

        /// <summary>
        /// Instructs the duplication detection to forget about the provided
        /// <paramref name="messageId"/>. A future call to <see cref="HandleReceivedMessage(MessageId)"/>
        /// must return true for the same message id again.
        /// </summary>
        void ForgetMessage(MessageId messageId);
    }
}
