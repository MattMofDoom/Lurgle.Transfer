using System;
using System.Collections.Generic;
using System.Configuration;
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
        private const int ArchiveDaysDefault = 30;
        private const int ArchiveDaysMin = 0;
        private const int ArchiveDaysMax = 365;
        private const int DefaultSftpPort = 22;
        private const int DefaultProxyPort = 80;
        private const int DefaultFtpBufferSize = 262144;

        /// <summary>
        ///     Configuration path
        /// </summary>
        public string ConfigPath { get; private set; }

        /// <summary>
        ///     App name
        /// </summary>
        public string AppName { get; private set; }

        /// <summary>
        ///     Source path
        /// </summary>
        public string SourcePath { get; set; }

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
        ///     List of SFTP destinations
        /// </summary>
        public List<Destination> SftpDestinations { get; private set; }

        /// <summary>
        ///     Load the currently configured config file
        /// </summary>
        public static TransferConfig GetConfig(TransferConfig config = null)
        {
            TransferConfig transferConfig;
            if (config == null)
                transferConfig = new TransferConfig
                {
                    ConfigPath = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString(),
                    AppName = ConfigurationManager.AppSettings["AppName"],
                    SourcePath = ConfigurationManager.AppSettings["SourcePath"],
                    DestPath = ConfigurationManager.AppSettings["DestPath"],
                    //Default doArchive to true for legacy configs
                    DoArchive = string.IsNullOrEmpty(ConfigurationManager.AppSettings["DoArchive"]) ||
                                Transfers.GetBool(ConfigurationManager.AppSettings["DoArchive"]),
                    ArchivePath = ConfigurationManager.AppSettings["ArchivePath"],
                    ArchiveDays = Transfers.GetInt(ConfigurationManager.AppSettings["ArchiveDays"]),
                    SftpDestinations = GetDestinations(ConfigurationManager.AppSettings["SftpDestinations"])
                };
            else
                transferConfig = config;

            if (transferConfig.ArchiveDays.Equals(-1) || transferConfig.ArchiveDays < ArchiveDaysMin ||
                transferConfig.ArchiveDays > ArchiveDaysMax)
                transferConfig.ArchiveDays = ArchiveDaysDefault;
            //If ArchiveDays is 0, then we disable archiving
            if (transferConfig.ArchiveDays.Equals(0)) transferConfig.DoArchive = false;

            return transferConfig;
        }

        /// <summary>
        ///     Parse a comma-delimited ftpDestinations <see cref="string" /> into a list of <see cref="Destination" />
        /// </summary>
        /// <param name="configValue">Setting string (comma-delimited")</param>
        /// <returns></returns>
        private static List<Destination> GetDestinations(string configValue)
        {
            var destinations = new List<Destination>();

            foreach (var destination in configValue.Split(','))
                if (Enum.TryParse(destination, true, out Destination destinationValue))
                    destinations.Add(destinationValue);

            return destinations;
        }

        /// <summary>
        ///     Return an <see cref="TransferAuth" /> value for the specified <see cref="Destination" />
        /// </summary>
        /// <param name="ftpDestination">Retrieve the config for this destination</param>
        /// <returns></returns>
        private TransferAuth GetFtpAuth(Destination ftpDestination)
        {
            return Enum.TryParse(
                ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpAuthMode"], true,
                out TransferAuth ftpAuth)
                ? ftpAuth
                : TransferAuth.Password;
        }

        /// <summary>
        ///     Parse the configured compresstype <see cref="string" /> into a <see cref="CompressType" /> value
        /// </summary>
        /// <param name="configValue">Setting string</param>
        /// <returns></returns>
        private CompressType GetCompressType(string configValue)
        {
            return Enum.TryParse(configValue, true, out CompressType compressType)
                ? compressType
                : CompressType.Uncompressed;
        }

        /// <summary>
        ///     Parse the configured transfertype <see cref="string" /> into a <see cref="TransferType" /> value
        /// </summary>
        /// <param name="ftpDestination">Setting string</param>
        /// <returns></returns>
        private TransferType GetTransferType(Destination ftpDestination)
        {
            //Allow configs using the old incorrect config line (xTransferType instead of xSftpTransferType") to still work 
            var configLine =
                ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpTransferType"];
            if (string.IsNullOrEmpty(configLine))
                configLine =
                    ConfigurationManager.AppSettings[
                        $"{ftpDestination}TransferType"];

            return Enum.TryParse(configLine, true, out TransferType transferType) ? transferType : TransferType.Upload;
        }

        /// <summary>
        ///     Parse the configured TransferMode <see cref="string" /> into a <see cref="TransferMode" /> value
        /// </summary>
        /// <param name="ftpDestination">Setting string</param>
        /// <returns></returns>
        private TransferMode GetTransferMode(Destination ftpDestination)
        {
            //Allow configs using the old incorrect config line (xTransferMode instead of xSftpTransferMode") to still work 
            var configLine =
                ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpTransferMode"];
            if (string.IsNullOrEmpty(configLine))
                configLine =
                    ConfigurationManager.AppSettings[
                        $"{ftpDestination}TransferMode"];

            return Enum.TryParse(configLine, true, out TransferMode transferMode) ? transferMode : TransferMode.Sftp;
        }

        /// <summary>
        ///     Retrieves mailIfError or mailIfSuccess .. if not present in config, returns true
        /// </summary>
        /// <param name="mailConfig"></param>
        /// <returns></returns>
        private bool GetMailBool(string mailConfig)
        {
            return string.IsNullOrEmpty(mailConfig) || Transfers.GetBool(mailConfig);
        }

        private PdfTarget GetPdfTarget(string pdfTarget)
        {
            return Enum.TryParse(pdfTarget, true, out PdfTarget pdfVersion) ? pdfVersion : PdfTarget.Pdf1_3;
        }

        private bool GetPdfBool(object sourceObject)
        {
            var sourceString = string.Empty;

            if (!Convert.IsDBNull(sourceObject)) sourceString = (string) sourceObject;

            return !bool.TryParse(sourceString, out var destBool) || destBool;
        }

        /// <summary>
        ///     Return an <see cref="TransferDestination" /> value for the specified <see cref="Destination" />
        /// </summary>
        /// <param name="ftpDestination">Retrieve the config for this destination</param>
        /// <returns></returns>
        public TransferDestination GetSftpDestination(Destination ftpDestination)
        {
            var config = new TransferDestination
            {
                TransferType = GetTransferType(ftpDestination),
                TransferMode = GetTransferMode(ftpDestination),
                Destination = ftpDestination,
                AuthMode = GetFtpAuth(ftpDestination),
                Name = ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpName"],
                BufferSize = Transfers.GetInt(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpBufferSize"]),
                Server = ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpServer"],
                Port = Transfers.GetInt(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpPort"]),
                UsePassive = Transfers.GetBool(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpUsePassive"]),
                Path = ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpPath"],
                UserName = ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpUserName"],
                Password = ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpPassword"],
                CertPath = ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpCertPath"],
                RetryCount = Transfers.GetInt(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpRetryCount"]),
                RetryDelay = Transfers.GetInt(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpRetryDelay"]),
                RetryTest = Transfers.GetBool(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpRetryTest"]),
                RetryFailAll = Transfers.GetBool(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpRetryFailAll"]),
                RetryFailConnect = Transfers.GetBool(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpRetryFailConnect"]),
                UseProxy = Transfers.GetBool(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpUseProxy"]),
                ProxyServer =
                    ConfigurationManager.AppSettings[
                        $"{ftpDestination}SftpProxyServer"],
                ProxyType = ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpProxyType"],
                ProxyPort = Transfers.GetInt(ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpProxyPort"]),
                ProxyUser = ConfigurationManager.AppSettings[
                    $"{ftpDestination}SftpProxyUser"],
                ProxyPassword =
                    ConfigurationManager.AppSettings[
                        $"{ftpDestination}SftpProxyPassword"],
                CompressType = GetCompressType(ConfigurationManager.AppSettings[
                    $"{ftpDestination}CompressType"]),
                ZipPrefix = ConfigurationManager.AppSettings[
                    $"{ftpDestination}ZipPrefix"],
                MailTo = ConfigurationManager.AppSettings[
                    $"{ftpDestination}MailTo"],
                MailToError =
                    ConfigurationManager.AppSettings[
                        $"{ftpDestination}MailToError"],
                MailIfError =
                    GetMailBool(ConfigurationManager.AppSettings[
                        $"{ftpDestination}MailIfError"]),
                MailIfSuccess = GetMailBool(ConfigurationManager.AppSettings[
                    $"{ftpDestination}MailIfSuccess"]),
                DownloadDays =
                    Transfers.GetInt(ConfigurationManager.AppSettings[
                        $"{ftpDestination}DownloadDays"]),
                ConvertPdf =
                    Transfers.GetBool(ConfigurationManager.AppSettings[
                        $"{ftpDestination}ConvertPdf"]),
                PdfTarget = GetPdfTarget(ConfigurationManager.AppSettings[
                    $"{ftpDestination}PdfVersion"]),
                PdfKeepOriginal = GetPdfBool(ConfigurationManager.AppSettings[
                    $"{ftpDestination}PdfKeepOriginal"])
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

            return config;
        }
    }
}