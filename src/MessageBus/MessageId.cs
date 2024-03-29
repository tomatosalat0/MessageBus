﻿using System;

namespace MessageBus
{
    /// <summary>
    /// Uniquely identifies a single request. General rule of thumb:
    /// <list type="bullet">
    ///     <item>Commands and queries create a new MessageId by using <see cref="NewId"/> or use <see cref="CausationId"/> when having an
    ///     existing CorrelationId within a Command or Query handler.</item>
    ///     <item>If you create Commands because of an event, create a new CorrelationId by using <see cref="CausedBy(MessageId)"/> and
    ///     pass the MessageId of the event.</item>
    /// </list>
    /// </summary>
    public readonly struct MessageId : IEquatable<MessageId>
    {
        private const string CausationSeparator = "->";

        private readonly string? _causationId;

        public MessageId(string value)
            : this(value, null)
        {
        }

        private MessageId(string value, string? causationId)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentException($"'{nameof(value)}' cannot be null or empty.", nameof(value));
            if (value.Contains(CausationSeparator)) throw new ArgumentException($"The message id must not contain '{CausationSeparator}', got '{value}'");
            if (causationId is not null && causationId.Contains(CausationSeparator)) throw new ArgumentException($"The causation id must not contain '{CausationSeparator}', got '{causationId}'");
            Value = value;
            _causationId = causationId;
        }

        /// <summary>
        /// Generate a new message id.
        /// </summary>
        public static MessageId NewId()
        {
            return new MessageId(Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// Generate a new message containing another <paramref name="messageId"/> as its causation.
        /// </summary>
        public static MessageId CausedBy(MessageId messageId)
        {
            return new MessageId(Guid.NewGuid().ToString("N"), messageId.Value);
        }

        /// <summary>
        /// The raw value.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// The correlationId which is the reason this message happened.
        /// Will be <c>null</c> if no causation id exists.
        /// </summary>
        public string? CausationId => _causationId;

        public bool Equals(MessageId other)
        {
            return Value.Equals(other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not MessageId other)
                return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode(StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return _causationId is not null
                ? $"{_causationId}{CausationSeparator}{Value}"
                : Value.ToString();
        }

        public static bool operator ==(MessageId lhs, MessageId rhs) => lhs.Equals(rhs);

        public static bool operator !=(MessageId lhs, MessageId rhs) => !(lhs == rhs);

        /// <summary>
        /// Parses the provided <paramref name="value"/> and converts it to a <see cref="MessageId"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="value"/> is null.</exception>
        /// <exception cref="ArgumentException">Is thrown if <paramref name="value"/> is not a valid message id.</exception>
        public static MessageId Parse(string? value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));

            ReadOnlySpan<char> asSpan = value.AsSpan();
            if (asSpan.IsEmpty)
                throw new ArgumentException($"The message id can not be an empty string");

            int causationSeparation = asSpan.IndexOf(CausationSeparator, StringComparison.Ordinal);
            if (causationSeparation < 0)
                return new MessageId(value);

            if (causationSeparation == 0 || causationSeparation == asSpan.Length - CausationSeparator.Length)
                throw new ArgumentException($"The provided message id is not valid, got '{value}'");

            ReadOnlySpan<char> causationId = asSpan.Slice(0, causationSeparation);
            ReadOnlySpan<char> messageId = asSpan.Slice(causationSeparation + CausationSeparator.Length);

            return new MessageId(messageId.ToString(), causationId.ToString());
        }
    }
}
