using SnapNFix.Domain.Entities;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Common.Interfaces;

public interface IPhotoValidationService : IScoped
{
    Task<string> SendImageForValidationAsync(string imagePath, CancellationToken cancellationToken);
    Task ProcessPhotoValidationInBackgroundAsync(SnapReport snapReport);
}


