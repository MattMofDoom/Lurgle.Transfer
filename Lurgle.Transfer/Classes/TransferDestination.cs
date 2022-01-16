// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.Linq;
using Lurgle.Transfer.Enums;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedMember.Global

namespace Lurgle.Transfer.Classes
{
    /// <summary>
    ///     Transfer configuration class
    /// </summary>
    public class TransferDestination
    {
        /// <summary>
        ///     Constructor that permits passing a TransferDestination config and optional overrides of any property
        /// </summary>
        /// <param name="config"></param>
        /// <param name="name"></param>
        /// <param name="transferType"></param>
        /// <param name="transferMode"></param>
        /// <param name="destination"></param>
        /// <param name="authMode"></param>
        /// <param name="certPath"></param>
        /// <param name="bufferSize"></param>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="usePassive"></param>
        /// <param name="remotePath"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="doArchive"></param>
        /// <param name="archivePath"></param>
        /// <param name="archiveDays"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="retryCount"></param>
        /// <param name="retryDelay"></param>
        /// <param name="retryTest"></param>
        /// <param name="retryFailAll"></param>
        /// <param name="retryFailConnect"></param>
        /// <param name="useProxy"></param>
        /// <param name="proxyType"></param>
        /// <param name="proxyServer"></param>
        /// <param name="proxyPort"></param>
        /// <param name="proxyUser"></param>
        /// <param name="proxyPassword"></param>
        /// <param name="compressType"></param>
        /// <param name="zipPrefix"></param>
        /// <param name="mailTo"></param>
        /// <param name="mailToError"></param>
        /// <param name="mailIfError"></param>
        /// <param name="mailIfSuccess"></param>
        /// <param name="downloadDays"></param>

        public TransferDestination(TransferDestination config = null, string name = null,
            TransferType? transferType = null,
            TransferMode? transferMode = null, string destination = null, TransferAuth? authMode = null,
            string certPath = null, int? bufferSize = null, string server = null, int? port = null,
            bool? usePassive = null,
            string remotePath = null, string sourcePath = null, string destPath = null, bool? doArchive = null,
            string archivePath = null, int? archiveDays = null, string userName = null, string password = null,
            int? retryCount = null, int? retryDelay = null, bool? retryTest = null, bool? retryFailAll = null,
            bool? retryFailConnect = null, bool? useProxy = null, string proxyType = null, string proxyServer = null,
            int? proxyPort = null, string proxyUser = null, string proxyPassword = null,
            CompressType? compressType = null, string zipPrefix = null, string mailTo = null, string mailToError = null,
            bool? mailIfError = null, bool? mailIfSuccess = null, int? downloadDays = null)
        {
            if (config != null)
            {
                Name = config.Name;
                TransferType = config.TransferType;
                TransferMode = config.TransferMode;
                Destination = config.Destination;
                AuthMode = config.AuthMode;
                CertPath = config.CertPath;
                BufferSize = config.BufferSize;
                Server = config.Server;
                Port = config.Port;
                UsePassive = config.UsePassive;
                RemotePath = config.RemotePath;
                SourcePath = config.SourcePath;
                DestPath = config.DestPath;
                DoArchive = config.DoArchive;
                ArchivePath = config.ArchivePath;
                ArchiveDays = config.ArchiveDays;
                UserName = config.UserName;
                Password = config.Password;
                RetryCount = config.RetryCount;
                RetryDelay = config.RetryDelay;
                RetryTest = config.RetryTest;
                RetryFailAll = config.RetryFailAll;
                RetryFailConnect = config.RetryFailConnect;
                UseProxy = config.UseProxy;
                ProxyType = config.ProxyType;
                ProxyServer = config.ProxyServer;
                ProxyPort = config.ProxyPort;
                ProxyUser = config.ProxyUser;
                ProxyPassword = config.ProxyPassword;
                CompressType = config.CompressType;
                ZipPrefix = config.ZipPrefix;
                MailTo = config.MailTo;
                MailToError = config.MailToError;
                MailIfError = config.MailIfError;
                MailIfSuccess = config.MailIfSuccess;
                DownloadDays = config.DownloadDays;
            }

            if (!string.IsNullOrEmpty(name))
                Name = name;
            if (transferType != null)
                TransferType = (TransferType) transferType;
            if (transferMode != null)
                TransferMode = (TransferMode) transferMode;
            if (!string.IsNullOrEmpty(destination))
                Destination = destination;
            if (authMode != null)
                AuthMode = (TransferAuth) authMode;
            if (!string.IsNullOrEmpty(certPath))
                CertPath = certPath;
            if (bufferSize != null)
                BufferSize = (int) bufferSize;
            if (!string.IsNullOrEmpty(server))
                Server = server;
            if (port != null)
                Port = (int) port;
            if (usePassive != null)
                UsePassive = (bool) usePassive;
            if (!string.IsNullOrEmpty(remotePath))
                RemotePath = remotePath;
            if (!string.IsNullOrEmpty(sourcePath))
                SourcePath = sourcePath;
            if (!string.IsNullOrEmpty(destPath))
                DestPath = destPath;
            if (doArchive != null)
                DoArchive = (bool) doArchive;
            if (!string.IsNullOrEmpty(archivePath))
                ArchivePath = archivePath;
            if (archiveDays != null)
                ArchiveDays = (int) archiveDays;
            if (!string.IsNullOrEmpty(userName))
                UserName = userName;
            if (!string.IsNullOrEmpty(password))
                Password = password;
            if (retryCount != null)
                RetryCount = (int) retryCount;
            if (retryDelay != null)
                RetryDelay = (int) retryDelay;
            if (retryTest != null)
                RetryTest = (bool) retryTest;
            if (retryFailAll != null)
                RetryFailAll = (bool) retryFailAll;
            if (retryFailConnect != null)
                RetryFailConnect = (bool) retryFailConnect;
            if (useProxy != null)
                UseProxy = (bool) useProxy;
            if (!string.IsNullOrEmpty(ProxyType))
                ProxyType = proxyType;
            if (!string.IsNullOrEmpty(proxyServer))
                ProxyServer = proxyServer;
            if (proxyPort != null)
                ProxyPort = (int) proxyPort;
            if (!string.IsNullOrEmpty(proxyUser))
                ProxyUser = proxyUser;
            if (!string.IsNullOrEmpty(proxyPassword))
                ProxyPassword = proxyPassword;
            if (compressType != null)
                CompressType = (CompressType) compressType;
            if (!string.IsNullOrEmpty(zipPrefix))
                ZipPrefix = zipPrefix;
            if (!string.IsNullOrEmpty(mailTo))
                MailTo = mailTo;
            if (!string.IsNullOrEmpty(mailToError))
                MailToError = mailToError;
            if (mailIfError != null)
                MailIfError = (bool) mailIfError;
            if (mailIfSuccess != null)
                MailIfSuccess = (bool) mailIfSuccess;
            if (downloadDays != null)
                DownloadDays = (int) downloadDays;

            // Ensure Destination and Name are set
            if (string.IsNullOrEmpty(Destination))
                Destination = string.IsNullOrEmpty(Name)
                    ? $"Default{Transfers.GetRandomNumber()}"
                    : string.Concat(name.Where(c => !char.IsWhiteSpace(c)));

            if (string.IsNullOrEmpty(Name))
                Name = Destination.StartsWith("Default") ? Destination : $"Default{Transfers.GetRandomNumber()}";

            if (Port.Equals(-1)) Port = TransferConfig.DefaultSftpPort;

            if (ProxyPort.Equals(-1)) ProxyPort = TransferConfig.DefaultProxyPort;

            if (BufferSize < 32768) BufferSize = TransferConfig.DefaultFtpBufferSize;

            if (RetryCount < 0)
                RetryCount = 0;

            if (RetryDelay < 0)
            {
                RetryDelay = 0;
            }
            else
            {
                if (RetryDelay < 600)
                    RetryDelay *= 1000;
                else
                    RetryDelay = 60000;
            }

            if (ArchiveDays.Equals(-1) || ArchiveDays < TransferConfig.ArchiveDaysMin ||
                ArchiveDays > TransferConfig.ArchiveDaysMax)
                ArchiveDays = TransferConfig.ArchiveDaysDefault;
            //If ArchiveDays is 0, then we disable archiving
            if (ArchiveDays.Equals(0)) DoArchive = false;
        }

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
        public string Destination { get; set; }

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
        public string RemotePath { get; set; }

        /// <summary>
        ///     Source path for uploads
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        ///     Destination path to move files after transfer
        /// </summary>
        public string DestPath { get; set; }

        /// <summary>
        ///     Do archival
        /// </summary>
        public bool DoArchive { get; set; }

        /// <summary>
        ///     Archive path
        /// </summary>
        public string ArchivePath { get; set; }

        /// <summary>
        ///     Days to retain files in archive
        /// </summary>
        public int ArchiveDays { get; set; }

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
    }
}