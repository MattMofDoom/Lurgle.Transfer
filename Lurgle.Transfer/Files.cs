using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Lurgle.Transfer.Classes;
using Lurgle.Transfer.Enums;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Local

// ReSharper disable UnusedMember.Global

namespace Lurgle.Transfer
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
        public static TransferInfo GetFileInfo(string sourceFile)
        {
            var fileAccess = File.GetLastAccessTimeUtc(sourceFile);
            var fileModify = File.GetLastWriteTimeUtc(sourceFile);
            var fileSize = new FileInfo(sourceFile).Length;
            return new TransferInfo(sourceFile, fileAccess, fileModify, fileSize);
        }

        /// <summary>
        ///     Retrieve file attributes
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <returns></returns>
        public static List<TransferInfo> GetFileInfo(IEnumerable<string> sourceFiles)
        {
            return (from file in sourceFiles
                let fileAccess = File.GetLastAccessTimeUtc(file)
                let fileModify = File.GetLastWriteTimeUtc(file)
                let fileSize = new FileInfo(file).Length
                select new TransferInfo(file, fileAccess, fileModify, fileSize)).ToList();
        }

        /// <summary>
        ///     Retrieve a list of files and attributes from a given path
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <returns></returns>
        public static List<TransferInfo> GetFiles(string sourcePath)
        {
            return GetFileInfo(Directory.GetFiles(sourcePath).ToList());
        }

        /// <summary>
        ///     Retrieve a list of files from a given path
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        public static List<string> GetFileList(IEnumerable<TransferInfo> fileInfo)
        {
            return fileInfo.Select(file => Path.GetFileName(file.FileName)).ToList();
        }

        /// <summary>
        ///     Add a random number to the beginning or end of a file name
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="appendType"></param>
        /// <returns></returns>
        public static string GetNewName(string filePath, AppendType appendType = AppendType.Prefix)
        {
            var fileName = Path.GetFileName(filePath);
            var destPath = Path.GetDirectoryName(filePath);

            if (appendType.Equals(AppendType.Prefix))
                fileName = $"{Transfers.GetRandomNumber()}-{fileName}";
            else
                fileName = Path.ChangeExtension(
                    $"{Path.GetFileNameWithoutExtension(fileName)}-{Transfers.GetRandomNumber()}",
                    Path.GetExtension(fileName));

            destPath = destPath != null ? Path.Combine(destPath, fileName) : fileName;

            return destPath;
        }

        /// <summary>
        ///     Delete compressed files that match the compression type
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="compressType"></param>
        /// <param name="compressedFiles"></param>
        /// <returns></returns>
        public static CompressResult DeleteCompressedFiles(TransferDestination destination,
            CompressType? compressType = null,
            IEnumerable<TransferInfo> compressedFiles = null)
        {
            var compression = compressType ?? destination.CompressType;

            var compressResult = new CompressResult(compression);
            List<TransferInfo> files;
            if (compressedFiles != null)
                files = compressedFiles.ToList();
            else
                files = string.IsNullOrEmpty(destination.SourcePath)
                    ? GetFiles(Transfers.Config.SourcePath)
                    : GetFiles(destination.SourcePath);

            foreach (var file in files)
                switch (Path.GetExtension(file.FileName))
                {
                    case ".gz" when compression == CompressType.Gzip:
                        compressResult.SourceFiles.Add(file);
                        File.Delete(file.FileName);
                        break;
                    case ".zip" when compression == CompressType.Zip || compression == CompressType.ZipPerFile:
                        compressResult.SourceFiles.Add(file);
                        File.Delete(file.FileName);
                        break;
                }

            return compressResult;
        }

        /// <summary>
        ///     Compress files given a configured compression type
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="compressType"></param>
        /// <param name="sourceFiles"></param>
        /// <param name="useConverted"></param>
        /// <param name="zipFilePath"></param>
        /// <param name="zipPrefix"></param>
        /// <returns></returns>
        public static CompressResult CompressFiles(TransferDestination destination, CompressType? compressType = null,
            IEnumerable<TransferInfo> sourceFiles = null,
            bool useConverted = false, string zipFilePath = null, string zipPrefix = null)
        {
            var compressResult = new CompressResult(compressType ?? destination.CompressType);
            List<TransferInfo> files;
            if (sourceFiles != null)
                files = sourceFiles.ToList();
            else
                files = string.IsNullOrEmpty(destination.SourcePath)
                    ? GetFiles(Transfers.Config.SourcePath)
                    : GetFiles(destination.SourcePath);

            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (compressType)
            {
                case CompressType.Gzip:
                    compressResult = CompressGzip(files, useConverted);
                    break;
                case CompressType.Zip:
                    var sourcePath = zipFilePath;
                    if (string.IsNullOrEmpty(zipFilePath))
                        sourcePath = string.IsNullOrEmpty(destination.SourcePath)
                            ? Transfers.Config.SourcePath
                            : destination.SourcePath;
                    compressResult = CompressZip(files, useConverted, sourcePath, zipPrefix);
                    break;
                case CompressType.ZipPerFile:
                    compressResult = CompressZipMulti(files, useConverted);
                    break;
                default:
                    compressResult.Status = CompressStatus.NotCompressed;
                    compressResult.SourceFiles = files;
                    compressResult.DestFiles = files;
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
        private static CompressResult CompressGzip(IEnumerable<TransferInfo> sourceFiles, bool useConverted)
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
        private static CompressResult CompressZip(IEnumerable<TransferInfo> sourceFiles, bool useConverted,
            string sourcePath,
            string zipPrefix = null)
        {
            var compressResult = new CompressResult(CompressType.Zip) {Status = CompressStatus.Success};

            foreach (var sourceFile in sourceFiles)
                if (!Path.GetExtension(sourceFile.FileName)
                        .Equals(".zip", StringComparison.CurrentCultureIgnoreCase))
                    compressResult.SourceFiles.Add(sourceFile);

            var timeNow = DateTime.Now.ToString("yyyyMMddHHmmss");

            var zipName = $"{(!string.IsNullOrEmpty(zipPrefix) ? zipPrefix : "Transfer_")}{timeNow}.zip";

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
        private static CompressResult CompressZipMulti(IEnumerable<TransferInfo> sourceFiles, bool useConverted)
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
        /// <param name="destination"></param>
        /// <param name="path"></param>
        /// <param name="archivePath"></param>
        /// <returns></returns>
        public static CompressResult ArchiveFiles(TransferDestination destination, string path = null,
            string archivePath = null)
        {
            var archiveResult = new CompressResult(CompressType.Uncompressed) {Status = CompressStatus.Success};
            var sourcePath = path;
            var destPath = archivePath;

            if (string.IsNullOrEmpty(sourcePath))
                sourcePath = string.IsNullOrEmpty(destination.SourcePath)
                    ? Transfers.Config.SourcePath
                    : destination.SourcePath;

            if (string.IsNullOrEmpty(destPath))
                destPath = string.IsNullOrEmpty(destination.ArchivePath)
                    ? Transfers.Config.ArchivePath
                    : destination.ArchivePath;

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
        /// <param name="destination"></param>
        /// <param name="archivePath"></param>
        /// <param name="archiveDays"></param>
        /// <returns></returns>
        public static CompressResult CleanArchivedFiles(TransferDestination destination, string archivePath = null,
            int? archiveDays = null)
        {
            var archiveResult = new CompressResult(CompressType.Uncompressed) {Status = CompressStatus.Success};
            var destPath = archivePath;
            if (string.IsNullOrEmpty(destPath))
                destPath = string.IsNullOrEmpty(destination.ArchivePath)
                    ? Transfers.Config.ArchivePath
                    : destination.ArchivePath;

            int days;
            if (archiveDays != null)
                days = (int) archiveDays;
            else
                days = destination.ArchiveDays == 0 && Transfers.Config.ArchiveDays > 0
                    ? Transfers.Config.ArchiveDays
                    : destination.ArchiveDays;

            if (days > 0 && Directory.Exists(destPath))
                foreach (var filePath in Directory.GetFiles(destPath).Where(fileName =>
                             File.GetCreationTime(fileName) < DateTime.Now.AddDays(-days)).ToList())
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
            else if (!Directory.Exists(destPath)) archiveResult.Status = CompressStatus.PathNotFound;

            return archiveResult;
        }
    }
}