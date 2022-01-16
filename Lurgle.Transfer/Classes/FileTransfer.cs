using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using FluentFTP;
using FluentFTP.Proxy;
using Flurl;
using Lurgle.Transfer.Enums;
using Renci.SshNet;

// ReSharper disable SwitchStatementHandlesSomeKnownEnumValuesWithDefault

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

// ReSharper disable UnusedMethodReturnValue.Global

namespace Lurgle.Transfer.Classes
{
    /// <summary>
    ///     SFTP/FTP instance
    /// </summary>
    public class FileTransfer
    {
        /// <summary>
        ///     SFTP/FTP instance
        /// </summary>
        /// <param name="config"></param>
        public FileTransfer(TransferDestination config)
        {
            TransferConfig = config;
            Destination = config.Destination;

            switch (config.TransferMode)
            {
                case TransferMode.Sftp:
                    var authMethods = new List<AuthenticationMethod>();
                    if ((TransferConfig.AuthMode.Equals(TransferAuth.Certificate) ||
                         TransferConfig.AuthMode.Equals(TransferAuth.Both)) &&
                        File.Exists(TransferConfig.CertPath))
                    {
                        UseCert = true;
                        PrivateKeyFile[] keyFile = {new PrivateKeyFile(TransferConfig.CertPath)};
                        authMethods.Add(new PrivateKeyAuthenticationMethod(TransferConfig.UserName, keyFile));
                    }

                    if (TransferConfig.AuthMode.Equals(TransferAuth.Password) ||
                        TransferConfig.AuthMode.Equals(TransferAuth.Both))
                        authMethods.Add(new PasswordAuthenticationMethod(TransferConfig.UserName,
                            TransferConfig.Password));

                    ConnectionInfo sftpConnection;

                    if (!TransferConfig.UseProxy ||
                        TransferConfig.UseProxy && string.IsNullOrEmpty(TransferConfig.ProxyServer))
                    {
                        sftpConnection = new ConnectionInfo(TransferConfig.Server, TransferConfig.Port,
                            TransferConfig.UserName,
                            authMethods.ToArray());
                    }
                    else
                    {
                        if (!Enum.TryParse(TransferConfig.ProxyType, out ProxyTypes proxyType))
                            proxyType = ProxyTypes.Http;

                        sftpConnection = new ConnectionInfo(TransferConfig.Server, TransferConfig.Port,
                            TransferConfig.UserName,
                            proxyType, TransferConfig.ProxyServer, TransferConfig.ProxyPort, TransferConfig.ProxyUser,
                            TransferConfig.ProxyPassword, authMethods.ToArray());
                    }

                    SftpClient = new SftpClient(sftpConnection)
                    {
                        BufferSize = (uint) TransferConfig.BufferSize,
                        KeepAliveInterval = new TimeSpan(0, 0, 15)
                    };
                    break;
                case TransferMode.Ftp:
                    if (TransferConfig.UseProxy && !string.IsNullOrEmpty(TransferConfig.ProxyServer))
                    {
                        var proxy = new ProxyInfo
                        {
                            Host = TransferConfig.ProxyServer,
                            Port = TransferConfig.ProxyPort
                        };

                        if (!string.IsNullOrEmpty(TransferConfig.ProxyUser) &&
                            !string.IsNullOrEmpty(TransferConfig.ProxyPassword))
                            proxy.Credentials =
                                new NetworkCredential(TransferConfig.ProxyUser, TransferConfig.ProxyPassword);

                        FtpClient = new FtpClientHttp11Proxy(proxy);
                    }
                    else
                    {
                        FtpClient = new FtpClient();
                    }

                    FtpClient.Host = TransferConfig.Server;
                    FtpClient.Port = TransferConfig.Port;

                    if (TransferConfig.AuthMode.Equals(TransferAuth.Password) ||
                        TransferConfig.AuthMode.Equals(TransferAuth.Both))
                        FtpClient.Credentials = new NetworkCredential(TransferConfig.UserName, TransferConfig.Password);

                    FtpClient.DataConnectionType =
                        TransferConfig.UsePassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;

                    FtpClient.TransferChunkSize = TransferConfig.BufferSize;
                    FtpClient.SocketKeepAlive = true;
                    FtpClient.SocketPollInterval = 15000;

                    if ((TransferConfig.AuthMode.Equals(TransferAuth.Certificate) ||
                         TransferConfig.AuthMode.Equals(TransferAuth.Both)) && File.Exists(TransferConfig.CertPath))
                    {
                        UseCert = true;
                        FtpClient.SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12;
                        FtpClient.EncryptionMode = FtpEncryptionMode.Explicit;
                        FtpClient.ClientCertificates.Add(new X509Certificate(TransferConfig.CertPath));
                        FtpClient.DataConnectionEncryption = true;
                    }
                    else
                    {
                        FtpClient.SslProtocols = SslProtocols.None;
                        FtpClient.EncryptionMode = FtpEncryptionMode.None;
                        FtpClient.DataConnectionEncryption = false;
                    }

                    break;
            }
        }

        /// <summary>
        ///     Transfer Configuration
        /// </summary>
        public TransferDestination TransferConfig { get; }

        /// <summary>
        ///     SFTP Client
        /// </summary>
        private SftpClient SftpClient { get; }

        private FtpClient FtpClient { get; }

        /// <summary>
        ///     Use a certificate for authentication
        /// </summary>
        public bool UseCert { get; }

        /// <summary>
        ///     Destination
        /// </summary>
        public string Destination { get; }


        /// <summary>
        ///     Connect to the configured server
        /// </summary>
        /// <returns></returns>
        public TransferResult Connect()
        {
            var transferResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};

            try
            {
                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        SftpClient.Connect();
                        break;
                    case TransferMode.Ftp:
                        FtpClient.Connect();
                        break;
                }
            }
            catch (Exception ex)
            {
                transferResult.ErrorDetails = ex;
                transferResult.Status = TransferStatus.Error;
            }


            return transferResult;
        }

        /// <summary>
        ///     Disconnect from the configured SFTP server
        /// </summary>
        /// <returns></returns>
        public TransferResult Disconnect()
        {
            var transferResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};

            try
            {
                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        SftpClient.Disconnect();
                        SftpClient.Dispose();
                        break;
                    case TransferMode.Ftp:
                        FtpClient.Disconnect();
                        FtpClient.Dispose();
                        break;
                }
            }
            catch (Exception ex)
            {
                transferResult.Status = TransferStatus.Error;
                transferResult.ErrorDetails = ex;
            }

            return transferResult;
        }

        /// <summary>
        ///     List remote files
        /// </summary>
        /// <param name="remotePath"></param>
        /// <param name="listFolders"></param>
        /// <returns></returns>
        public TransferResult ListFiles(string remotePath = null, bool listFolders = true)
        {
            var filePath = remotePath;
            if (string.IsNullOrEmpty(remotePath))
                filePath = !string.IsNullOrEmpty(TransferConfig.RemotePath)
                    ? TransferConfig.RemotePath
                    : "";

            var transferResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};
            var listFiles = new List<TransferInfo>();
            try
            {
                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp when !SftpClient.IsConnected:
                        SftpClient.Connect();
                        break;
                    case TransferMode.Ftp when !FtpClient.IsConnected:
                        FtpClient.Connect();
                        break;
                }

                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        var transferList = SftpClient.ListDirectory(filePath).ToList();
                        listFiles.AddRange(from file in transferList
                            where listFolders || file.IsRegularFile && !file.Name.StartsWith(".")
                            select new TransferInfo(file.Name, file.LastAccessTimeUtc, file.LastWriteTimeUtc,
                                file.Attributes.Size));
                        break;
                    case TransferMode.Ftp:
                        var ftpList = FtpClient.GetListing(filePath).ToList();
                        listFiles.AddRange(from file in ftpList
                            where listFolders || file.Type == FtpFileSystemObjectType.File && !file.Name.StartsWith(".")
                            select new TransferInfo(file.Name, file.Created, file.Modified, file.Size));
                        break;
                }
            }
            catch (Exception ex)
            {
                transferResult.Status = TransferStatus.Error;
                transferResult.ErrorDetails = ex;
            }

            transferResult.FileList = listFiles;

            return transferResult;
        }

        /// <summary>
        ///     Download files via FTP/SFTP
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="downloadPath"></param>
        /// <param name="destPath"></param>
        /// <returns></returns>
        public TransferResult DownloadFiles(string fileName, string downloadPath = null, string destPath = null)
        {
            var remotePath = downloadPath;
            if (string.IsNullOrEmpty(remotePath))
                remotePath = string.IsNullOrEmpty(TransferConfig.DestPath)
                    ? TransferConfig.DestPath
                    : Transfers.Config.DestPath;

            var filePath = destPath;
            if (string.IsNullOrEmpty(destPath))
                filePath = !string.IsNullOrEmpty(TransferConfig.RemotePath)
                    ? TransferConfig.RemotePath
                    : "";
            var transferResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};

            try
            {
                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp when !SftpClient.IsConnected:
                        SftpClient.Connect();
                        break;
                    case TransferMode.Ftp when !FtpClient.IsConnected:
                        FtpClient.Connect();
                        break;
                }

                var remoteFile = Url.Combine(remotePath, fileName);
                var destFile = Path.Combine(filePath, fileName);
                var transferFile = new FileStream(destFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite,
                    TransferConfig.BufferSize);

                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        var fileInfo = SftpClient.GetAttributes(remoteFile);
                        SftpClient.DownloadFile(remoteFile, transferFile);
                        transferResult.FileSize = fileInfo.Size;
                        break;
                    case TransferMode.Ftp:
                        var fileSize = FtpClient.GetFileSize(remoteFile);
                        FtpClient.Download(transferFile, remoteFile);
                        transferResult.FileSize = fileSize;
                        break;
                }

                transferFile.Close();
                transferFile.Dispose();
                transferResult.Status = TransferStatus.Success;
            }
            catch (Exception ex)
            {
                transferResult.Status = TransferStatus.Error;
                transferResult.ErrorDetails = ex;
            }

            return transferResult;
        }

        /// <summary>
        ///     Send all files from the destination or global source path
        /// </summary>
        /// <param name="doRetries"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public List<TransferResult> SendFiles(bool doRetries = false, bool overWrite = false)
        {
            var files = Files.GetFiles(string.IsNullOrEmpty(TransferConfig.SourcePath)
                ? Transfers.Config.SourcePath
                : TransferConfig.SourcePath);
            return SendFiles(files, doRetries, overWrite);
        }

        /// <summary>
        ///     Transfer a list of files using a list of string paths
        /// </summary>
        /// <param name="fileList"></param>
        /// <param name="doRetries"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public List<TransferResult> SendFiles(IEnumerable<string> fileList, bool doRetries = false,
            bool overWrite = false)
        {
            var files = (from file in fileList where File.Exists(file) select Files.GetFileInfo(file)).ToList();
            return SendFiles(files, doRetries, overWrite);
        }

        /// <summary>
        ///     Transfer a list of files using a list of TransferInfo
        /// </summary>
        /// <param name="files"></param>
        /// <param name="doRetries"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public List<TransferResult> SendFiles(List<TransferInfo> files, bool doRetries = false, bool overWrite = false)
        {
            var results = new List<TransferResult>();
            foreach (var file in files)
            {
                TransferResult result;
                var retries = 0;
                do
                {
                    retries++;
                    if (retries > 1)
                        Thread.Sleep(TransferConfig.RetryDelay);
                    result = SendFiles(file.FileName, file.FileName, overWrite);
                } while (doRetries && result.Status != TransferStatus.Success);

                results.Add(result);
            }

            return results;
        }

        /// <summary>
        ///     Send file via FTP/SFTP
        /// </summary>
        /// <param name="destFile"></param>
        /// <param name="localPath"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public TransferResult SendFiles(string destFile, string localPath = null, bool overWrite = false)
        {
            var filePath = localPath;
            if (string.IsNullOrEmpty(filePath))
                filePath = !string.IsNullOrEmpty(TransferConfig.SourcePath)
                    ? Path.Combine(TransferConfig.SourcePath, destFile)
                    : Path.Combine(Transfers.Config.SourcePath, destFile);
            var transferResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};
            var exists = false;

            try
            {
                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp when !SftpClient.IsConnected:
                        SftpClient.Connect();
                        break;
                    case TransferMode.Ftp when !FtpClient.IsConnected:
                        FtpClient.Connect();
                        break;
                }

                var transferPath = Url.Combine(TransferConfig.RemotePath, Path.GetFileName(destFile));
                var transferFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                    TransferConfig.BufferSize);

                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        //SSH.NET will return an exception if overwrite is false and we attempt to upload, so check before upload
                        exists = SftpClient.Exists(transferPath);
                        if (overWrite || !exists)
                            SftpClient.UploadFile(transferFile, transferPath, overWrite);
                        var fileInfo = SftpClient.GetAttributes(transferPath);
                        transferResult.FileSize = fileInfo.Size;
                        break;
                    case TransferMode.Ftp:
                        //Skip doesn't actually work as expected - it deletes the file, so we'll check before upload
                        exists = FtpClient.FileExists(transferPath);
                        if (overWrite || !exists)
                            FtpClient.Upload(transferFile, transferPath,
                                overWrite ? FtpRemoteExists.Skip : FtpRemoteExists.Overwrite, true);

                        transferResult.FileSize = FtpClient.GetFileSize(transferPath);
                        break;
                }

                transferFile.Close();
                transferFile.Dispose();
                if (!overWrite && exists)
                    transferResult.Status = TransferStatus.FileExists;
                else
                    transferResult.Status = TransferStatus.Success;
            }
            catch (Exception ex)
            {
                transferResult.Status = TransferStatus.Error;
                transferResult.ErrorDetails = ex;
            }

            return transferResult;
        }
    }
}