using Lurgle.Transfer.Classes;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Lurgle.Transfer
{
    /// <summary>
    ///     Common functions
    /// </summary>
    public static class Transfers
    {
        /// <summary>
        ///     Current Lurgle.Transfer configuration
        /// </summary>
        public static TransferConfig Config { get; private set; }

        /// <summary>
        ///     Set the logging config. This will only set/update the config if there is no LogWriter currently set.
        /// </summary>
        public static void SetConfig(TransferConfig config = null)
        {
            Config = TransferConfig.GetConfig(config);
        }

        /// <summary>
        ///     Initialisation transfer config
        /// </summary>
        public static void Init()
        {
            if (Config == null) SetConfig();
        }
    }
}