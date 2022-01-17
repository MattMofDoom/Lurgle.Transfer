using System;
using System.Collections.Generic;
using System.Text;

namespace Lurgle.Transfer.Enums
{
    /// <summary>
    /// Select which filename to use from TransferInfo
    /// </summary>
    public enum FileSelection
    {
        /// <summary>
        /// Use TransferInfo.FileName
        /// </summary>
        UseFileName,
        /// <summary>
        /// Use TransferInfo.SourceFileName
        /// </summary>
        UseSourceFileName
    }
}
