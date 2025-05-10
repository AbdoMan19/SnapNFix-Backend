using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface IPhotoValidationService : IScoped
{
    Task<string> SendImageForValidationAsync(string imagePath, CancellationToken cancellationToken);
}