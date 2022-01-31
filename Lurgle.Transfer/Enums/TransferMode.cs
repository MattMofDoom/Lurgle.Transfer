// ReSharper disable UnusedMember.Global

namespace Lurgle.Transfer.Enums
{
    /// <summary>
    ///     Allows selection of SFTP or FTP
    /// </summary>
    public enum TransferMode
    {
        /// <summary>
        ///     SFTP (SSH)
        /// </summary>
        Sftp,

        /// <summary>
        ///     FTP
        /// </summary>
        Ftp,

        /// <summary>
        ///     SMB v1 (Windows 2003)
        /// </summary>
        Smb1,

        /// <summary>
        ///     SMB v2 (Windows 2008+)
        /// </summary>
        Smb2,

        /// <summary>
        ///     SMB v3 - functionally no different to SMB v2, provided for reference or future use
        /// </summary>
        Smb3
    }
}