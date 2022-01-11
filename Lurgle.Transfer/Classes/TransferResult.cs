using System;
using System.Collections.Generic;
using Lurgle.Transfer.Enums;

// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Lurgle.Transfer.Classes
{
    /// <summary>
    ///     SFTP Result
    /// </summary>
    public class TransferResult
    {
        /// <summary>
        ///     SFTP Result
        /// </summary>
        /// <param name="ftpDestination"></param>
        /// <param name="isCert"></param>
        public TransferResult(Destination ftpDestination, bool isCert)
        {
            Destination = Destination.Unknown;
            Status = TransferStatus.Unknown;
            FileSize = 0;
            UseCert = isCert;
            Destination = ftpDestination;
            FileList = new List<TransferInfo>();
            ErrorDetails = null;
        }

        /// <summary>
        ///     SFTP Status
        /// </summary>
        public TransferStatus Status { get; set; }

        /// <summary>
        ///     Remote File Size
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        ///     Use certificate authentication
        /// </summary>
        private bool UseCert { get; }

        /// <summary>
        ///     Destination
        /// </summary>
        private Destination Destination { get; }

        /// <summary>
        ///     File list
        /// </summary>
        public List<TransferInfo> FileList { get; set; }

        /// <summary>
        ///     Exception details
        /// </summary>
        public Exception ErrorDetails { get; set; }
    }
}