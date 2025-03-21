using MediatR;
using Microsoft.AspNetCore.Identity;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser;

  public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, GenericResponseModel<Guid>>
  {
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public RegisterUserCommandHandler(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
      _userManager = userManager;
      _roleManager = roleManager;
    }

    public async Task<GenericResponseModel<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        { 
          FirstName = request.FirstName,
          LastName = request.LastName,
          PhoneNumber = request.PhoneNumber,
          PhoneNumberConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
          var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code , e.Description)).ToList();
          return GenericResponseModel<Guid>.Failure("Registration failed",errors);
        }
        //TODO:: make enum
        await _userManager.AddToRoleAsync(user, "Citizen");
        return GenericResponseModel<Guid>.Success(user.Id);
    }
  }