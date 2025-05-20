using Google.Apis.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Infrastructure.Options;
using Azure.Storage.Blobs;

namespace SnapNFix.Infrastructure.Services.ImageProcessingService;

public class ImageProcessingService : IImageProcessingService
{
    private readonly ImageProcessingSettings _settings;
    private readonly ILogger<ImageProcessingService> _logger;

    private readonly BlobContainerClient _containerClient;

    public ImageProcessingService(
        IOptions<ImageProcessingSettings> settings,
        ILogger<ImageProcessingService> logger,
        IConfiguration configuration) 
    {
        _settings = settings.Value;
        _logger = logger;

        var connectionString = configuration["AzureBlob:ConnectionString"];
        var containerName = configuration["AzureBlob:ContainerName"];
        _containerClient = new BlobContainerClient(connectionString, containerName);
        _containerClient.CreateIfNotExists();
    }

    public async Task<(bool isValid, string errorMessage)> ValidateImageAsync(IFormFile image)
    {
        // Check file size
        if (image.Length > _settings.MaxFileSizeInBytes)
            return (false, $"Image size should not exceed {_settings.MaxFileSizeInMb}MB.");

        // Check file extension
        var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
        if (!_settings.AllowedExtensions.Contains(extension))
            return (false, $"Invalid image format. Only {string.Join(", ", _settings.AllowedExtensions)} are allowed.");

        // Verify file is actually an image
        try
        {
            await using var imageStream = image.OpenReadStream();
            await Image.IdentifyAsync(imageStream);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating image {FileName}", image.FileName);
            return (false, "The file is not a valid image.");
        }
    }

    public async Task<string> SaveImageAsync(IFormFile image, string subfolder)
    {
        var uniqueFileName = $"{subfolder}/{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        try
        {
            var blobClient = _containerClient.GetBlobClient(uniqueFileName);
            using var stream = image.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving image {FileName} to Azure Blob Storage", image.FileName);
            throw;
        }
    }
}