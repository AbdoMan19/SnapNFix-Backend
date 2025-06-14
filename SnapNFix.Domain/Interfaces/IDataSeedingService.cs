using System.Threading.Tasks;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces
{
  public interface IDataSeedingService : IScoped
  {
    Task SeedLargeDatasetAsync(int numberOfUsers, int numberOfReports);
    Task ClearAllDataAsync();
  }
}