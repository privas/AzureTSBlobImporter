using AzureTSBlobImporter.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureTSBlobImporter.BL
{
    internal class TableStorageBL
    {
        private CloudStorageAccount storageAccount;
        private CloudTableClient tableClient;
        private CloudTable table;
        private TableBatchOperation batchOperation;

        public TableStorageBL(string collectionName)
        {
            storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.AzureConnString);
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference(collectionName);

            if (!table.Exists())
                table.CreateIfNotExists();
        }
        public List<Document> GetDocumentConfig()
        {
            storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.AzureConnString);
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("");

            TableQuery<Document> query = new TableQuery<Document>();

            List<Document> reponseModelList = new List<Document>();
            foreach (Document entity in table.ExecuteQuery(query))
            {
                reponseModelList.Add(
                    new Document()
                    {
                        PartitionKey = entity.PartitionKey,
                        RowKey = entity.RowKey,
                        downloadURL = entity.downloadURL,
                        blobURL = entity.blobURL,
                        lastChecked = entity.lastChecked
                    }
                );
            }

            return reponseModelList;
        }

        #region Document Config
        public Document GetGTFSItem(string id)
        {
            storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.AzureConnString);
            tableClient = storageAccount.CreateCloudTableClient();
            table = tableClient.GetTableReference("");

            TableQuery<Document> query = new TableQuery<Document>()
                                        .Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, id));

            return table.ExecuteQuery(query).FirstOrDefault();
        }
        public static void InsertGTFSItem(Document entry)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Properties.Settings.Default.AzureConnString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("");
            TableBatchOperation batchOperation = new TableBatchOperation();

            entry.ETag = "*";
            entry.lastChecked = DateTimeOffset.UtcNow;

            batchOperation.InsertOrMerge(entry);
            table.ExecuteBatch(batchOperation);
            batchOperation.Clear();
        }
        #endregion
    }
}
