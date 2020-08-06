// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Luna.Clients.Azure.Storage
{
    /// <summary>
    /// Interface that outlines functionality used to manage an Azure storage instance and 
    /// files within the storage instance.
    /// </summary>
    public interface IStorageUtility
    {
        /// <summary>
        /// Creates a storage container.
        /// </summary>
        /// <param name="containerName">The name of the storage container to create.</param>
        /// <returns></returns>
        Task CreateContainerAsync(string containerName);

        /// <summary>
        /// Deletes a storage container.
        /// </summary>
        /// <param name="containerName">The name of the storage container to delete.</param>
        /// <returns></returns>
        Task DeleteContainerAsync(string containerName);

        /// <summary>
        /// Checks if a storage container exists.
        /// </summary>
        /// <param name="containerName">The name of the container to check exists.</param>
        /// <returns>True if exists, false otherwise.</returns>
        Task<bool> ContainerExistsAsync(string containerName);

        /// <summary>
        /// Uploads a binary file to a storage container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="content">The content of the file to upload.</param>
        /// <param name="overwrite">True if this file should be overwritten if it already exists, false otherwise.</param>
        /// <returns>The URI to the created resource.</returns>
        Task<string> UploadBinaryFileAsync(string containerName, string fileName, byte[] content, bool overwrite);

        /// <summary>
        /// Uploads a text file to a storage container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="content">The content of the file to upload.</param>
        /// <param name="overwrite">True if this file should be overwritten if it already exists, false otherwise.</param>
        /// <returns>The URI to the created resource.</returns>
        Task<string> UploadTextFileAsync(string containerName, string fileName, string content, bool overwrite);

        /// <summary>
        /// Removes a file from a storage container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="fileName">The name of the file to delete.</param>
        /// <returns></returns>
        Task DeleteFileAsync(string containerName, string fileName);

        /// <summary>
        /// Gets a reference to the file in storage with the SaS key in the URL.
        /// </summary>
        /// <param name="url">The URL of the file.</param>
        /// <param name="readOnly">True if the SaS key is read only, false otherwise.</param>
        /// <returns>The file URL with the SaS key.</returns>
        Task<string> GetFileReferenceWithSasKeyAsync(string url, bool readOnly = true);

        /// <summary>
        /// Download the contents of the blob at the url to text.
        /// </summary>
        /// <param name="url">The URL of the file</param>
        /// <returns></returns>
        Task<string> DownloadToTextAsync(string url);

        /// <summary>
        /// Delete the file from storage container if exists
        /// </summary>
        /// <param name="uri">The uri</param>
        /// <returns></returns>
        Task DeleteFileAsync(Uri uri);

        /// <summary>
        /// Insert a table entity, if the table doesn't exist, will create a new table
        /// </summary>
        /// <param name="tableName">The table name</param>
        /// <param name="entity">The entity</param>
        /// <returns></returns>
        Task InsertTableEntity(string tableName, TableEntity entity);
    }
}
