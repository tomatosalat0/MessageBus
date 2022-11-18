using System;
using System.Collections.Generic;
using System.Linq;

namespace MessageBus.Decorations.Duplications
{
    public sealed class MemoryDuplicateMessageDetection : IDuplicateDetection
    {
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
        private readonly ICurrentTime _currentTime;
        private readonly int _maxIdsToRemember;
        private readonly TimeSpan _maxDurationToRemember;
        private readonly Dictionary<MessageId, DateTime> _rememberedMessages = new Dictionary<MessageId, DateTime>();
        private readonly object _rememberedMessagesLock = new object();
        private DateTime _lastCleanup = DateTime.UtcNow;

        public MemoryDuplicateMessageDetection(int maxIdsToRemember, TimeSpan maxDurationToRemember, ICurrentTime currentTime)
        {
            _maxIdsToRemember = maxIdsToRemember;
            _maxDurationToRemember = maxDurationToRemember;
            _currentTime = currentTime ?? throw new ArgumentNullException(nameof(currentTime));
        }

        public MemoryDuplicateMessageDetection(int maxIdsToRemember, TimeSpan maxDurationToRemember)
            : this(maxIdsToRemember, maxDurationToRemember, new FromDateTime())
        {
            _maxIdsToRemember = maxIdsToRemember;
            _maxDurationToRemember = maxDurationToRemember;
        }

        public MemoryDuplicateMessageDetection()
            : this(maxIdsToRemember: 1000, maxDurationToRemember: TimeSpan.FromMinutes(10))
        {
        }

        public bool HandleReceivedMessage(MessageId messageId)
        {
            lock (_rememberedMessagesLock)
            {
                DateTime referenceTime = _currentTime.UtcNow();
                PerformCleanupIfNeeded(referenceTime);
                if (_rememberedMessages.TryGetValue(messageId, out DateTime addedOn) && referenceTime.Subtract(addedOn) <= _maxDurationToRemember)
                    return false;

                _rememberedMessages[messageId] = referenceTime;
                if (_rememberedMessages.Count > _maxIdsToRemember)
                    RemoveOldest();
                return true;
            }
        }

        public void ForgetMessage(MessageId messageId)
        {
            lock (_rememberedMessagesLock)
            {
                _rememberedMessages.Remove(messageId);
            }
        }

        private void PerformCleanupIfNeeded(DateTime referenceTime)
        {
            if (referenceTime < _lastCleanup)
            {
                _lastCleanup = referenceTime;
                return;
            }
            if (referenceTime.Subtract(_lastCleanup) >= _cleanupInterval)
            {
                CleanupNow(referenceTime);
                _lastCleanup = referenceTime;
            }
        }

        private void RemoveOldest()
        {
            MessageId? oldest = _rememberedMessages.OrderBy(p => p.Value).Select(p => p.Key).FirstOrDefault();
            if (oldest is not null)
                _rememberedMessages.Remove(oldest.Value);
        }

        private void CleanupNow(DateTime referenceTime)
        {
            IReadOnlyList<MessageId> toRemove = _rememberedMessages
                .Where(p => referenceTime.Subtract(p.Value) > _maxDurationToRemember)
                .Select(p => p.Key)
                .ToArray();

            foreach (var key in toRemove)
                _rememberedMessages.Remove(key);
        }

        public interface ICurrentTime
        {
            DateTime UtcNow();
        }

        private class FromDateTime : ICurrentTime
        {
            public DateTime UtcNow() => DateTime.UtcNow;
        }
    }
}
