namespace Lurgle.Sftp.Enums
{
    /// <summary>
    ///     Result of folder validation
    /// </summary>
    public enum FolderResult
    {
        /// <summary>
        ///     Folder exists
        /// </summary>
        FolderExists,

        /// <summary>
        ///     Folder created
        /// </summary>
        FolderCreated,

        /// <summary>
        ///     Error creating folder
        /// </summary>
        ErrorCreatingFolder
    }
}