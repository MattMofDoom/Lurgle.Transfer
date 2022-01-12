using System;
using System.Collections.Generic;
using Lurgle.Transfer.Enums;

// ReSharper disable MemberCanBePrivate.Global

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
        /// <param name="destination"></param>
        /// <param name="isCert"></param>
        public TransferResult(string destination, bool isCert)
        {
            Destination = destination;
            Status = TransferStatus.Unknown;
            FileSize = 0;
            UseCert = isCert;
            Destination = destination;
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
        public bool UseCert { get; }

        /// <summary>
        ///     Destination
        /// </summary>
        public string Destination { get; }

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