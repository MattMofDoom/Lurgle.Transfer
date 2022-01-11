// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Lurgle.Transfer.Enums
{
    /// <summary>
    ///     Exit codes
    /// </summary>
    public enum ExitCodes
    {
        /// <summary>
        ///     Success
        /// </summary>
        Success = 0,

        /// <summary>
        ///     Unhandled exception
        /// </summary>
        UnhandledException = 1,

        /// <summary>
        ///     Compression phase failed
        /// </summary>
        CompressFailed = 2,

        /// <summary>
        ///     Connection phase failed
        /// </summary>
        ConnectFailed = 3,

        /// <summary>
        ///     Transfer phase failed
        /// </summary>
        TransferFailed = 4,

        /// <summary>
        ///     Cleanup phase failed
        /// </summary>
        CleanFailed = 5,

        /// <summary>
        ///     Archive phase failed
        /// </summary>
        ArchiveFailed = 6,

        /// <summary>
        ///     Mail host was unreachable
        /// </summary>
        UnreachableMailHost = 7,

        /// <summary>
        ///     SFTP Transfer displayed the Help dialog
        /// </summary>
        HelpDisplayed = 8,

        /// <summary>
        ///     PDF conversion failed
        /// </summary>
        ConvertFailed = 9,

        /// <summary>
        ///     An application exception was intercepted
        /// </summary>
        AppException = 10,

        /// <summary>
        ///     Source path validation failed
        /// </summary>
        SourcePathFailure = 11,

        /// <summary>
        ///     Destination path validation failed
        /// </summary>
        DestPathFailure = 12,

        /// <summary>
        ///     Archive path validation failed
        /// </summary>
        ArchivePathFailure = 13,

        /// <summary>
        ///     An email error occurred
        /// </summary>
        EmailError = 14
    }
}