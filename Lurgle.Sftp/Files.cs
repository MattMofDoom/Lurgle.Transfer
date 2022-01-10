using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Lurgle.Sftp.Classes;
using Lurgle.Sftp.Enums;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

// ReSharper disable UnusedMember.Global

namespace Lurgle.Sftp
{
    /// <summary>
    ///     Local file handling
    /// </summary>
    public static class Files
    {
        /// <summary>
        ///     Successful folder validation results
        /// </summary>
        public static readonly List<FolderResult> FolderSuccess = new List<FolderResult>
            {FolderResult.FolderCreated, FolderResult.FolderExists};

        /// <summary>
        ///     Extension of Path.GetFileName for use in RazorLight templates
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileName(string filePath)
        {
            return Path.GetFileName(filePath);
        }

        /// <summary>
        ///     Validate a given filePath
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static FolderResult GetFolder(string filePath)
        {
            if (Directory.Exists(filePath)) return FolderResult.FolderExists;
            try
            {
                Directory.CreateDirectory(filePath);
            }
            catch (Exception)
            {
                return FolderResult.ErrorCreatingFolder;
            }

            return FolderResult.FolderCreated;
        }

        /// <summary>
        ///     Retrieve file attributes
        /// </summary>
        /// <param name="sourceFile"></param>
        /// <returns></returns>
        public static SftpInfo GetFileInfo(string sourceFile)
        {
            var fileAccess = File.GetLastAccessTimeUtc(sourceFile);
            var fileModify = File.GetLastWriteTimeUtc(sourceFile);
            var fileSize = new FileInfo(sourceFile).Length;
            return new SftpInfo(sourceFile, fileAccess, fileModify, fileSize);
        }

        /// <summary>
        ///     Retrieve file attributes
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <returns></returns>
        public static List<SftpInfo> GetFileInfo(IEnumerable<string> sourceFiles)
        {
            return (from file in sourceFiles
                let fileAccess = File.GetLastAccessTimeUtc(file)
                let fileModify = File.GetLastWriteTimeUtc(file)
                let fileSize = new FileInfo(file).Length
                select new SftpInfo(file, fileAccess, fileModify, fileSize)).ToList();
        }

        /// <summary>
        ///     Retrieve a list of files and attributes from a given path
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public static List<SftpInfo> GetFiles(string sourcePath)
        {
            return GetFileInfo(Directory.GetFiles(sourcePath).ToList());
        }

        /// <summary>
        ///     Retrieve a list of files from a given path
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static List<string> GetFileList(IEnumerable<SftpInfo> fileInfo)
        {
            return fileInfo.Select(file => Path.GetFileName(file.FileName)).ToList();
        }

        /// <summary>
        ///     Add a random number to the beginning of a file name
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="appendType"></param>
        /// <returns></returns>
        public static string GetNewName(string filePath, AppendType appendType = AppendType.Prefix)
        {
            var fileName = Path.GetFileName(filePath);
            var destPath = Path.GetDirectoryName(filePath);

            if (appendType.Equals(AppendType.Prefix))
                fileName = $"{GetRandomNumber()}-{fileName}";
            else
                fileName = Path.ChangeExtension(
                    $"{Path.GetFileNameWithoutExtension(fileName)}-{GetRandomNumber()}",
                    Path.GetExtension(fileName));

            destPath = destPath != null ? Path.Combine(destPath, fileName) : fileName;

            return destPath;
        }

        /// <summary>
        ///     returns random number
        /// </summary>
        /// <returns></returns>
        private static int GetRandomNumber()
        {
            var randomGen = new Random((int) DateTime.Now.Ticks & 0x0000FFFF);
            return randomGen.Next();
        }

        /// <summary>
        ///     Compress files given a configured compression type
        /// </summary>
        /// <param name="compressType"></param>
        /// <param name="sourceFiles"></param>
        /// <param name="useConverted"></param>
        /// <param name="sourcePath"></param>
        /// <param name="zipPrefix"></param>
        /// <returns></returns>
        public static CompressResult CompressFiles(CompressType compressType, IEnumerable<SftpInfo> sourceFiles,
            bool useConverted, string sourcePath, string zipPrefix = null)
        {
            var compressResult = new CompressResult(compressType);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (compressType)
            {
                case CompressType.Gzip:
                    compressResult = CompressGzip(sourceFiles, useConverted);
                    break;
                case CompressType.Zip:
                    compressResult = CompressZip(sourceFiles, useConverted, sourcePath, zipPrefix);
                    break;
                case CompressType.ZipPerFile:
                    compressResult = CompressZipMulti(sourceFiles, useConverted);
                    break;
                default:
                    compressResult.Status = CompressStatus.NotCompressed;
                    compressResult.SourceFiles = sourceFiles as List<SftpInfo>;
                    compressResult.DestFiles = sourceFiles as List<SftpInfo>;
                    break;
            }

            return compressResult;
        }

        /// <summary>
        ///     Compress files to Gzip archives
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <param name="useConverted"></param>
        /// <returns></returns>
        private static CompressResult CompressGzip(IEnumerable<SftpInfo> sourceFiles, bool useConverted)
        {
            var compressResult = new CompressResult(CompressType.Gzip) {Status = CompressStatus.Success};

            foreach (var sourceFile in sourceFiles)
                if (!Path.GetExtension(sourceFile.FileName)
                    .Equals(".gz", StringComparison.CurrentCultureIgnoreCase))
                    compressResult.SourceFiles.Add(sourceFile);

            //Compress each file for remote transfer
            foreach (var fileInfo in compressResult.SourceFiles)
                try
                {
                    var destFilePath = string.Concat(useConverted ? fileInfo.FileName : fileInfo.SourceFileName,
                        ".gz");

                    compressResult.LastFile = Path.GetFileName(fileInfo.FileName);
                    if (fileInfo.FileName != null)
                    {
                        var sourceFile = new FileStream(fileInfo.FileName, FileMode.Open, FileAccess.Read,
                            FileShare.Read,
                            262144);
                        var destFile = new FileStream(destFilePath, FileMode.Create, FileAccess.ReadWrite,
                            FileShare.None,
                            262144);
                        var compressStream = new GZipStream(destFile, CompressionLevel.Optimal);
                        sourceFile.CopyTo(compressStream);
                        compressStream.Close();
                        compressStream.Dispose();
                        destFile.Close();
                        destFile.Dispose();
                        sourceFile.Close();
                        sourceFile.Dispose();
                    }

                    compressResult.DestFiles.Add(GetFileInfo(destFilePath));
                }
                catch (Exception ex)
                {
                    compressResult.Status = CompressStatus.Error;
                    compressResult.ErrorDetails = ex;
                    break;
                }

            return compressResult;
        }

        /// <summary>
        ///     Compress files to a ZIP archive
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <param name="useConverted"></param>
        /// <param name="sourcePath"></param>
        /// <param name="zipPrefix"></param>
        /// <returns></returns>
        private static CompressResult CompressZip(IEnumerable<SftpInfo> sourceFiles, bool useConverted,
            string sourcePath,
            string zipPrefix = null)
        {
            var compressResult = new CompressResult(CompressType.Zip) {Status = CompressStatus.Success};

            foreach (var sourceFile in sourceFiles)
                if (!Path.GetExtension(sourceFile.FileName)
                    .Equals(".zip", StringComparison.CurrentCultureIgnoreCase))
                    compressResult.SourceFiles.Add(sourceFile);

            var timeNow = DateTime.Now.ToString("yyyyMMddHHmmss");

            var zipName = $"{(!string.IsNullOrEmpty(zipPrefix) ? zipPrefix : "SFTP_")}{timeNow}.zip";

            try
            {
                var destFilePath = Path.Combine(sourcePath, zipName);

                if (File.Exists(destFilePath)) File.Delete(destFilePath);

                var zipStream = new FileStream(destFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite,
                    262144);
                var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create);

                //Compress each file for remote transfer
                foreach (var fileInfo in compressResult.SourceFiles)
                    try
                    {
                        compressResult.LastFile = Path.GetFileName(fileInfo.FileName);
                        var entryName = Path.GetFileName(useConverted ? fileInfo.FileName : fileInfo.SourceFileName);

                        zipFile.CreateEntryFromFile(fileInfo.FileName, entryName, CompressionLevel.Optimal);
                    }
                    catch (Exception ex)
                    {
                        compressResult.Status = CompressStatus.Error;
                        compressResult.ErrorDetails = ex;
                        break;
                    }

                zipFile.Dispose();
                zipStream.Close();
                zipStream.Dispose();
                compressResult.DestFiles.Add(GetFileInfo(destFilePath));
            }
            catch (Exception ex)
            {
                compressResult.Status = CompressStatus.Error;
                compressResult.ErrorDetails = ex;
            }

            return compressResult;
        }

        /// <summary>
        ///     Compress files to individual ZIP files
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <param name="useConverted"></param>
        /// <returns></returns>
        private static CompressResult CompressZipMulti(IEnumerable<SftpInfo> sourceFiles, bool useConverted)
        {
            var compressResult = new CompressResult(CompressType.Zip) {Status = CompressStatus.Success};

            foreach (var sourceFile in sourceFiles)
                if (!Path.GetExtension(sourceFile.FileName)
                    .Equals(".zip", StringComparison.CurrentCultureIgnoreCase))
                    compressResult.SourceFiles.Add(sourceFile);

            //Compress each file for remote transfer
            foreach (var fileInfo in compressResult.SourceFiles)
                try
                {
                    var destFilePath = string.Concat(useConverted ? fileInfo.FileName : fileInfo.SourceFileName,
                        ".zip");

                    compressResult.LastFile = Path.GetFileName(fileInfo.FileName);
                    var zipStream = new FileStream(destFilePath, FileMode.Create, FileAccess.ReadWrite,
                        FileShare.ReadWrite, 262144);
                    var zipFile = new ZipArchive(zipStream, ZipArchiveMode.Create);

                    var entryName = Path.GetFileName(useConverted ? fileInfo.FileName : fileInfo.SourceFileName);

                    zipFile.CreateEntryFromFile(fileInfo.FileName, entryName, CompressionLevel.Optimal);
                    zipFile.Dispose();
                    zipStream.Close();
                    zipStream.Dispose();
                    compressResult.DestFiles.Add(GetFileInfo(destFilePath));
                }
                catch (Exception ex)
                {
                    compressResult.Status = CompressStatus.Error;
                    compressResult.ErrorDetails = ex;
                    break;
                }

            return compressResult;
        }

        /// <summary>
        ///     Move files to an archive folder
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <returns></returns>
        public static CompressResult ArchiveFiles(string sourcePath, string destPath)
        {
            var archiveResult = new CompressResult(CompressType.Uncompressed) {Status = CompressStatus.Success};

            if (Directory.Exists(sourcePath))
                foreach (var filePath in Directory.GetFiles(sourcePath))
                    try
                    {
                        archiveResult.SourceFiles.Add(GetFileInfo(filePath));
                        archiveResult.LastFile = Path.GetFileName(filePath);
                        var destFilePath = Path.Combine(destPath, archiveResult.LastFile);

                        if (File.Exists(destFilePath)) File.Delete(destFilePath);

                        File.Move(filePath, destFilePath);
                        archiveResult.DestFiles.Add(GetFileInfo(destFilePath));
                    }
                    catch (Exception ex)
                    {
                        archiveResult.Status = CompressStatus.Error;
                        archiveResult.ErrorDetails = ex;
                        break;
                    }
            else
                archiveResult.Status = CompressStatus.PathNotFound;

            return archiveResult;
        }

        /// <summary>
        ///     Remove archived files after a given number of days
        /// </summary>
        /// <param name="archivePath"></param>
        /// <param name="archiveDays"></param>
        /// <returns></returns>
        public static CompressResult DoCleanup(string archivePath, int archiveDays)
        {
            var archiveResult = new CompressResult(CompressType.Uncompressed) {Status = CompressStatus.Success};

            if (Directory.Exists(archivePath))
                foreach (var filePath in Directory.GetFiles(archivePath).Where(fileName =>
                    File.GetCreationTime(fileName) < DateTime.Now.AddDays(-archiveDays)).ToList())
                    try
                    {
                        archiveResult.LastFile = Path.GetFileName(filePath);
                        var deleteFile = GetFileInfo(filePath);
                        File.Delete(filePath);

                        archiveResult.DestFiles.Add(deleteFile);
                    }
                    catch (Exception ex)
                    {
                        archiveResult.Status = CompressStatus.Error;
                        archiveResult.ErrorDetails = ex;
                        break;
                    }
            else
                archiveResult.Status = CompressStatus.PathNotFound;

            return archiveResult;
        }
    }
}