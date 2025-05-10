using SnapNFix.Domain.Interfaces.ServiceLifetime;
using Microsoft.AspNetCore.Http;
namespace SnapNFix.Domain.Interfaces;

public interface IImageProcessingService : IScoped
{
    Task<(bool isValid, string errorMessage)> ValidateImageAsync(IFormFile image);
    Task<string> SaveImageAsync(IFormFile image, string subfolder);
}