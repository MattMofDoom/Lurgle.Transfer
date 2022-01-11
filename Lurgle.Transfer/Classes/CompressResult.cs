using System;
using System.Collections.Generic;
using Lurgle.Transfer.Enums;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Lurgle.Transfer.Classes
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
            SourceFiles = new List<TransferInfo>();
            DestFiles = new List<TransferInfo>();
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
        public List<TransferInfo> SourceFiles { get; set; }

        /// <summary>
        ///     Destination files
        /// </summary>
        public List<TransferInfo> DestFiles { get; set; }

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