namespace Lurgle.Sftp.Enums
{
    /// <summary>
    ///     SFTP authentication types
    /// </summary>
    public enum SftpAuth
    {
        /// <summary>
        ///     Password authentication
        /// </summary>
        Password,

        /// <summary>
        ///     Certificate authentication
        /// </summary>
        Certificate,

        /// <summary>
        ///     Password and Certificate authentication
        /// </summary>
        Both
    }
}