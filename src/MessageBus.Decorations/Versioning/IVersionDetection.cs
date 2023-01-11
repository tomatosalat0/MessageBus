using System;

namespace MessageBus.Decorations.Versioning
{
    public interface IVersionDetection<TIndicatorType>
        where TIndicatorType : IComparable<TIndicatorType>
    {
        /// <summary>
        /// Gets the version from an incoming message. Returns true if
        /// the provided <paramref name="versionId"/> is newer then the
        /// latest known version, otherwise false.
        /// </summary>
        bool HandleMessageVersion(TIndicatorType versionId);

        /// <summary>
        /// Instructs the version detection to forget about the provided
        /// <paramref name="versionId"/>. A future call to <see cref="HandleMessageVersion(TIndicatorType)"/>
        /// must return true for the same version id again.
        /// </summary>
        void ForgetVersion(TIndicatorType versionId);
    }
}
