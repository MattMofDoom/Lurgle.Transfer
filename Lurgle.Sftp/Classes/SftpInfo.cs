using System;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Lurgle.Sftp.Classes
{
    /// <summary>
    ///     File attributes
    /// </summary>
    public class SftpInfo
    {
        /// <summary>
        ///     File attributes
        /// </summary>
        /// <param name="file"></param>
        /// <param name="accessDate"></param>
        /// <param name="modifyDate"></param>
        /// <param name="fileSize"></param>
        public SftpInfo(string file, DateTime accessDate, DateTime modifyDate, long fileSize)
        {
            FileName = file;
            SourceFileName = file;
            AccessedDate = accessDate.ToLocalTime();
            ModifiedDate = modifyDate.ToLocalTime();
            Size = fileSize;
        }

        /// <summary>
        ///     File name
        /// </summary>
        public string FileName { get; }

        /// <summary>
        ///     Source file name
        /// </summary>
        public string SourceFileName { get; set; }

        /// <summary>
        ///     Last access date
        /// </summary>
        public DateTime AccessedDate { get; }

        /// <summary>
        ///     Last modified date
        /// </summary>
        public DateTime ModifiedDate { get; }

        /// <summary>
        ///     File size
        /// </summary>
        public long Size { get; }
    }
}