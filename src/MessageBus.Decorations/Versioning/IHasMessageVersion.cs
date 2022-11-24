using System;

namespace MessageBus.Decorations.Versioning
{
    public interface IHasMessageVersion<TIndicatorType>
        where TIndicatorType : IComparable<TIndicatorType>
    {
        /// <summary>
        /// Indicates the version of this message. Note that this version is not
        /// a schema version. Each new message must get a new increasing version.
        /// If a message is comming in out of order (old version), it will get
        /// dropped.
        /// </summary>
        TIndicatorType MessageVersion { get; }
    }
}
