using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

public class SetTargetCommandHandler : IRequestHandler<SetTargetCommand, GenericResponseModel<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ILogger<SetTargetCommandHandler> _logger;

        public SetTargetCommandHandler(
            IUnitOfWork unitOfWork,
            IUserService userService,
            ILogger<SetTargetCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _logger = logger;
        }

        public async Task<GenericResponseModel<bool>> Handle(SetTargetCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var currentUserId = await _userService.GetCurrentUserIdAsync();

                // Deactivate all previous targets
                var existingTargets = await _unitOfWork.Repository<AdminTarget>()
                    .FindBy(t => t.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var target in existingTargets)
                {
                    target.IsActive = false;
                    await _unitOfWork.Repository<AdminTarget>().Update(target);
                }

                // Create new target
                var newTarget = new AdminTarget
                {
                    TargetResolutionRate = request.TargetResolutionRate,
                    CreatedBy = currentUserId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<AdminTarget>().Add(newTarget);
                await _unitOfWork.SaveChanges();

                _logger.LogInformation("Target updated to {Target}% by SuperAdmin {UserId}", 
                    request.TargetResolutionRate, currentUserId);

                return GenericResponseModel<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting target");
                return GenericResponseModel<bool>.Failure("Failed to set target",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.TargetResolutionRate), "Failed to set target")
                    });
            }
        }
    }