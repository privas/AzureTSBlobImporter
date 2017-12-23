using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using AzureTSBlobImporter.Models;

namespace AzureTSBlobImporter.Services
{
    public class AzureBlobUploader
    {
        public async Task UploadAsync(Document document, byte[] file, string filename)
        {
            const int storageSharedAccessWriteExpiryInMinutes = 5;
            string storageAccountConnectionString = Properties.Settings.Default.AzureConnString;
            const string storageAccountContainer = "";
            string blobFilename = filename;
            var utcNow = DateTime.UtcNow;

            var sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessStartTime = utcNow.AddMinutes(-10),//you may not have exactly the same time as the azure server that receives the request 
                SharedAccessExpiryTime = utcNow.AddMinutes(storageSharedAccessWriteExpiryInMinutes),
                Permissions = SharedAccessBlobPermissions.Write
            };
            var storageAccount = CloudStorageAccount.Parse(storageAccountConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var blobContainer = blobClient.GetContainerReference(storageAccountContainer);
            blobContainer.CreateIfNotExists();

            var pathInsideContainer = $"{document.RowKey}/{blobFilename}";
            var blob = blobContainer.GetBlockBlobReference(pathInsideContainer);

            var sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);
            var sasUri = blob.Uri + sasBlobToken;

            const int pageSizeInBytes = 104857600; //100MB for requests using REST versions after 2016-05-31
            //const int pageSizeInBytes = 4096; //4MB for requests using REST versions before 2016-05-31

            var prevLastByte = 0;
            var bytesRemain = file.Length;
            var blockIds = new List<string>();
            do
            {
                var bytesToCopy = Math.Min(bytesRemain, pageSizeInBytes);
                var bytesToSend = new byte[bytesToCopy];
                Array.Copy(file, prevLastByte, bytesToSend, 0, bytesToCopy);
                prevLastByte += bytesToCopy;
                bytesRemain -= bytesToCopy;

                //create blockId
                var blockId = Guid.NewGuid().ToString();
                var base64BlockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(blockId));
                blockIds.Add(base64BlockId);

                //final uri
                var uri = $"{sasUri}&comp=block&blockid={base64BlockId}";

                //post block
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Put, uri);
                    request.Headers.Add("x-ms-version", "2017-04-17"); //for requests using REST versions after 2016-05-31
                                                                       //request.Headers.Add("x-ms-version", "2015-04-05"); //for requests using REST versions before 2016-05-31
                    request.Content = new ByteArrayContent(bytesToSend);

                    await client.SendAsync(request);
                }
            } while (bytesRemain > 0);

            //post blocklist
            var blocklistUri = $"{sasUri}&comp=blocklist";
            var xmlBlockIds = new XElement("BlockList", blockIds.Select(id => new XElement("Latest", id)));
            using (var client = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Put, blocklistUri);
                request.Headers.Add("x-ms-version", "2017-04-17"); //for requests using REST versions after 2016-05-31
                                                                   //request.Headers.Add("x-ms-version", "2015-04-05"); //for requests using REST versions before 2016-05-31
                request.Content = new StringContent(xmlBlockIds.ToString(), Encoding.UTF8, "application/xml");

                await client.SendAsync(request);

            }
        }
    }
}
