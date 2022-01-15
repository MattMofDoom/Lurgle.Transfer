﻿using System;
using System.IO;
using System.Threading;
using Lurgle.Transfer;
using Lurgle.Transfer.Classes;
using Lurgle.Transfer.Enums;

namespace LurgleTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Transfers.Init();

            foreach (var destination in Transfers.Config.TransferDestinations)
            {
                var transfer = new FileTransfer(destination.Value);
                TransferResult transferResult;
                var retries = 0;

                do
                {
                    Console.WriteLine("Attempting connect to {0} ({1})...", transfer.TransferConfig.Name,
                        transfer.Destination);
                    retries++;
                    if (retries > 1)
                    {
                        Console.WriteLine("Await {0} seconds for retry ...", transfer.TransferConfig.RetryDelay / 1000);
                        Thread.Sleep(transfer.TransferConfig.RetryDelay);
                    }

                    transferResult = transfer.Connect();
                    Console.WriteLine("Connect: {0} {1}", transferResult.Status,
                        transferResult.ErrorDetails == null ? string.Empty : transferResult.ErrorDetails.ToString());
                } while (transferResult.Status != TransferStatus.Success &&
                         retries < transfer.TransferConfig.RetryCount);

                if (transferResult.Status != TransferStatus.Success) continue;
                Console.WriteLine("Attempting {0} to {1} ({2})", transfer.TransferConfig.TransferType,
                    transfer.TransferConfig.Name, transfer.Destination);
                switch (transfer.TransferConfig.TransferType)
                {
                    case TransferType.Upload:
                        foreach (var file in Files.GetFiles(destination.Value.SourcePath))
                        {
                            retries = 0;
                            do
                            {
                                retries++;
                                if (retries > 1)
                                {
                                    Console.WriteLine("Await {0} seconds for retry ...",
                                        transfer.TransferConfig.RetryDelay / 1000);
                                    Thread.Sleep(transfer.TransferConfig.RetryDelay);
                                }

                                transferResult = transfer.SendFiles(Path.GetFileName(file.FileName), file.FileName);

                                Console.WriteLine("Send Result: File {0}, Result {1} {2}", file.FileName,
                                    transferResult.Status,
                                    transferResult.ErrorDetails == null
                                        ? string.Empty
                                        : transferResult.ErrorDetails.ToString());
                            } while (transferResult.Status != TransferStatus.Success &&
                                     retries < transfer.TransferConfig.RetryCount);
                        }

                        break;
                    case TransferType.Download:
                        retries = 0;
                        do
                        {
                            retries++;
                            if (retries > 1)
                            {
                                Console.WriteLine("Await {0} seconds for retry ...",
                                    transfer.TransferConfig.RetryDelay / 1000);
                                Thread.Sleep(transfer.TransferConfig.RetryDelay);
                            }

                            transferResult = transfer.ListFiles();
                        } while (transferResult.Status != TransferStatus.Success &&
                                 retries < transfer.TransferConfig.RetryCount);

                        if (transferResult.Status == TransferStatus.Success && transferResult.FileList.Count > 0)
                        {
                            Console.WriteLine("Remote File List:" + Environment.NewLine);
                            var fileList = transferResult.FileList;
                            var fileCount = 0;
                            foreach (var file in fileList)
                            {
                                fileCount++;
                                Console.WriteLine("{0}: {1}", fileCount, file.FileName);
                            }

                            Console.WriteLine();

                            foreach (var file in fileList)
                            {
                                retries = 0;
                                do
                                {
                                    retries++;
                                    if (retries > 1)
                                    {
                                        Console.WriteLine("Await {0} seconds for retry ...",
                                            transfer.TransferConfig.RetryDelay / 1000);
                                        Thread.Sleep(transfer.TransferConfig.RetryDelay);
                                    }

                                    transferResult =
                                        transfer.DownloadFiles(file.FileName, transfer.TransferConfig.RemotePath,
                                            transfer.TransferConfig.DestPath);

                                    Console.WriteLine("Download Result: File {0}, Result {1} {2}", file.FileName,
                                        transferResult.Status,
                                        transferResult.ErrorDetails == null
                                            ? string.Empty
                                            : transferResult.ErrorDetails.ToString());
                                } while (transferResult.Status != TransferStatus.Success &&
                                         retries < transfer.TransferConfig.RetryCount);
                            }
                        }

                        break;
                }

                Console.WriteLine("Disconnecting from {0} ({1})...", transfer.TransferConfig.Name,
                    transfer.Destination);
                transfer.Disconnect();
                Console.WriteLine("Disconnect: {0} {1}", transferResult.Status,
                    transferResult.ErrorDetails == null ? string.Empty : transferResult.ErrorDetails.ToString());
            }
        }
    }
}