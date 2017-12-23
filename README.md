# AzureTSBlobImporter
Azure Table Storage Blob Import application that works with streams instead of local files. 

## Introduction
This small Azure Webjobs proyect is aimed to check a periodically ZIP archive posted on a website and process the file contents directly into an Azure Table Storage Blob Section. This is a work in progress meant to be reused accross any other system or application.

It's approach resides on a Table Storage Collection that logs transit activity from a specified link. Then it proceeeds to compare an APIs HEAD Response field of Last-Modified-Date to the current UTC time.

> This repo is a continuing work in progress with periodical updates.

### This release utilizes code from the following links :
- Uploading Block Blobs larger than 256 MB in Azure - [Link](https://inside.covve.com/uploading-block-blobs-larger-than-256-mb-in-azure/)
