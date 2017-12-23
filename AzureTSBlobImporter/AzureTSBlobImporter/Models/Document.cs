using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureTSBlobImporter.Models
{
    public class Document : TableEntity
    {
        public Document() { }

        public Document(string regid)
        {
            PartitionKey = Guid.NewGuid().ToString();
            RowKey = regid;
            Timestamp = DateTime.UtcNow;
        }

        public DateTimeOffset? lastChecked { get; set; }

        public string blobURL { get; set; }
        public string downloadURL { get; set; }
    }
}