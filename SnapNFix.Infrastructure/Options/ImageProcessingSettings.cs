namespace SnapNFix.Infrastructure.Options;

public class ImageProcessingSettings
{
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
    public int MaxFileSizeInMb { get; set; }
    public string BasePath { get; set; }
    public string[] AllowedContentTypes { get; set; } = Array.Empty<string>();
    
    public long MaxFileSizeInBytes => MaxFileSizeInMb * 1024 * 1024;
}