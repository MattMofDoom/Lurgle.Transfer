namespace Lurgle.Sftp.Enums
{
    /// <summary>
    ///     Master list of compression types
    /// </summary>
    public enum CompressType
    {
        /// <summary>
        ///     Uncompressed
        /// </summary>
        Uncompressed,

        /// <summary>
        ///     Gzip
        /// </summary>
        Gzip,

        /// <summary>
        ///     Zip
        /// </summary>
        Zip,

        /// <summary>
        ///     Zip file for each file
        /// </summary>
        ZipPerFile
    }
}