namespace Lurgle.Sftp.Enums
{
    /// <summary>
    ///     Master list of compression statuses
    /// </summary>
    public enum CompressStatus
    {
        /// <summary>
        ///     Success
        /// </summary>
        Success,

        /// <summary>
        ///     Error
        /// </summary>
        Error,

        /// <summary>
        ///     Not Compressed
        /// </summary>
        NotCompressed,

        /// <summary>
        ///     Path not found
        /// </summary>
        PathNotFound,

        /// <summary>
        ///     Unknown/not specified
        /// </summary>
        Unknown = -1
    }
}