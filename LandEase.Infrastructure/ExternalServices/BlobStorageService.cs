using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using LandEase.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LandEase.Infrastructure.ExternalServices;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration config)
    {
        var connectionString = config["AzureStorage:ConnectionString"];
        var containerName = config["AzureStorage:ContainerName"];
        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream, string fileName, string contentType)
    {
        await _containerClient.CreateIfNotExistsAsync();

        var blobClient = _containerClient.GetBlobClient(fileName);

        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders
        {
            ContentType = contentType
        });

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        var uri = new Uri(fileUrl);
        var blobName = string.Join("/", uri.Segments.Skip(2));
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }
}