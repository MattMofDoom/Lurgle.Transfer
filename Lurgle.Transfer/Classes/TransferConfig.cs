using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Lurgle.Transfer.Enums;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Lurgle.Transfer.Classes
{
    /// <summary>
    ///     Configuration
    /// </summary>
    public class TransferConfig
    {
        /// <summary>
        ///     Default value for ArchiveDays
        /// </summary>
        public const int ArchiveDaysDefault = 30;

        /// <summary>
        ///     Minimum value for ArchiveDays
        /// </summary>
        public const int ArchiveDaysMin = 0;

        /// <summary>
        ///     Maximum value for Archive Days
        /// </summary>
        public const int ArchiveDaysMax = 365;

        /// <summary>
        ///     Default port
        /// </summary>
        public const int DefaultSftpPort = 22;

        /// <summary>
        ///     Default proxy port
        /// </summary>
        public const int DefaultProxyPort = 80;

        /// <summary>
        ///     Default buffer size
        /// </summary>
        public const int DefaultFtpBufferSize = 262144;

        /// <summary>
        ///     Constructor that permits passing a config and optional overrides of any property
        /// </summary>
        /// <param name="config"></param>
        /// <param name="appName"></param>
        /// <param name="appVersion"></param>
        /// <param name="sourcePath"></param>
        /// <param name="destPath"></param>
        /// <param name="doArchive"></param>
        /// <param name="archivePath"></param>
        /// <param name="archiveDays"></param>
        /// <param name="transferDestinations"></param>
        public TransferConfig(TransferConfig config = null, string appName = null, string appVersion = null,
            string sourcePath = null, string destPath = null, bool? doArchive = null, string archivePath = null,
            int? archiveDays = null, Dictionary<string, TransferDestination> transferDestinations = null)
        {
            if (config != null)
            {
                AppName = config.AppName;
                AppVersion = config.AppVersion;
                SourcePath = config.SourcePath;
                DestPath = config.DestPath;
                DoArchive = config.DoArchive;
                ArchivePath = config.ArchivePath;
                ArchiveDays = config.ArchiveDays;
                TransferDestinations = config.TransferDestinations;
            }

            if (!string.IsNullOrEmpty(AppName))
                AppName = appName;
            if (!string.IsNullOrEmpty(AppVersion))
                AppVersion = appVersion;
            if (!string.IsNullOrEmpty(sourcePath))
                SourcePath = sourcePath;
            if (!string.IsNullOrEmpty(destPath))
                DestPath = destPath;
            if (doArchive != null)
                DoArchive = (bool) doArchive;
            if (!string.IsNullOrEmpty(archivePath))
                ArchivePath = archivePath;
            if (archiveDays != null)
                ArchiveDays = (int) archiveDays;
            if (transferDestinations != null)
                TransferDestinations = transferDestinations;

            if (archiveDays.Equals(-1) || archiveDays < ArchiveDaysMin ||
                archiveDays > ArchiveDaysMax)
                archiveDays = ArchiveDaysDefault;
            //If ArchiveDays is 0, then we disable archiving
            if (archiveDays.Equals(0)) DoArchive = false;
        }

        /// <summary>
        ///     App name
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        ///     App version will be determined from the binary version, but can be overriden
        /// </summary>
        public string AppVersion { get; private set; }

        /// <summary>
        ///     Source path
        /// </summary>
        public string SourcePath { get; private set; }

        /// <summary>
        ///     Destination path
        /// </summary>
        public string DestPath { get; private set; }

        /// <summary>
        ///     Do archival
        /// </summary>
        public bool DoArchive { get; private set; }

        /// <summary>
        ///     Archive path
        /// </summary>
        public string ArchivePath { get; private set; }

        /// <summary>
        ///     Days to retain files in archive
        /// </summary>
        public int ArchiveDays { get; private set; }

        /// <summary>
        ///     Transfer destinations
        /// </summary>
        public Dictionary<string, TransferDestination> TransferDestinations { get; private set; }

        /// <summary>
        ///     Load the currently configured config file
        /// </summary>
        public static TransferConfig GetConfig(TransferConfig config = null)
        {
            TransferConfig transferConfig;
            if (config == null)
                transferConfig = new TransferConfig
                {
                    AppName = ConfigurationManager.AppSettings["AppName"],
                    SourcePath = ConfigurationManager.AppSettings["SourcePath"],
                    DestPath = ConfigurationManager.AppSettings["DestPath"],
                    //Default doArchive to true for legacy configs
                    DoArchive = string.IsNullOrEmpty(ConfigurationManager.AppSettings["DoArchive"]) ||
                                GetBool(ConfigurationManager.AppSettings["DoArchive"]),
                    ArchivePath = ConfigurationManager.AppSettings["ArchivePath"],
                    ArchiveDays = GetInt(ConfigurationManager.AppSettings["ArchiveDays"]),
                    TransferDestinations = GetDestinations()
                };
            else
                transferConfig = config;

            var isSuccess = true;

            //If AppName is not specified in config, attempt to populate it. Populate AppVersion while we're at it.
            try
            {
                if (string.IsNullOrEmpty(transferConfig.AppName))
                    transferConfig.AppName = Assembly.GetEntryAssembly()?.GetName().Name;

                transferConfig.AppVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString();
            }
            catch
            {
                isSuccess = false;
            }

            if (!isSuccess)
                try
                {
                    if (string.IsNullOrEmpty(transferConfig.AppName))
                        transferConfig.AppName = Assembly.GetExecutingAssembly().GetName().Name;

                    transferConfig.AppVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                catch
                {
                    //We surrender ...
                    transferConfig.AppVersion = string.Empty;
                }

            if (transferConfig.ArchiveDays.Equals(-1) || transferConfig.ArchiveDays < ArchiveDaysMin ||
                transferConfig.ArchiveDays > ArchiveDaysMax)
                transferConfig.ArchiveDays = ArchiveDaysDefault;
            //If ArchiveDays is 0, then we disable archiving
            if (transferConfig.ArchiveDays.Equals(0)) transferConfig.DoArchive = false;

            return transferConfig;
        }

        /// <summary>
        ///     Convert the supplied <see cref="object" /> to a <see cref="bool" />
        ///     <para />
        ///     This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to a bool</param>
        /// <returns></returns>
        private static bool GetBool(object sourceObject)
        {
            var sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject)) sourceString = (string) sourceObject;

            return bool.TryParse(sourceString, out var destBool) && destBool;
        }

        /// <summary>
        ///     Convert the supplied <see cref="object" /> to an <see cref="int" />
        ///     <para />
        ///     This will filter out nulls that could otherwise cause exceptions
        /// </summary>
        /// <param name="sourceObject">An object that can be converted to an int</param>
        /// <returns></returns>
        private static int GetInt(object sourceObject)
        {
            var sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject)) sourceString = (string) sourceObject;

            if (int.TryParse(sourceString, out var destInt)) return destInt;

            return -1;
        }

        /// <summary>
        ///     Parse a comma-delimited TransferDestinations <see cref="string" /> into a list of destination configs
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, TransferDestination> GetDestinations()
        {
            var result = new Dictionary<string, TransferDestination>();
            var configValue = ConfigurationManager.AppSettings["TransferDestinations"];
            //Backward compatibility
            if (string.IsNullOrEmpty(configValue))
                configValue = ConfigurationManager.AppSettings["SftpDestinations"];

            var destinations = configValue.Split(',').ToList();

            foreach (var destination in destinations)
            {
                var destConfig = GetTransferDestination(destination);
                if (!string.IsNullOrEmpty(destConfig.Name))
                    result.Add(destination, destConfig);
            }

            return result;
        }

        /// <summary>
        ///     Return an <see cref="TransferAuth" /> value for the specified destination
        /// </summary>
        /// <param name="destination">Retrieve the config for this destination</param>
        /// <returns></returns>
        private static TransferAuth GetFtpAuth(string destination)
        {
            return Enum.TryParse(
                ConfigurationManager.AppSettings[
                    $"{destination}SftpAuthMode"], true,
                out TransferAuth ftpAuth)
                ? ftpAuth
                : TransferAuth.Password;
        }

        /// <summary>
        ///     Parse the configured compresstype <see cref="string" /> into a <see cref="CompressType" /> value
        /// </summary>
        /// <param name="configValue">Setting string</param>
        /// <returns></returns>
        private static CompressType GetCompressType(string configValue)
        {
            return Enum.TryParse(configValue, true, out CompressType compressType)
                ? compressType
                : CompressType.Uncompressed;
        }

        /// <summary>
        ///     Parse the configured transfertype <see cref="string" /> into a <see cref="TransferType" /> value
        /// </summary>
        /// <param name="destination">Setting string</param>
        /// <returns></returns>
        private static TransferType GetTransferType(string destination)
        {
            var configLine =
                ConfigurationManager.AppSettings[
                    $"{destination}TransferType"];
            //Backward compatibility
            if (string.IsNullOrEmpty(configLine))
                configLine =
                    ConfigurationManager.AppSettings[
                        $"{destination}SftpTransferType"];

            return Enum.TryParse(configLine, true, out TransferType transferType) ? transferType : TransferType.Upload;
        }

        /// <summary>
        ///     Parse the configured TransferMode <see cref="string" /> into a <see cref="TransferMode" /> value
        /// </summary>
        /// <param name="destination">Setting string</param>
        /// <returns></returns>
        private static TransferMode GetTransferMode(string destination)
        {
            var configLine =
                ConfigurationManager.AppSettings[
                    $"{destination}TransferMode"];

            return Enum.TryParse(configLine, true, out TransferMode transferMode) ? transferMode : TransferMode.Sftp;
        }

        /// <summary>
        ///     Retrieves mailIfError or mailIfSuccess .. if not present in config, returns true
        /// </summary>
        /// <param name="mailConfig"></param>
        /// <returns></returns>
        private static bool GetMailBool(string mailConfig)
        {
            return string.IsNullOrEmpty(mailConfig) || GetBool(mailConfig);
        }

        private static PdfTarget GetPdfTarget(string pdfTarget)
        {
            return Enum.TryParse(pdfTarget, true, out PdfTarget pdfVersion) ? pdfVersion : PdfTarget.Pdf1_3;
        }

        private static bool GetPdfBool(object sourceObject)
        {
            var sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject)) sourceString = (string) sourceObject;

            return !bool.TryParse(sourceString, out var destBool) || destBool;
        }

        /// <summary>
        ///     Return an <see cref="TransferDestination" /> value for the specified destination
        /// </summary>
        /// <param name="destination">Retrieve the config for this destination</param>
        /// <returns></returns>
        private static TransferDestination GetTransferDestination(string destination)
        {
            var config = new TransferDestination
            {
                Destination = destination,
                TransferType = GetTransferType(destination),
                TransferMode = GetTransferMode(destination),
                AuthMode = GetFtpAuth(destination),
                Name = ConfigurationManager.AppSettings[
                    $"{destination}Name"],
                BufferSize = GetInt(ConfigurationManager.AppSettings[
                    $"{destination}BufferSize"]),
                Server = ConfigurationManager.AppSettings[
                    $"{destination}Server"],
                Port = GetInt(ConfigurationManager.AppSettings[
                    $"{destination}Port"]),
                UsePassive = GetBool(ConfigurationManager.AppSettings[
                    $"{destination}UsePassive"]),
                RemotePath = ConfigurationManager.AppSettings[
                    $"{destination}RemotePath"],
                SourcePath = ConfigurationManager.AppSettings[
                    $"{destination}SourcePath"],
                DestPath = ConfigurationManager.AppSettings[
                    $"{destination}DestPath"],
                DoArchive = GetBool(ConfigurationManager.AppSettings[
                    $"{destination}DoArchive"]),
                ArchivePath = ConfigurationManager.AppSettings[
                    $"{destination}ArchivePath"],
                ArchiveDays = GetInt(ConfigurationManager.AppSettings[
                    $"{destination}ArchiveDays"]),
                UserName = ConfigurationManager.AppSettings[
                    $"{destination}UserName"],
                Password = ConfigurationManager.AppSettings[
                    $"{destination}Password"],
                CertPath = ConfigurationManager.AppSettings[
                    $"{destination}CertPath"],
                RetryCount = GetInt(ConfigurationManager.AppSettings[
                    $"{destination}RetryCount"]),
                RetryDelay = GetInt(ConfigurationManager.AppSettings[
                    $"{destination}RetryDelay"]),
                RetryTest = GetBool(ConfigurationManager.AppSettings[
                    $"{destination}RetryTest"]),
                RetryFailAll = GetBool(ConfigurationManager.AppSettings[
                    $"{destination}RetryFailAll"]),
                RetryFailConnect = GetBool(ConfigurationManager.AppSettings[
                    $"{destination}RetryFailConnect"]),
                UseProxy = GetBool(ConfigurationManager.AppSettings[
                    $"{destination}UseProxy"]),
                ProxyServer =
                    ConfigurationManager.AppSettings[
                        $"{destination}ProxyServer"],
                ProxyType = ConfigurationManager.AppSettings[
                    $"{destination}ProxyType"],
                ProxyPort = GetInt(ConfigurationManager.AppSettings[
                    $"{destination}ProxyPort"]),
                ProxyUser = ConfigurationManager.AppSettings[
                    $"{destination}ProxyUser"],
                ProxyPassword =
                    ConfigurationManager.AppSettings[
                        $"{destination}ProxyPassword"],
                CompressType = GetCompressType(ConfigurationManager.AppSettings[
                    $"{destination}CompressType"]),
                ZipPrefix = ConfigurationManager.AppSettings[
                    $"{destination}ZipPrefix"],
                MailTo = ConfigurationManager.AppSettings[
                    $"{destination}MailTo"],
                MailToError =
                    ConfigurationManager.AppSettings[
                        $"{destination}MailToError"],
                MailIfError =
                    GetMailBool(ConfigurationManager.AppSettings[
                        $"{destination}MailIfError"]),
                MailIfSuccess = GetMailBool(ConfigurationManager.AppSettings[
                    $"{destination}MailIfSuccess"]),
                DownloadDays =
                    GetInt(ConfigurationManager.AppSettings[
                        $"{destination}DownloadDays"]),
                ConvertPdf =
                    GetBool(ConfigurationManager.AppSettings[
                        $"{destination}ConvertPdf"]),
                PdfTarget = GetPdfTarget(ConfigurationManager.AppSettings[
                    $"{destination}PdfVersion"]),
                PdfKeepOriginal = GetPdfBool(ConfigurationManager.AppSettings[
                    $"{destination}PdfKeepOriginal"])
            };

            //Backward compatibility
            if (string.IsNullOrEmpty(config.Name))
                config = new TransferDestination
                {
                    Destination = destination,
                    TransferType = GetTransferType(destination),
                    TransferMode = GetTransferMode(destination),
                    AuthMode = GetFtpAuth(destination),
                    Name = ConfigurationManager.AppSettings[
                        $"{destination}SftpName"],
                    BufferSize = GetInt(ConfigurationManager.AppSettings[
                        $"{destination}SftpBufferSize"]),
                    Server = ConfigurationManager.AppSettings[
                        $"{destination}SftpServer"],
                    Port = GetInt(ConfigurationManager.AppSettings[
                        $"{destination}SftpPort"]),
                    UsePassive = GetBool(ConfigurationManager.AppSettings[
                        $"{destination}SftpUsePassive"]),
                    RemotePath = ConfigurationManager.AppSettings[
                        $"{destination}transferPath"],
                    SourcePath = ConfigurationManager.AppSettings[
                        $"{destination}SftpSourcePath"],
                    DestPath = ConfigurationManager.AppSettings[
                        $"{destination}SftpDestPath"],
                    DoArchive = GetBool(ConfigurationManager.AppSettings[
                        $"{destination}SftpDoArchive"]),
                    ArchivePath = ConfigurationManager.AppSettings[
                        $"{destination}SftpArchivePath"],
                    ArchiveDays = GetInt(ConfigurationManager.AppSettings[
                        $"{destination}SftpArchiveDays"]),
                    UserName = ConfigurationManager.AppSettings[
                        $"{destination}SftpUserName"],
                    Password = ConfigurationManager.AppSettings[
                        $"{destination}SftpPassword"],
                    CertPath = ConfigurationManager.AppSettings[
                        $"{destination}SftpCertPath"],
                    RetryCount = GetInt(ConfigurationManager.AppSettings[
                        $"{destination}SftpRetryCount"]),
                    RetryDelay = GetInt(ConfigurationManager.AppSettings[
                        $"{destination}SftpRetryDelay"]),
                    RetryTest = GetBool(ConfigurationManager.AppSettings[
                        $"{destination}SftpRetryTest"]),
                    RetryFailAll = GetBool(ConfigurationManager.AppSettings[
                        $"{destination}SftpRetryFailAll"]),
                    RetryFailConnect = GetBool(ConfigurationManager.AppSettings[
                        $"{destination}SftpRetryFailConnect"]),
                    UseProxy = GetBool(ConfigurationManager.AppSettings[
                        $"{destination}SftpUseProxy"]),
                    ProxyServer =
                        ConfigurationManager.AppSettings[
                            $"{destination}SftpProxyServer"],
                    ProxyType = ConfigurationManager.AppSettings[
                        $"{destination}SftpProxyType"],
                    ProxyPort = GetInt(ConfigurationManager.AppSettings[
                        $"{destination}SftpProxyPort"]),
                    ProxyUser = ConfigurationManager.AppSettings[
                        $"{destination}SftpProxyUser"],
                    ProxyPassword =
                        ConfigurationManager.AppSettings[
                            $"{destination}SftpProxyPassword"],
                    CompressType = GetCompressType(ConfigurationManager.AppSettings[
                        $"{destination}CompressType"]),
                    ZipPrefix = ConfigurationManager.AppSettings[
                        $"{destination}ZipPrefix"],
                    MailTo = ConfigurationManager.AppSettings[
                        $"{destination}MailTo"],
                    MailToError =
                        ConfigurationManager.AppSettings[
                            $"{destination}MailToError"],
                    MailIfError =
                        GetMailBool(ConfigurationManager.AppSettings[
                            $"{destination}MailIfError"]),
                    MailIfSuccess = GetMailBool(ConfigurationManager.AppSettings[
                        $"{destination}MailIfSuccess"]),
                    DownloadDays =
                        GetInt(ConfigurationManager.AppSettings[
                            $"{destination}DownloadDays"]),
                    ConvertPdf =
                        GetBool(ConfigurationManager.AppSettings[
                            $"{destination}ConvertPdf"]),
                    PdfTarget = GetPdfTarget(ConfigurationManager.AppSettings[
                        $"{destination}PdfVersion"]),
                    PdfKeepOriginal = GetPdfBool(ConfigurationManager.AppSettings[
                        $"{destination}PdfKeepOriginal"])
                };

            if (string.IsNullOrEmpty(config.MailToError)) config.MailToError = config.MailTo;

            if (config.Port.Equals(-1)) config.Port = DefaultSftpPort;

            if (config.ProxyPort.Equals(-1)) config.ProxyPort = DefaultProxyPort;

            if (config.BufferSize < 32768) config.BufferSize = DefaultFtpBufferSize;

            if (config.RetryCount < 0)
                config.RetryCount = 0;

            if (config.RetryDelay < 0)
            {
                config.RetryDelay = 0;
            }
            else
            {
                if (config.RetryDelay < 600)
                    config.RetryDelay *= 1000;
                else
                    config.RetryDelay = 60000;
            }

            if (config.ArchiveDays.Equals(-1) || config.ArchiveDays < ArchiveDaysMin ||
                config.ArchiveDays > ArchiveDaysMax)
                config.ArchiveDays = ArchiveDaysDefault;
            //If ArchiveDays is 0, then we disable archiving
            if (config.ArchiveDays.Equals(0)) config.DoArchive = false;

            return config;
        }
    }
}