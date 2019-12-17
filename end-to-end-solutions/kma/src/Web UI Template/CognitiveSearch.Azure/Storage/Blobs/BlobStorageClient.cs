using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Auth;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveSearch.Azure.Storage.Blobs
{
    public static class BlobStorageClient
    {
        public static async Task<byte[]> DownloadBlobAsync(BlobStorageConfig storageConfig, string blobName)
        {
            var container = await GetContainerAsync(storageConfig);
            var blob = container.GetBlockBlobReference(blobName);
            await blob.FetchAttributesAsync();
            var target = new byte[blob.Properties.Length];
            await blob.DownloadToByteArrayAsync(target, 0);
            return target;
        }

        public static async Task<string> ReadBlobAsync(BlobStorageConfig storageConfig, string blobName)
        {
            var container = await GetContainerAsync(storageConfig);
            var blob = container.GetBlockBlobReference(blobName);
            await blob.FetchAttributesAsync();
            var target = await blob.DownloadTextAsync();
            return target;
        }

        public static async Task WriteBlobAsync(BlobStorageConfig storageConfig, string blobName, string text)
        {
            var container = await GetContainerAsync(storageConfig);
            var blob = container.GetBlockBlobReference(blobName);
            await blob.FetchAttributesAsync();
            await blob.UploadTextAsync(text);
        }

        public static async Task<Uri> GetBlobUriAsync(BlobStorageConfig storageConfig, string blobName)
        {
            try
            {
                var container = await GetContainerAsync(storageConfig);
                var blob = container.GetBlockBlobReference(blobName);
                return await Task.FromResult(blob.Uri);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static async Task<bool> BlobExistsAsync(BlobStorageConfig storageConfig, string blobName)
        {
            var container = await GetContainerAsync(storageConfig);
            var blob = container.GetBlockBlobReference(blobName);
            return await blob.ExistsAsync();
        }

        /// <summary>
        /// Retrieves a list of the names of blobs in a container.
        /// </summary>
        /// <param name="storageConfig"></param>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<string>> GetBlobListAsync(BlobStorageConfig storageConfig, string directoryName = "")
        {
            try
            {
                var container = await GetContainerAsync(storageConfig);
                var list = new List<IListBlobItem>();

                BlobContinuationToken token = null;
                BlobResultSegment resultSegment = null;

                if (directoryName == null) directoryName = "";

                var dir = container.GetDirectoryReference(directoryName);

                do
                {
                    resultSegment = await dir.ListBlobsSegmentedAsync(token);
                    list.AddRange(resultSegment.Results);
                    token = resultSegment.ContinuationToken;
                }
                while (token != null);

                var blobs = list.Select(b => b as CloudBlockBlob).Where(b => b is CloudBlockBlob);
                return blobs.Select(b => b.Name);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// Retrieves the folder structure contained within the Blob Storage container specified in storageConfig.
        /// </summary>
        /// <param name="storageConfig"></param>
        /// <param name="directoryName"></param>
        /// <returns></returns>
        public static async Task<BlobStorageDirectory> GetDirectoryStructureAsync(BlobStorageConfig storageConfig, string directoryName = "")
        {
            try
            {
                var container = await GetContainerAsync(storageConfig);
                var list = new List<IListBlobItem>();

                BlobContinuationToken token = null;
                BlobResultSegment resultSegment = null;

                var dir = container.GetDirectoryReference(directoryName);

                do
                {
                    resultSegment = await dir.ListBlobsSegmentedAsync(token);
                    list.AddRange(resultSegment.Results);
                    token = resultSegment.ContinuationToken;
                }
                while (token != null);

                var structure = await ParseBlobDirectoryStructure(storageConfig, list, directoryName);
                return structure;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private static async Task<BlobStorageDirectory> ParseBlobDirectoryStructure(BlobStorageConfig storageConfig, IEnumerable<IListBlobItem> blobList, string directoryName = "")
        {
            var blobs = blobList.Select(b => b as CloudBlockBlob).Where(b => b is CloudBlockBlob);
            var subDirectories = blobList.Select(b => b as CloudBlobDirectory).Where(b => b is CloudBlobDirectory);

            var structure = new BlobStorageDirectory
            {
                Name = directoryName,
                BlobNames = blobs.Select(i => i.Name).ToList(),
            };

            foreach (var directory in subDirectories)
            {
                var subDirectory = await GetDirectoryStructureAsync(storageConfig, directory.Prefix);
                structure.SubDirectories.Add(subDirectory);
            }

            return structure;
        }

        public static async Task<bool> UploadBlobAsync(BlobStorageConfig storageConfig, string blobName, string blobContent)
        {
            try
            {
                var container = await GetContainerAsync(storageConfig);
                var blob = container.GetBlockBlobReference(blobName);
                await blob.UploadTextAsync(blobContent);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public static async Task<bool> UploadBlobAsync(BlobStorageConfig storageConfig, string blobName, Stream stream)
        {
            try
            {
                var container = await GetContainerAsync(storageConfig);
                var blob = container.GetBlockBlobReference(blobName);
                await blob.UploadFromStreamAsync(stream);
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="storageConfig"><see cref="Cocorahs.Library.BlobStorage.BlobStorageConfig"/></param>
        /// <param name="blobName">Name of the target blob for the append operation.</param>
        /// <param name="blobContent">Content to append to the blob.</param>
        /// <returns></returns>
        public static async Task AppendToBlobAsync(BlobStorageConfig storageConfig, string blobName, string blobContent)
        {
            var container = await GetContainerAsync(storageConfig);
            var blob = container.GetAppendBlobReference(blobName);

            // Create the blob, if it does not already exist
            if (!await blob.ExistsAsync())
            {
                await blob.CreateOrReplaceAsync();
            }

            // Add the entry to the log
            await blob.AppendTextAsync(blobContent);
        }

        public static async Task<string> GetContainerSasUriAsync(BlobStorageConfig config)
        {
            var container = await GetContainerAsync(config);
            var policy = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(1),
                Permissions = SharedAccessBlobPermissions.Read
            };

            return await Task.FromResult(container.GetSharedAccessSignature(policy));
        }

        private static async Task<CloudBlobContainer> GetContainerAsync(BlobStorageConfig config)
        {
            var client = GetStorageAccount(config.AccountName, config.Key).CreateCloudBlobClient();

            var container = client.GetContainerReference(config.ContainerName);
            await container.CreateIfNotExistsAsync();

            return container;
        }

        private static CloudStorageAccount GetStorageAccount(string accountName, string key) => new CloudStorageAccount(new StorageCredentials(accountName, key), true);
    }
}