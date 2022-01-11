// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Lurgle.Transfer.Enums
{
    /// <summary>
    ///     Master list of Email types
    /// </summary>
    public enum EmailType
    {
        /// <summary>
        ///     File Sent notifications
        /// </summary>
        Sent,

        /// <summary>
        ///     Error notifications
        /// </summary>
        Error,

        /// <summary>
        ///     Exception notifications
        /// </summary>
        Exception,

        /// <summary>
        ///     Logging failure notifications
        /// </summary>
        LogFailed
    }
}