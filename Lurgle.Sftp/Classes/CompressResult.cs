using System;
using System.Collections.Generic;
using Lurgle.Sftp.Enums;
// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Lurgle.Sftp.Classes
{
    /// <summary>
    ///     Compression result
    /// </summary>
    public class CompressResult
    {
        /// <summary>
        ///     Compression result
        /// </summary>
        /// <param name="compressType"></param>
        public CompressResult(CompressType compressType)
        {
            Status = CompressStatus.Unknown;
            Type = compressType;
            SourceFiles = new List<SftpInfo>();
            DestFiles = new List<SftpInfo>();
            ErrorDetails = null;
        }

        /// <summary>
        ///     Compression status
        /// </summary>
        public CompressStatus Status { get; set; }

        /// <summary>
        ///     Compression type
        /// </summary>
        private CompressType Type { get; }

        /// <summary>
        ///     Source files
        /// </summary>
        public List<SftpInfo> SourceFiles { get; set; }

        /// <summary>
        ///     Destination files
        /// </summary>
        public List<SftpInfo> DestFiles { get; set; }

        /// <summary>
        ///     Last file seen
        /// </summary>
        public string LastFile { get; set; }

        /// <summary>
        ///     Exception details
        /// </summary>
        public Exception ErrorDetails { get; set; }
    }
}