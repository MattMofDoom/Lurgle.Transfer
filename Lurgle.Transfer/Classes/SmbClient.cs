using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Lurgle.Transfer.Enums;
using SMBLibrary;
using SMBLibrary.Client;
using FileAttributes = SMBLibrary.FileAttributes;

namespace Lurgle.Transfer.Classes
{
    /// <summary>
    ///     SMB client for file transfers
    /// </summary>
    public class SmbClient
    {
        private readonly FileTransfer _fileTransfer;

        /// <summary>
        ///     SmbClient constructor
        /// </summary>
        /// <param name="fileTransfer"></param>
        public SmbClient(FileTransfer fileTransfer)
        {
            _fileTransfer = fileTransfer;
        }

        private SMB1Client Smb1Client { get; } = new SMB1Client();
        private SMB2Client Smb2Client { get; } = new SMB2Client();

        /// <summary>
        ///     Connect to remote server
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            bool connect;
            if (IPAddress.TryParse(_fileTransfer.TransferConfig.Server, out var address))
                connect = _fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1
                    ? Smb1Client.Connect(address,
                        _fileTransfer.TransferConfig.Port == 135
                            ? SMBTransportType.NetBiosOverTCP
                            : SMBTransportType.DirectTCPTransport)
                    : Smb2Client.Connect(address,
                        _fileTransfer.TransferConfig.Port == 135
                            ? SMBTransportType.NetBiosOverTCP
                            : SMBTransportType.DirectTCPTransport);
            else
                connect = _fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1
                    ? Smb1Client.Connect(_fileTransfer.TransferConfig.Server,
                        _fileTransfer.TransferConfig.Port == 135
                            ? SMBTransportType.NetBiosOverTCP
                            : SMBTransportType.DirectTCPTransport)
                    : Smb2Client.Connect(_fileTransfer.TransferConfig.Server,
                        _fileTransfer.TransferConfig.Port == 135
                            ? SMBTransportType.NetBiosOverTCP
                            : SMBTransportType.DirectTCPTransport);

            if (!connect)
            {
                CheckStatus(NTStatus.STATUS_NOT_FOUND);
            }
            else
            {
                NTStatus status;
                if (_fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1)
                    status = Smb1Client.Login(GetDomain(_fileTransfer.TransferConfig.UserName),
                        GetUserName(_fileTransfer.TransferConfig.UserName),
                        _fileTransfer.TransferConfig.Password);
                else
                    status = Smb2Client.Login(GetDomain(_fileTransfer.TransferConfig.UserName),
                        GetUserName(_fileTransfer.TransferConfig.UserName),
                        _fileTransfer.TransferConfig.Password);

                CheckStatus(status);
            }

            return connect;
        }

        /// <summary>
        ///     Disconnect from remote server
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            if (_fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1)
            {
                CheckStatus(Smb1Client.Logoff());
                Smb1Client.Disconnect();
            }
            else
            {
                CheckStatus(Smb2Client.Logoff());
                Smb2Client.Disconnect();
            }

            return false;
        }

        /// <summary>
        ///     Retrieve a list of files from remote server
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="listFolders"></param>
        /// <returns></returns>
        public IEnumerable<TransferInfo> ListFiles(string filePath, bool listFolders)
        {
            var listFiles = new List<TransferInfo>();
            NTStatus status;
            var fileStore = _fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1
                ? Smb1Client.TreeConnect(GetShare(filePath), out status)
                : Smb2Client.TreeConnect(GetShare(filePath), out status);
            CheckStatus(status);

            CheckStatus(fileStore.CreateFile(out var folder, out _, GetFolder(filePath),
                AccessMask.GENERIC_READ,
                FileAttributes.Directory, ShareAccess.Read, CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_DIRECTORY_FILE, null));

            CheckStatus(fileStore.QueryDirectory(out var fileList, folder, "*",
                FileInformationClass.FileDirectoryInformation));

            CheckStatus(fileStore.CloseFile(folder));

            listFiles.AddRange(from FileDirectoryInformation fileInfo in fileList
                where listFolders && !fileInfo.FileName.StartsWith(".") ||
                      fileInfo.FileAttributes != FileAttributes.Directory
                select new TransferInfo(fileInfo.FileName, fileInfo.LastAccessTime, fileInfo.ChangeTime,
                    fileInfo.EndOfFile, GetInfoType(fileInfo.FileAttributes)));

            return listFiles;
        }

        /// <summary>
        ///     Download a file from remote server
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="remotePath"></param>
        /// <param name="transferFile"></param>
        /// <returns></returns>
        public TransferResult GetFile(string fileName, string remotePath, Stream transferFile)
        {
            NTStatus status;
            var fileStore = _fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1
                ? Smb1Client.TreeConnect(GetShare(remotePath), out status)
                : Smb2Client.TreeConnect(GetShare(remotePath), out status);
            CheckStatus(status);

            var remoteFile = Path.Combine(GetFolder(remotePath), fileName);
            CheckStatus(fileStore.CreateFile(out var fileHandle, out _, remoteFile,
                AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.Read,
                CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null));

            CheckStatus(fileStore.GetFileInformation(out var fileAttributes, fileHandle,
                FileInformationClass.FileStandardInformation));

            var size = ((FileStandardInformation) fileAttributes).EndOfFile;

            CheckStatus(fileStore.GetFileInformation(out fileAttributes, fileHandle,
                FileInformationClass.FileBasicInformation));

            var accessTime = ((FileBasicInformation) fileAttributes).LastAccessTime.Time;
            var modifyTime = ((FileBasicInformation) fileAttributes).ChangeTime.Time;
            DateTime accessDate;
            DateTime modifiedDate;
            if (accessTime != null && modifyTime != null)
            {
                accessDate = (DateTime) accessTime;
                modifiedDate = (DateTime) modifyTime;
            }
            else
            {
                throw new Exception("Unable to read remote file attributes after transfer");
            }

            byte[] data;
            long bytesRead = 0;

            do
            {
                status = fileStore.ReadFile(out data, fileHandle, bytesRead,
                    _fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1
                        ? (int) Smb1Client.MaxReadSize
                        : (int) Smb2Client.MaxReadSize);

                if (status == NTStatus.STATUS_END_OF_FILE || data.Length == 0) continue;
                bytesRead += data.Length;
                transferFile.Write(data, 0, data.Length);
            } while (status != NTStatus.STATUS_END_OF_FILE && data.Length != 0);

            CheckStatus(fileStore.CloseFile(fileHandle));
            CheckStatus(fileStore.Disconnect());

            var transferResult = new TransferResult(_fileTransfer.Destination, false)
            {
                LastFile = fileName,
                FileSize = size,
                FileList =
                {
                    new TransferInfo(fileName, accessDate, modifiedDate, size, InfoType.File)
                },
                Status = TransferStatus.Success
            };

            return transferResult;
        }

        /// <summary>
        ///     Upload a file to remote server
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="transferFile"></param>
        /// <param name="remotePath"></param>
        /// <param name="overWrite"></param>
        /// <returns></returns>
        public TransferResult SendFile(string fileName, Stream transferFile, string remotePath, bool overWrite)
        {
            NTStatus status;
            var fileStore = _fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1
                ? Smb1Client.TreeConnect(GetShare(remotePath), out status)
                : Smb2Client.TreeConnect(GetShare(remotePath), out status);
            CheckStatus(status);

            var remoteFile = Path.Combine(GetFolder(remotePath), fileName);
            var remoteList = ListFiles(remotePath, false);

            foreach (var file in remoteList.Where(file =>
                         file.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase) && !overWrite))
                return new TransferResult(_fileTransfer.Destination, false)
                {
                    LastFile = file.FileName,
                    FileSize = file.Size,
                    FileList =
                    {
                        new TransferInfo(fileName, file.AccessedDate, file.ModifiedDate, file.Size, file.Type)
                    },
                    Status = TransferStatus.FileExists
                };


            CheckStatus(fileStore.CreateFile(out var fileHandle, out _, remoteFile,
                AccessMask.GENERIC_WRITE | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.None,
                overWrite ? CreateDisposition.FILE_OVERWRITE_IF : CreateDisposition.FILE_CREATE,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null));

            long writeOffset = 0;
            var maxWrite = _fileTransfer.TransferConfig.TransferMode == TransferMode.Smb1
                ? (int) Smb1Client.MaxWriteSize
                : (int) Smb2Client.MaxWriteSize;

            do
            {
                var data = new byte[maxWrite];
                var bytesRead = transferFile.Read(data, 0, data.Length);
                if (bytesRead < maxWrite)
                    Array.Resize(ref data, bytesRead);

                CheckStatus(fileStore.WriteFile(out _, fileHandle, writeOffset, data));
                writeOffset += bytesRead;
            } while (transferFile.Position < transferFile.Length);

            CheckStatus(fileStore.CloseFile(fileHandle));

            CheckStatus(fileStore.CreateFile(out fileHandle, out _, remoteFile,
                AccessMask.GENERIC_READ | AccessMask.SYNCHRONIZE, FileAttributes.Normal, ShareAccess.Read,
                CreateDisposition.FILE_OPEN,
                CreateOptions.FILE_NON_DIRECTORY_FILE | CreateOptions.FILE_SYNCHRONOUS_IO_ALERT, null));

            CheckStatus(fileStore.GetFileInformation(out var fileAttributes, fileHandle,
                FileInformationClass.FileBasicInformation));

            CheckStatus(fileStore.CloseFile(fileHandle));

            CheckStatus(fileStore.Disconnect());

            var accessTime = ((FileBasicInformation) fileAttributes).LastAccessTime.Time;
            var modifyTime = ((FileBasicInformation) fileAttributes).ChangeTime.Time;
            DateTime accessDate;
            DateTime modifiedDate;
            if (accessTime != null && modifyTime != null)
            {
                accessDate = (DateTime) accessTime;
                modifiedDate = (DateTime) modifyTime;
            }
            else
            {
                throw new Exception("Unable to read remote file attributes after transfer");
            }


            var transferResult = new TransferResult(_fileTransfer.Destination, false)
            {
                LastFile = fileName,
                FileSize = transferFile.Length,
                FileList =
                {
                    new TransferInfo(fileName, accessDate, modifiedDate, transferFile.Length, InfoType.File)
                },
                Status = TransferStatus.Success
            };
            return transferResult;
        }

        /// <summary>
        ///     Return the domain name portion of DOMAIN\userName or userName@domain
        /// </summary>
        /// <param name="userName">DOMAIN\userName or userName@domain</param>
        /// <returns></returns>
        private static string GetDomain(string userName)
        {
            var pos = userName.IndexOf("\\", StringComparison.Ordinal);
            if (!pos.Equals(-1))
                return userName.Substring(0, pos);
            pos = userName.IndexOf("@", StringComparison.OrdinalIgnoreCase);
            return !pos.Equals(-1) ? userName.Substring(pos + 1) : string.Empty;
        }

        /// <summary>
        ///     Return the user name portion of DOMAIN\userName
        /// </summary>
        /// <param name="userName">DOMAIN\userName</param>
        /// <returns></returns>
        private static string GetUserName(string userName)
        {
            return !string.IsNullOrEmpty(GetDomain(userName))
                ? userName.Contains("@") ? GetUserNameFromUpn(userName) :
                userName.Substring(userName.IndexOf("\\", StringComparison.Ordinal) + 1)
                : userName;
        }

        /// <summary>
        ///     Returns the user name portion of username@domain
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private static string GetUserNameFromUpn(string userName)
        {
            var pos = userName.IndexOf("@", StringComparison.OrdinalIgnoreCase);
            return !pos.Equals(-1) ? userName.Substring(0, pos) : userName;
        }

        private static void CheckStatus(NTStatus status)
        {
            if (status != NTStatus.STATUS_SUCCESS && status != NTStatus.STATUS_NO_MORE_FILES &&
                status != NTStatus.STATUS_END_OF_FILE)
                throw new Exception(
                    $"File operation failed, Error {status.ToString()}, HResult 0x{ToHResult(status):X}");
        }

        /// <summary>
        ///     Converts an NTStatus to an HRESULT.
        /// </summary>
        /// <param name="status">The <see cref="NTStatus" /> to convert.</param>
        /// <returns>The HRESULT.</returns>
        private static int ToHResult(NTStatus status)
        {
            // From winerror.h
            // #define HRESULT_FROM_NT(x)      ((HRESULT) ((x) | FACILITY_NT_BIT))
            return (int) status | 0x10000000;
        }

        private static string GetShare(string path)
        {
            var pos = -1;
            if (path.Contains("\\"))
                pos = path.IndexOf("\\", StringComparison.OrdinalIgnoreCase);
            else if (path.Contains("/"))
                pos = path.IndexOf("/", StringComparison.OrdinalIgnoreCase);
            return pos > 1 ? path.Substring(0, pos) : path;
        }

        private static string GetFolder(string path)
        {
            var pos = -1;
            if (path.Contains("\\"))
                pos = path.IndexOf("\\", StringComparison.OrdinalIgnoreCase);
            else if (path.Contains("/"))
                pos = path.IndexOf("/", StringComparison.OrdinalIgnoreCase);
            return pos > 1 ? path.Substring(pos + 1) : "";
        }

        private static InfoType GetInfoType(FileAttributes attributes)
        {
            switch (attributes)
            {
                case FileAttributes.Directory:
                    return InfoType.Directory;
                case FileAttributes.ReparsePoint:
                case FileAttributes.SparseFile:
                case FileAttributes.Offline:
                    return InfoType.Link;
                case FileAttributes.Normal:
                case FileAttributes.Archive:
                case FileAttributes.Compressed:
                case FileAttributes.Encrypted:
                case FileAttributes.Hidden:
                case FileAttributes.ReadOnly:
                case FileAttributes.System:
                case FileAttributes.Temporary:
                    return InfoType.File;
                case FileAttributes.NotContentIndexed:
                case FileAttributes.IntegrityStream:
                case FileAttributes.NoScrubData:
                default:
                    return InfoType.Other;
            }
        }
    }
}