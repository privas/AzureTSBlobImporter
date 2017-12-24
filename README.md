# AzureTSBlobImporter
Azure Table Storage Blob Import application that works with network downloaded streams instead of local files. 

## Introduction
This small Azure Webjobs project is aimed to check a ZIP archive posted perodically on an API endpoint and process the file contents directly into an Azure Table Storage Blob Section. 

It's approach points to a Table Storage Table that logs transit activity from a specified link. THe process then compares an APIs HEAD Response field labelled as 'Last-Modified-Date' to the current UTC time to determine if a download is in order. 

> This repo is a continuing work in progress with periodical updates.

### This release utilizes code from the following links :
- Uploading Block Blobs larger than 256 MB in Azure - [Link](https://inside.covve.com/uploading-block-blobs-larger-than-256-mb-in-azure/)
