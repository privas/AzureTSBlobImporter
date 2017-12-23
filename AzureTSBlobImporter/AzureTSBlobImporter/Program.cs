using System;
using System.Collections.Generic;
using System.Net.Http;
using AzureTSBlobImporter.BL;
using AzureTSBlobImporter.Models;

namespace AzureTSBlobImporter
{
    class Program
    {
        static void Main()
        {
            try
            {
                List<Document> _docs = new List<Document>();
                TableStorageBL _bl = new TableStorageBL("DOCS");
                _docs = _bl.GetDocumentConfig();

                if (_docs.Count > 0)
                {
                    foreach (Document _doc in _docs)
                    {
                        bool isNewDownload = checkDocumentFeed(_doc.downloadURL, _doc.lastChecked);
                        if (isNewDownload)
                        {
                            BlobStorageBLL.processZIPfile(_doc).Wait();
                            TableStorageBL.InsertGTFSItem(_doc);
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message + "-" + ex.InnerException); }
        }

        protected static bool checkDocumentFeed(string downloadURL, DateTimeOffset? lastChecked)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Head, downloadURL);
                HttpResponseMessage response = client.SendAsync(requestMessage).Result;
                DateTimeOffset? _lastModified = response.Content.Headers.LastModified;

                if (!lastChecked.HasValue)
                    lastChecked = DateTimeOffset.MinValue;

                if (_lastModified >= lastChecked)
                    return true;
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
