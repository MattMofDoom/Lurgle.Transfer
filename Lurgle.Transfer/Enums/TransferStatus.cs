namespace Lurgle.Transfer.Enums
{
    /// <summary>
    ///     Master list of transfer status results
    /// </summary>
    public enum TransferStatus
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
        ///     File Exists
        /// </summary>
        FileExists,

        /// <summary>
        ///     Unknown
        /// </summary>
        Unknown = -1
    }
}