// ReSharper disable UnusedAutoPropertyAccessor.Global

using Lurgle.Transfer.Enums;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace Lurgle.Transfer.Classes
{
    /// <summary>
    ///     SFTP configuration class
    /// </summary>
    public class TransferDestination
    {
        /// <summary>
        ///     Type of transfer
        /// </summary>
        public TransferType TransferType { get; set; }

        /// <summary>
        ///     Transfer mode (SFTP or FTP)
        /// </summary>
        public TransferMode TransferMode { get; set; }

        /// <summary>
        ///     Destination
        /// </summary>
        public Destination Destination { get; set; }

        /// <summary>
        ///     Authentication mode
        /// </summary>
        public TransferAuth AuthMode { get; set; }

        /// <summary>
        ///     Transfer name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Buffer size
        /// </summary>
        public int BufferSize { get; set; }

        /// <summary>
        ///     SFTP/FTP server
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        ///     SFTP/FTP TCP port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        ///     For FTP, use Passive mode
        /// </summary>
        public bool UsePassive { get; set; }

        /// <summary>
        ///     Remote path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        ///     Username
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Certificate file path for authentication
        /// </summary>
        public string CertPath { get; set; }

        /// <summary>
        ///     Number of times to retry a file transfer before exiting with error
        /// </summary>
        public int RetryCount { get; set; }

        /// <summary>
        ///     Time in seconds to wait before retry
        /// </summary>
        public int RetryDelay { get; set; }

        /// <summary>
        ///     Retry test mode - by default, first attempt will automatically be flagged as a failure
        /// </summary>
        public bool RetryTest { get; set; }

        /// <summary>
        ///     If RetryTest is set, fail all retries
        /// </summary>
        public bool RetryFailAll { get; set; }

        /// <summary>
        ///     If RetryTest is set, fail all connection attempts
        /// </summary>
        public bool RetryFailConnect { get; set; }

        /// <summary>
        ///     Use proxy server
        /// </summary>
        public bool UseProxy { get; set; }

        /// <summary>
        ///     Proxy type
        /// </summary>
        public string ProxyType { get; set; }

        /// <summary>
        ///     Proxy server
        /// </summary>
        public string ProxyServer { get; set; }

        /// <summary>
        ///     Proxy server TCP port
        /// </summary>
        public int ProxyPort { get; set; }

        /// <summary>
        ///     Proxy username
        /// </summary>
        public string ProxyUser { get; set; }

        /// <summary>
        ///     Proxy password
        /// </summary>
        public string ProxyPassword { get; set; }

        /// <summary>
        ///     Compression type
        /// </summary>
        public CompressType CompressType { get; set; }

        /// <summary>
        ///     Prefix for ZIP file
        /// </summary>
        public string ZipPrefix { get; set; }

        /// <summary>
        ///     Email address for notifications (Success, and Error if <see cref="MailToError" /> is not configured)
        /// </summary>
        public string MailTo { get; set; }

        /// <summary>
        ///     Email address for error notifications
        /// </summary>
        public string MailToError { get; set; }

        /// <summary>
        ///     Send email notifications on error
        /// </summary>
        public bool MailIfError { get; set; }

        /// <summary>
        ///     Send email notifications on success
        /// </summary>
        public bool MailIfSuccess { get; set; }

        /// <summary>
        ///     Maximum age of files to download
        /// </summary>
        public int DownloadDays { get; set; }

        /// <summary>
        ///     Convert PDFs to another PDF version before sending
        /// </summary>
        public bool ConvertPdf { get; set; }

        /// <summary>
        ///     Target PDF version
        /// </summary>
        public PdfTarget PdfTarget { get; set; }

        /// <summary>
        ///     Keep the original PDF after conversion
        /// </summary>
        public bool PdfKeepOriginal { get; set; }
    }
}