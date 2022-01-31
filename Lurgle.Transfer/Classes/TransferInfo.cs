using System;
using Lurgle.Transfer.Enums;

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Lurgle.Transfer.Classes
{
    /// <summary>
    ///     File attributes
    /// </summary>
    public class TransferInfo
    {
        /// <summary>
        ///     File attributes
        /// </summary>
        /// <param name="file"></param>
        /// <param name="accessDate"></param>
        /// <param name="modifyDate"></param>
        /// <param name="fileSize"></param>
        /// <param name="fileType"></param>
        public TransferInfo(string file, DateTime accessDate, DateTime modifyDate, long fileSize,
            InfoType? fileType = null)
        {
            FileName = file;
            SourceFileName = file;
            AccessedDate = accessDate.ToLocalTime();
            ModifiedDate = modifyDate.ToLocalTime();
            Size = fileSize;
            Type = fileType ?? InfoType.File;
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

        /// <summary>
        ///     File type (file, directory, link, other)
        /// </summary>
        public InfoType Type { get; }
    }
}