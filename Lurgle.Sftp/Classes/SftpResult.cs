using System;
using System.Collections.Generic;
using Lurgle.Sftp.Enums;
// ReSharper disable UnusedAutoPropertyAccessor.Global

// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace Lurgle.Sftp.Classes
{
    /// <summary>
    ///     SFTP Result
    /// </summary>
    public class SftpResult
    {
        /// <summary>
        ///     SFTP Result
        /// </summary>
        /// <param name="ftpDestination"></param>
        /// <param name="isCert"></param>
        public SftpResult(Destination ftpDestination, bool isCert)
        {
            Destination = Destination.Unknown;
            Status = SftpStatus.Unknown;
            FileSize = 0;
            UseCert = isCert;
            Destination = ftpDestination;
            FileList = new List<SftpInfo>();
            ErrorDetails = null;
        }

        /// <summary>
        ///     SFTP Status
        /// </summary>
        public SftpStatus Status { get; set; }

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
        public List<SftpInfo> FileList { get; set; }

        /// <summary>
        ///     Exception details
        /// </summary>
        public Exception ErrorDetails { get; set; }
    }
}