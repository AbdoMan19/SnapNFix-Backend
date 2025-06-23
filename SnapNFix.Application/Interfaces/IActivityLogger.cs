using Application.DTOs;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Interfaces;

    public interface IActivityLogger : ISingleton
    {
        public Task LogActivityAsync(ActivityLogDto activityLog);
    }



