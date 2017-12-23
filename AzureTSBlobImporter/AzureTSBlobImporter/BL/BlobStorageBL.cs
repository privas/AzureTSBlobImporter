using AzureTSBlobImporter.Models;
using AzureTSBlobImporter.Properties;
using AzureTSBlobImporter.Services;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AzureTSBlobImporter.BL
{
    public class BlobStorageBLL
    {
        public static async Task processZIPfile(Document _doc)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Settings.Default.AzureConnString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("");

            WebClient wc = new WebClient();
            using (var zipBlobFileStream = new MemoryStream(wc.DownloadData(_doc.downloadURL)))
            {
                byte[] bytes;
                await zipBlobFileStream.FlushAsync();
                zipBlobFileStream.Position = 0;

                using (var zip = new ZipArchive(zipBlobFileStream))
                {
                    foreach (var entry in zip.Entries)
                    {
                        CloudBlockBlob blob = container.GetBlockBlobReference(_doc.RowKey + "/" + entry.FullName);
                        using (var entryStream = entry.Open())
                        {
                            if (entry.Length > 0)
                            {
                                using (var md5 = MD5.Create())
                                {
                                    using (var stream = entry.Open())
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            stream.CopyTo(ms);
                                            bytes = ms.ToArray();
                                        }
                                    }
                                }

                                AzureBlobUploader _abu = new AzureBlobUploader();
                                await _abu.UploadAsync(_doc, bytes, entry.FullName);
                            }
                        }
                    }
                }
            }
        }
    }
}
