using System;
using System.Threading.Tasks;
using Luna.Clients.Azure.Auth;
using Luna.Clients.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Luna.Clients.Azure.Storage
{
    /// <summary>
    /// Utility class that provides functionality to manage files on Azure storage.
    /// </summary>
    public class StorageUtility : IStorageUtility
    {
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly ILogger<StorageUtility> _logger;

        private enum NameType { 
            Blob, 
            Container,
            Directory,
            File,
            Queue,
            Share,
            Table
         }


        /// <summary>
        /// Constructor that initializes a storage client with the provided account credentials.
        /// </summary>
        /// <param name="storageAccountName">The name of the storage account.</param>
        /// <param name="storageAccountKey">The key for the storage account.</param>
        /// <param name="logger"></param>
        [ActivatorUtilitiesConstructor]
        public StorageUtility(IOptionsMonitor<StorageAccountConfigurationOption>  options, ILogger<StorageUtility> logger,
            IKeyVaultHelper keyVaultHelper)
        {
            string key = keyVaultHelper.GetSecretAsync(options.CurrentValue.Config.VaultName, options.CurrentValue.Config.AccountKey).Result;
            StorageCredentials storageCredentials = new StorageCredentials(options.CurrentValue.Config.AccountName, key);
            CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
            _cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates a storage container.
        /// </summary>
        /// <param name="containerName">The name of the container to create.</param>
        /// <returns></returns>
        public async Task CreateContainerAsync(string containerName)
        {
            validateName(containerName, NameType.Container);

            CloudBlobContainer cloudBlobContainer = _cloudBlobClient.GetContainerReference(containerName);
            
            // Cannot create the container because it already exists
            if (await cloudBlobContainer.ExistsAsync())
            {
                // TODO
                throw new LunaServerException($"Cannot create container. Container {containerName} already exists", false);
            }
            else
            {
                _logger.LogInformation($"Creating container {containerName}");
                await cloudBlobContainer.CreateAsync();
            }
        }

        /// <summary>
        /// Deletes a storage container if it exists.
        /// </summary>
        /// <param name="containerName">The name of the container to delete.</param>
        /// <returns></returns>
        public async Task DeleteContainerAsync(string containerName)
        {
            validateName(containerName, NameType.Container);
            _logger.LogInformation($"DeleteContainerAsync: Container name {containerName}");
            CloudBlobContainer cloudBlobContainer = _cloudBlobClient.GetContainerReference(containerName);
            await cloudBlobContainer.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Checks if a storage container exists.
        /// </summary>
        /// <param name="containerName">The name of the container to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        public async Task<bool> ContainerExistsAsync(string containerName)
        {
            _logger.LogInformation($"Check if container {containerName} exists");
            validateName(containerName, NameType.Container);

            NameValidator.ValidateContainerName(containerName);
            CloudBlobContainer cloudBlobContainer = _cloudBlobClient.GetContainerReference(containerName);
            return await cloudBlobContainer.ExistsAsync();
        }

        /// <summary>
        /// Uploads a binary file through a byte array to a blob in a container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="fileName">The name of the binary file to upload.</param>
        /// <param name="content">The content of the binary file.</param>
        /// <param name="overwrite">True if this file should be overwritten if it already exists, false otherwise.</param>
        /// <returns>The URI to the created resource.</returns>
        public async Task<string> UploadBinaryFileAsync(string containerName, string fileName, byte[] content, bool overwrite)
        {
            _logger.LogInformation($"Upload Binary File : {fileName} to blob in {containerName} with overwrite mode set to {overwrite}");
            validateName(containerName, NameType.Container);
            validateName(fileName, NameType.Blob);

            CloudBlobContainer cloudBlobContainer = _cloudBlobClient.GetContainerReference(containerName);
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            if (overwrite)
            {
                _logger.LogInformation("Overriding existing file.");
                await cloudBlockBlob.DeleteIfExistsAsync();
            }
            else if (await cloudBlockBlob.ExistsAsync())
            {
                // The file already exists but overwrite is false
                // TODO
                throw new LunaServerException("Overriding existing file not allowed in this mode.", false);
            }

            await cloudBlockBlob.UploadFromByteArrayAsync(content, 0, content.Length);
            
            return cloudBlockBlob.Uri.ToString(); 
        }

        /// <summary>
        /// Uploads a text file through a string to a blob in a container. 
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="fileName">The name of the text file to upload.</param>
        /// <param name="content">The content of the text file to upload.</param>
        /// <param name="overwrite">True if this file should be overwritten if it already exists, false otherwise.</param>
        /// <returns>The URI to the created resource.</returns>
        public async Task<string> UploadTextFileAsync(string containerName, string fileName, string content, bool overwrite)
        {
            _logger.LogInformation($"Upload Text File : {fileName} to blob in {containerName} with overwrite mode set to {overwrite}");
            validateName(containerName, NameType.Container);
            validateName(fileName, NameType.Blob);

            CloudBlobContainer cloudBlobContainer = _cloudBlobClient.GetContainerReference(containerName);
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            if (overwrite)
            {
                _logger.LogInformation("Overriding existing file.");
                await cloudBlockBlob.DeleteIfExistsAsync();
            }
            else if (await cloudBlockBlob.ExistsAsync())
            {
                // The file already exists but overwrite is false
                // TODO
                throw new LunaServerException("Overriding existing file not allowed in this mode.", false);
            }

            await cloudBlockBlob.UploadTextAsync(content);

            return cloudBlockBlob.Uri.ToString();
        }

        /// <summary>
        /// Deletes the file from the storage container if it exists.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="fileName">The name of the file to be deleted.</param>
        /// <returns></returns>
        public async Task DeleteFileAsync(string containerName, string fileName)
        {
            _logger.LogInformation($"Delete File : {fileName} in {containerName}");

            validateName(containerName, NameType.Container);
            validateName(fileName, NameType.Blob);

            CloudBlobContainer cloudBlobContainer = _cloudBlobClient.GetContainerReference(containerName);
            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(fileName);

            await cloudBlockBlob.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Delete the file from storage container if exists
        /// </summary>
        /// <param name="uri">The uri</param>
        /// <returns></returns>
        public async Task DeleteFileAsync(Uri uri)
        {
            _logger.LogInformation($"Delete File at {uri}");

            var blob = await _cloudBlobClient.GetBlobReferenceFromServerAsync(uri);
            await blob.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Gets a reference to the file in blob storage with the SaS key in the URL.
        /// </summary>
        /// <param name="url">The URL of the blob.</param>
        /// <param name="readOnly">True if the SaS key is read only, false otherwise.</param>
        /// <returns>The blob URL with the SaS key.</returns>
        public async Task<string> GetFileReferenceWithSasKeyAsync(string url, bool readOnly = true)
        {
            _logger.LogInformation($"GetFileReferenceWithSasKeyAsync with url {url} and readOnly set as {readOnly}");
            //Remove the existing SaS key
            Uri uri = new Uri(url);
            url = uri.GetLeftPart(UriPartial.Path);
            ICloudBlob cloudBlob = await _cloudBlobClient.GetBlobReferenceFromServerAsync(new Uri(url));

            if (readOnly)
            {
                SharedAccessBlobPolicy _sharedAccessBlobReadPolicy = new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read,
                    SharedAccessStartTime = DateTime.Now.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.Now.AddHours(1)
                };
                return url + cloudBlob.GetSharedAccessSignature(_sharedAccessBlobReadPolicy);
            }
            else
            {
                SharedAccessBlobPolicy _sharedAccessBlobReadWritePolicy = new SharedAccessBlobPolicy
                {
                    Permissions = SharedAccessBlobPermissions.Read
                | SharedAccessBlobPermissions.Write
                | SharedAccessBlobPermissions.List
                | SharedAccessBlobPermissions.Delete
                | SharedAccessBlobPermissions.Create
                | SharedAccessBlobPermissions.Add,
                    SharedAccessStartTime = DateTime.Now.AddMinutes(-5),
                    SharedAccessExpiryTime = DateTime.Now.AddHours(1)
                };
                return url + cloudBlob.GetSharedAccessSignature(_sharedAccessBlobReadWritePolicy);
            }
        }

        /// <summary>
        /// Download the contents of the blob at the url to text.
        /// </summary>
        /// <param name="url">The URL of the file.</param>
        /// <returns></returns>
        public async Task<string> DownloadToTextAsync(string url)
        {
            _logger.LogInformation($"Download content to text from {url}");
            var cloudBlob = new CloudBlockBlob(new Uri(url));
            return await cloudBlob.DownloadTextAsync();
        }

        private void validateName(string name, NameType nameType)
        {
            try
            {
                _logger.LogInformation($"Validate name: {name} for given storage utility NameType: {nameType}");
                switch (nameType)
                {
                    case NameType.Blob:
                        NameValidator.ValidateBlobName(name);
                        break;
                    case NameType.Container:
                        NameValidator.ValidateContainerName(name);
                        break;
                    case NameType.Directory:
                        NameValidator.ValidateDirectoryName(name);
                        break;
                    case NameType.File:
                        NameValidator.ValidateFileName(name);
                        break;
                    case NameType.Queue:
                        NameValidator.ValidateQueueName(name);
                        break;
                    case NameType.Share:
                        NameValidator.ValidateShareName(name);
                        break;
                    case NameType.Table:
                        NameValidator.ValidateTableName(name);
                        break;
                }
            }
            catch (ArgumentException ex)
            {
                // TODO
                throw new LunaServerException($"The provided NameType {nameType} is not supported", false);
            }
        }
    }
}
