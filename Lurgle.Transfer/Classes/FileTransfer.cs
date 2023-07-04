using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using FluentFTP;
using FluentFTP.Proxy.SyncProxy;
using Flurl;
using Lurgle.Transfer.Enums;
using Renci.SshNet;
// ReSharper disable ClassNeverInstantiated.Global

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
                        var proxy = new FtpProxyProfile
                        {
                            ProxyHost = TransferConfig.ProxyServer,
                            ProxyPort = TransferConfig.ProxyPort
                        };

                        if (!string.IsNullOrEmpty(TransferConfig.ProxyUser) &&
                            !string.IsNullOrEmpty(TransferConfig.ProxyPassword))
                            proxy.ProxyCredentials =
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

                    FtpClient.Config.DataConnectionType =
                        TransferConfig.UsePassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;

                    FtpClient.Config.TransferChunkSize = TransferConfig.BufferSize;
                    FtpClient.Config.SocketKeepAlive = true;
                    FtpClient.Config.SocketPollInterval = 15000;

                    if ((TransferConfig.AuthMode.Equals(TransferAuth.Certificate) ||
                         TransferConfig.AuthMode.Equals(TransferAuth.Both)) && File.Exists(TransferConfig.CertPath))
                    {
                        UseCert = true;
                        FtpClient.Config.SslProtocols = SslProtocols.Tls11 | SslProtocols.Tls12;
                        FtpClient.Config.EncryptionMode = FtpEncryptionMode.Explicit;
                        FtpClient.Config.ClientCertificates.Add(new X509Certificate(TransferConfig.CertPath));
                        FtpClient.Config.DataConnectionEncryption = true;
                    }
                    else
                    {
                        FtpClient.Config.SslProtocols = SslProtocols.None;
                        FtpClient.Config.EncryptionMode = FtpEncryptionMode.None;
                        FtpClient.Config.DataConnectionEncryption = false;
                    }

                    break;
                case TransferMode.Smb1:
                case TransferMode.Smb2:
                case TransferMode.Smb3:
                    SmbClient = new SmbClient(this);
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

        private SmbClient SmbClient { get; }

        private bool SmbConnected { get; set; }


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
                    case TransferMode.Smb1:
                    case TransferMode.Smb2:
                    case TransferMode.Smb3:
                        SmbConnected = SmbClient.Connect();
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
                    case TransferMode.Smb1:
                    case TransferMode.Smb2:
                    case TransferMode.Smb3:
                        SmbConnected = SmbClient.Disconnect();
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
                    case TransferMode.Ftp when !FtpClient.IsConnected:
                    case TransferMode.Smb1 when !SmbConnected:
                    case TransferMode.Smb2 when !SmbConnected:
                    case TransferMode.Smb3 when !SmbConnected:
                        Connect();
                        break;
                }

                switch (TransferConfig.TransferMode)
                {
                    case TransferMode.Sftp:
                        var transferList = SftpClient.ListDirectory(filePath).ToList();
                        listFiles.AddRange(from file in transferList
                            where listFolders || file.IsRegularFile && !file.Name.StartsWith(".")
                            select new TransferInfo(file.Name, file.LastAccessTimeUtc, file.LastWriteTimeUtc,
                                file.Attributes.Size,
                                file.IsRegularFile ? InfoType.File :
                                file.IsDirectory ? InfoType.Directory :
                                file.IsSymbolicLink ? InfoType.Link : InfoType.Other));
                        break;
                    case TransferMode.Ftp:
                        var ftpList = FtpClient.GetListing(filePath).ToList();
                        listFiles.AddRange(from file in ftpList
                            where listFolders || file.Type == FtpObjectType.File && !file.Name.StartsWith(".")
                            select new TransferInfo(file.Name, file.Created, file.Modified, file.Size,
                                file.Type == FtpObjectType.File ? InfoType.File :
                                file.Type == FtpObjectType.Directory ? InfoType.Directory : InfoType.Link));
                        break;
                    case TransferMode.Smb1:
                    case TransferMode.Smb2:
                    case TransferMode.Smb3:
                        listFiles.AddRange(SmbClient.ListFiles(filePath, listFolders));
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
        ///     Download all files from the remote path
        /// </summary>
        /// <param name="doRetries"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public TransferResult DownloadFiles(bool doRetries = false, bool overWrite = false)
        {
            var files = ListFiles(string.IsNullOrEmpty(TransferConfig.RemotePath)
                ? TransferConfig.RemotePath
                : "").FileList;
            return DownloadFiles(files, doRetries, overWrite);
        }

        /// <summary>
        ///     Transfer a list of files using a list of string paths
        /// </summary>
        /// <param name="files"></param>
        /// <param name="doRetries"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public TransferResult DownloadFiles(List<TransferInfo> files, bool doRetries = false,
            bool overWrite = false)
        {
            var result = new TransferResult(TransferConfig.Destination, UseCert);

            foreach (var file in files)
            {
                var retries = 0;
                do
                {
                    retries++;
                    if (retries > 1)
                        Thread.Sleep(TransferConfig.RetryDelay);
                    var fileResult = DownloadFile(file.FileName,
                        string.IsNullOrEmpty(TransferConfig.RemotePath)
                            ? TransferConfig.RemotePath
                            : "",
                        string.IsNullOrEmpty(TransferConfig.DestPath)
                            ? TransferConfig.DestPath
                            : Transfers.Config.DestPath, overWrite);

                    result.Status = fileResult.Status;
                    result.FileList.AddRange(fileResult.FileList);
                    result.LastFile = file.FileName;
                    result.FileSize = fileResult.FileSize;
                } while (doRetries && result.Status != TransferStatus.Success);
            }

            return result;
        }

        /// <summary>
        ///     Download a single file via supported transfer method
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="downloadPath"></param>
        /// <param name="destPath"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        [Obsolete("Use DownloadFile for a single file")]
        public TransferResult DownloadFiles(string fileName, string downloadPath = null, string destPath = null,
            bool overWrite = true)
        {
            return DownloadFile(fileName, downloadPath, destPath, overWrite);
        }

        /// <summary>
        ///     Download a single files via supported transfer method
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="downloadPath"></param>
        /// <param name="destPath"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public TransferResult DownloadFile(string fileName, string downloadPath = null, string destPath = null,
            bool overWrite = true)
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
                    case TransferMode.Ftp when !FtpClient.IsConnected:
                    case TransferMode.Smb1 when !SmbConnected:
                    case TransferMode.Smb2 when !SmbConnected:
                    case TransferMode.Smb3 when !SmbConnected:
                        Connect();
                        break;
                }

                var remoteFile = Url.Combine(remotePath, fileName);
                var destFile = Path.Combine(filePath, fileName);
                if (overWrite | !File.Exists(destFile))
                {
                    var transferFile = new FileStream(destFile, FileMode.Create, FileAccess.ReadWrite,
                        FileShare.ReadWrite,
                        TransferConfig.BufferSize);

                    switch (TransferConfig.TransferMode)
                    {
                        case TransferMode.Sftp:
                            SftpClient.DownloadFile(remoteFile, transferFile);
                            break;
                        case TransferMode.Ftp:
                            FtpClient.DownloadStream(transferFile, remoteFile);
                            break;
                        case TransferMode.Smb1:
                        case TransferMode.Smb2:
                        case TransferMode.Smb3:
                            transferResult = SmbClient.GetFile(fileName, remotePath, transferFile);
                            break;
                    }

                    transferFile.Close();
                    transferFile.Dispose();
                    if (TransferConfig.TransferMode == TransferMode.Sftp ||
                        TransferConfig.TransferMode == TransferMode.Ftp)
                        transferResult = new TransferResult(Destination, UseCert)
                        {
                            FileList = {Files.GetFileInfo(destFile)},
                            Status = TransferStatus.Success,
                            LastFile = fileName,
                            FileSize = Files.GetFileInfo(destFile).Size
                        };
                }
                else
                {
                    transferResult = new TransferResult(Destination, UseCert)
                    {
                        FileList = {Files.GetFileInfo(destFile)},
                        Status = TransferStatus.FileExists,
                        LastFile = fileName,
                        FileSize = Files.GetFileInfo(destFile).Size
                    };
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
        ///     Send all files from the destination or global source path
        /// </summary>
        /// <param name="doRetries"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public TransferResult SendFiles(bool doRetries = false, bool overWrite = false)
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
        public TransferResult SendFiles(IEnumerable<string> fileList, bool doRetries = false,
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
        public TransferResult SendFiles(List<TransferInfo> files, bool doRetries = false, bool overWrite = false)
        {
            var result = new TransferResult(TransferConfig.Destination, UseCert);
            foreach (var file in files)
            {
                var retries = 0;
                TransferResult fileResult;

                do
                {
                    retries++;
                    if (retries > 1)
                        Thread.Sleep(TransferConfig.RetryDelay);
                    fileResult = SendFile(file.FileName, file.FileName, overWrite);
                } while (doRetries && result.Status != TransferStatus.Success);

                result.Status = fileResult.Status;
                result.FileList.AddRange(fileResult.FileList);
                result.LastFile = file.FileName;
                result.FileSize = fileResult.FileSize;
            }

            return result;
        }

        /// <summary>
        /// Send a single file via supported transfer method
        /// </summary>
        /// <param name="destFile"></param>
        /// <param name="localPath"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        [Obsolete("Use SendFile for a single file")]
        public TransferResult SendFiles(string destFile, string localPath = null, bool overWrite = false)
        {
            return SendFile(destFile, localPath, overWrite);
        }

        /// <summary>
        ///     Send file via FTP/SFTP
        /// </summary>
        /// <param name="destFile"></param>
        /// <param name="localPath"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public TransferResult SendFile(string destFile, string localPath = null, bool overWrite = false)
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
                    case TransferMode.Ftp when !FtpClient.IsConnected:
                    case TransferMode.Smb1 when !SmbConnected:
                    case TransferMode.Smb2 when !SmbConnected:
                    case TransferMode.Smb3 when !SmbConnected:
                        Connect();
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
                        transferResult.LastFile = Path.GetFileName(destFile);
                        transferResult.FileSize = fileInfo.Size;
                        transferResult.FileList.Add(new TransferInfo(Path.GetFileName(destFile),
                            fileInfo.LastAccessTime,
                            fileInfo.LastWriteTime, fileInfo.Size, InfoType.File));
                        break;
                    case TransferMode.Ftp:
                        //Skip doesn't actually work as expected - it deletes the file, so we'll check before upload
                        exists = FtpClient.FileExists(transferPath);
                        if (overWrite || !exists)
                            FtpClient.UploadStream(transferFile, transferPath,
                                overWrite ? FtpRemoteExists.Skip : FtpRemoteExists.Overwrite, true);

                        var remoteList = ListFiles(TransferConfig.RemotePath, false).FileList.Where(file =>
                            file.FileName.Equals(Path.GetFileName(destFile), StringComparison.OrdinalIgnoreCase));
                        transferResult.LastFile = Path.GetFileName(destFile);
                        transferResult.FileSize = FtpClient.GetFileSize(transferPath);
                        transferResult.FileList.AddRange(remoteList);
                        break;
                    case TransferMode.Smb1:
                    case TransferMode.Smb2:
                    case TransferMode.Smb3:
                        transferResult = SmbClient.SendFile(Path.GetFileName(destFile), transferFile,
                            TransferConfig.RemotePath, overWrite);
                        break;
                }

                transferFile.Close();
                transferFile.Dispose();
                if ((TransferConfig.TransferMode == TransferMode.Sftp ||
                     TransferConfig.TransferMode == TransferMode.Ftp) && !overWrite && exists)
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