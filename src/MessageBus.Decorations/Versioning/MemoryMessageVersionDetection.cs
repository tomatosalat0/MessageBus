using System;
using System.Collections.Generic;

namespace MessageBus.Decorations.Versioning
{
    public sealed class MemoryMessageVersionDetection<TIndicatorType> : IVersionDetection<TIndicatorType>
        where TIndicatorType : IComparable<TIndicatorType>
    {
        private const int MAX_VERSIONS_TO_KEEP = 10;

        private readonly List<TIndicatorType> _previousVersions = new List<TIndicatorType>();
        private readonly object _previousVersionLock = new object();

        public bool HandleMessageVersion(TIndicatorType versionId)
        {
            lock (_previousVersionLock)
            {
                bool isNewVersion = _previousVersions.Count == 0 ||
                    _previousVersions[_previousVersions.Count - 1].CompareTo(versionId) < 0;

                if (isNewVersion)
                {
                    _previousVersions.Add(versionId);
                    EnsureRememberdVersionCountLimit();
                    return true;
                }

                return false;
            }
        }

        public void ForgetVersion(TIndicatorType versionId)
        {
            lock (_previousVersionLock)
            {
                if (_previousVersions.Count == 0)
                    return;

                _previousVersions.RemoveAll(p => versionId.CompareTo(p) == 0);
            }
        }

        private void EnsureRememberdVersionCountLimit()
        {
            while (_previousVersions.Count > MAX_VERSIONS_TO_KEEP)
                _previousVersions.RemoveAt(0);
        }
    }
}
