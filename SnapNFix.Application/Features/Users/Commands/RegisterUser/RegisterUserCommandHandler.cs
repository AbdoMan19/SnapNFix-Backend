using MediatR;
using Microsoft.AspNetCore.Identity;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser
{
  public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, GenericResponseModel<RegisterUserCommandResponse>>
  {
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterUserCommandHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
      _userManager = userManager;
      _roleManager = roleManager;
    }

    public async Task<GenericResponseModel<RegisterUserCommandResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
      try
      {
        var user = new User
        {
          Id = Guid.NewGuid(),
          FirstName = request.FirstName,
          LastName = request.LastName,
          PhoneNumber = request.PhoneNumber,
          UserName = Guid.NewGuid().ToString(),
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
          var errors = string.Join(", ", result.Errors.Select(e => e.Description));
          return GenericResponseModel<RegisterUserCommandResponse>.Failure($"Registration failed: {errors}");
        }
        //TODO:: make enum
        await _userManager.AddToRoleAsync(user, "Citizen");
        var response = new RegisterUserCommandResponse
        {
          Id = user.Id
        };

        return GenericResponseModel<RegisterUserCommandResponse>.Success(response);
      }
      catch (Exception ex)
      {
        return GenericResponseModel<RegisterUserCommandResponse>.Failure($"Registration failed: {ex.Message}");
      }
    }
  }
}