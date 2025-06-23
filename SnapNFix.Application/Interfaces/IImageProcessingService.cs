using SnapNFix.Application.Common.Interfaces.ServiceLifetime;
using Microsoft.AspNetCore.Http;
namespace SnapNFix.Application.Interfaces;

public interface IImageProcessingService : IScoped
{
    Task<(bool isValid, string errorMessage)> ValidateImageAsync(IFormFile image);
    Task<string> SaveImageAsync(IFormFile image, string subfolder);
}


