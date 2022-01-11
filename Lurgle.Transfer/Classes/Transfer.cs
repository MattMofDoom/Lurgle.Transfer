using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
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
    public class Transfer
    {
        /// <summary>
        ///     SFTP/FTP instance
        /// </summary>
        /// <param name="config"></param>
        public Transfer(TransferDestination config)
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

                    if (TransferConfig.AuthMode.Equals(TransferAuth.Password) || TransferConfig.AuthMode.Equals(TransferAuth.Both))
                        authMethods.Add(new PasswordAuthenticationMethod(TransferConfig.UserName, TransferConfig.Password));

                    ConnectionInfo sftpConnection;

                    if (!TransferConfig.UseProxy ||
                        TransferConfig.UseProxy && string.IsNullOrEmpty(TransferConfig.ProxyServer))
                    {
                        sftpConnection = new ConnectionInfo(TransferConfig.Server, TransferConfig.Port, TransferConfig.UserName,
                            authMethods.ToArray());
                    }
                    else
                    {
                        if (!Enum.TryParse(TransferConfig.ProxyType, out ProxyTypes proxyType)) proxyType = ProxyTypes.Http;

                        sftpConnection = new ConnectionInfo(TransferConfig.Server, TransferConfig.Port, TransferConfig.UserName,
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
                            proxy.Credentials = new NetworkCredential(TransferConfig.ProxyUser, TransferConfig.ProxyPassword);

                        FtpClient = new FtpClientHttp11Proxy(proxy);
                    }
                    else
                    {
                        FtpClient = new FtpClient();
                    }

                    FtpClient.Host = TransferConfig.Server;
                    FtpClient.Port = TransferConfig.Port;

                    if (TransferConfig.AuthMode.Equals(TransferAuth.Password) || TransferConfig.AuthMode.Equals(TransferAuth.Both))
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
        ///     SFTP Configuration
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
        public Destination Destination { get; }


        /// <summary>
        ///     Connect to the configured SFTP server
        /// </summary>
        /// <returns></returns>
        public TransferResult Connect()
        {
            var sftpResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};

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
                sftpResult.ErrorDetails = ex;
                sftpResult.Status = TransferStatus.Error;
            }


            return sftpResult;
        }

        /// <summary>
        ///     Disconnect from the configured SFTP server
        /// </summary>
        /// <returns></returns>
        public TransferResult Disconnect()
        {
            var sftpResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};

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
                sftpResult.Status = TransferStatus.Error;
                sftpResult.ErrorDetails = ex;
            }

            return sftpResult;
        }

        /// <summary>
        ///     List remote files
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="listFolders"></param>
        /// <returns></returns>
        public TransferResult ListSftp(string filePath, bool listFolders = true)
        {
            var transferResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};
            var listFiles = new List<TransferInfo>();
            try
            {
                if (!SftpClient.IsConnected) SftpClient.Connect();

                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        var sftpList = SftpClient.ListDirectory(filePath).ToList();
                        listFiles.AddRange(from file in sftpList
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
        ///     Download files via SFTP
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public TransferResult DownloadSftp(string sourcePath, string fileName, string filePath)
        {
            var transferResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};

            try
            {
                if (!SftpClient.IsConnected) SftpClient.Connect();

                var remotePath = Url.Combine(sourcePath, fileName);
                var destPath = Path.Combine(filePath, fileName);
                var transferFile = new FileStream(destPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite,
                    TransferConfig.BufferSize);

                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        var fileInfo = SftpClient.GetAttributes(remotePath);
                        SftpClient.DownloadFile(remotePath, transferFile);
                        transferResult.FileSize = fileInfo.Size;
                        break;
                    case TransferMode.Ftp:
                        var fileSize = FtpClient.GetFileSize(remotePath);
                        FtpClient.Download(transferFile, remotePath);
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
        ///     Send file via SFTP
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="destFile"></param>
        /// <returns></returns>
        public TransferResult SendSftp(string filePath, string destFile)
        {
            var transferResult = new TransferResult(Destination, UseCert) {Status = TransferStatus.Success};

            try
            {
                if (!SftpClient.IsConnected) SftpClient.Connect();

                var sftpPath = Url.Combine(TransferConfig.Path, Path.GetFileName(destFile));
                var transferFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                    TransferConfig.BufferSize);

                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        SftpClient.UploadFile(transferFile, sftpPath, true);
                        var fileInfo = SftpClient.GetAttributes(sftpPath);
                        transferResult.FileSize = fileInfo.Size;
                        break;
                    case TransferMode.Ftp:
                        FtpClient.Upload(transferFile, sftpPath, FtpRemoteExists.Overwrite, true);
                        transferResult.FileSize = FtpClient.GetFileSize(sftpPath);
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
    }
}