using System.Threading.Tasks;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Interfaces;
public interface IDataSeedingService : IScoped
{
  Task SeedLargeDatasetAsync(int numberOfUsers, int numberOfReports);
  Task ClearAllDataAsync();
}



