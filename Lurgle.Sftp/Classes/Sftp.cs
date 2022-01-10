using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flurl;
using Lurgle.Sftp.Enums;
using Renci.SshNet;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

// ReSharper disable UnusedMethodReturnValue.Global

namespace Lurgle.Sftp.Classes
{
    /// <summary>
    ///     SFTP instance
    /// </summary>
    public class Sftp
    {
        /// <summary>
        ///     SFTP instance
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sftpDestination"></param>
        public Sftp(SftpConfig config, Destination sftpDestination)
        {
            SftpConfig = config;
            Destination = sftpDestination;

            var authMethods = new List<AuthenticationMethod>();
            if ((SftpConfig.AuthMode.Equals(SftpAuth.Certificate) || SftpConfig.AuthMode.Equals(SftpAuth.Both)) &&
                File.Exists(SftpConfig.CertPath))
            {
                UseCert = true;
                PrivateKeyFile[] keyFile = {new PrivateKeyFile(SftpConfig.CertPath)};
                authMethods.Add(new PrivateKeyAuthenticationMethod(SftpConfig.UserName, keyFile));
            }

            if (SftpConfig.AuthMode.Equals(SftpAuth.Password) || SftpConfig.AuthMode.Equals(SftpAuth.Both))
                authMethods.Add(new PasswordAuthenticationMethod(SftpConfig.UserName, SftpConfig.Password));

            ConnectionInfo sftpConnection;

            if (!SftpConfig.UseProxy ||
                SftpConfig.UseProxy && string.IsNullOrEmpty(SftpConfig.ProxyServer))
            {
                sftpConnection = new ConnectionInfo(SftpConfig.Server, SftpConfig.Port, SftpConfig.UserName,
                    authMethods.ToArray());
            }
            else
            {
                if (!Enum.TryParse(SftpConfig.ProxyType, out ProxyTypes proxyType)) proxyType = ProxyTypes.Http;

                sftpConnection = new ConnectionInfo(SftpConfig.Server, SftpConfig.Port, SftpConfig.UserName,
                    proxyType, SftpConfig.ProxyServer, SftpConfig.ProxyPort, SftpConfig.ProxyUser,
                    SftpConfig.ProxyPassword, authMethods.ToArray());
            }

            SftpClient = new SftpClient(sftpConnection)
            {
                BufferSize = (uint) SftpConfig.BufferSize,
                KeepAliveInterval = new TimeSpan(0, 0, 15)
            };
        }

        /// <summary>
        ///     SFTP Configuration
        /// </summary>
        public SftpConfig SftpConfig { get; }

        /// <summary>
        ///     SFTP Client
        /// </summary>
        private SftpClient SftpClient { get; }

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
        public SftpResult Connect()
        {
            var sftpResult = new SftpResult(Destination, UseCert) {Status = SftpStatus.Success};

            try
            {
                SftpClient.Connect();
            }
            catch (Exception ex)
            {
                sftpResult.ErrorDetails = ex;
                sftpResult.Status = SftpStatus.Error;
            }

            return sftpResult;
        }

        /// <summary>
        ///     Disconnect from the configured SFTP server
        /// </summary>
        /// <returns></returns>
        public SftpResult Disconnect()
        {
            var sftpResult = new SftpResult(Destination, UseCert) {Status = SftpStatus.Success};

            try
            {
                SftpClient.Disconnect();
                SftpClient.Dispose();
            }
            catch (Exception ex)
            {
                sftpResult.Status = SftpStatus.Error;
                sftpResult.ErrorDetails = ex;
            }

            return sftpResult;
        }

        /// <summary>
        ///     List remote files
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="filePath"></param>
        /// <param name="listFolders"></param>
        /// <returns></returns>
        public SftpResult ListSftp(Destination destination, string filePath, bool listFolders = true)
        {
            var transferResult = new SftpResult(destination, UseCert) {Status = SftpStatus.Success};
            var listFiles = new List<SftpInfo>();
            try
            {
                if (!SftpClient.IsConnected) SftpClient.Connect();

                var sftpList = SftpClient.ListDirectory(filePath).ToList();
                listFiles.AddRange(from file in sftpList
                    where listFolders || file.IsRegularFile && !file.Name.StartsWith(".")
                    select new SftpInfo(file.Name, file.LastAccessTimeUtc, file.LastWriteTimeUtc,
                        file.Attributes.Size));
            }
            catch (Exception ex)
            {
                transferResult.Status = SftpStatus.Error;
                transferResult.ErrorDetails = ex;
            }

            transferResult.FileList = listFiles;

            return transferResult;
        }

        /// <summary>
        ///     Download files via SFTP
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="sourcePath"></param>
        /// <param name="fileName"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public SftpResult DownloadSftp(Destination destination, string sourcePath, string fileName, string filePath)
        {
            var transferResult = new SftpResult(destination, UseCert) {Status = SftpStatus.Success};

            try
            {
                if (!SftpClient.IsConnected) SftpClient.Connect();

                var destPath = Path.Combine(filePath, fileName);
                var transferFile = new FileStream(destPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite,
                    SftpConfig.BufferSize);
                var fileInfo = SftpClient.GetAttributes(Url.Combine(sourcePath, fileName));
                SftpClient.DownloadFile(Url.Combine(sourcePath, fileName), transferFile);
                transferResult.FileSize = fileInfo.Size;
                transferFile.Close();
                transferFile.Dispose();
                transferResult.Status = SftpStatus.Success;
            }
            catch (Exception ex)
            {
                transferResult.Status = SftpStatus.Error;
                transferResult.ErrorDetails = ex;
            }

            return transferResult;
        }

        /// <summary>
        ///     Send file via SFTP
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="filePath"></param>
        /// <param name="destFile"></param>
        /// <returns></returns>
        public SftpResult SendSftp(Destination destination, string filePath, string destFile)
        {
            var transferResult = new SftpResult(destination, UseCert) {Status = SftpStatus.Success};

            try
            {
                if (!SftpClient.IsConnected) SftpClient.Connect();

                var sftpPath = Url.Combine(SftpConfig.Path, Path.GetFileName(destFile));

                var transferFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                    SftpConfig.BufferSize);
                SftpClient.UploadFile(transferFile, sftpPath, true);
                var fileInfo = SftpClient.GetAttributes(sftpPath);
                transferResult.FileSize = fileInfo.Size;
                transferFile.Close();
                transferFile.Dispose();
                transferResult.Status = SftpStatus.Success;
            }
            catch (Exception ex)
            {
                transferResult.Status = SftpStatus.Error;
                transferResult.ErrorDetails = ex;
            }

            return transferResult;
        }
    }
}