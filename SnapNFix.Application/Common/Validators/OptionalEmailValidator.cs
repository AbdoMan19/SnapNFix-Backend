using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using SnapNFix.Application.Resources;

public class OptionalEmailValidator<TUser> : IUserValidator<TUser> where TUser : IdentityUser<Guid>
{
    public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
        var errors = new List<IdentityError>();

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(user.Email) )
            {
                errors.Add(new IdentityError
                {
                    Code = Shared.InvalidEmail,
                    Description = Shared.InvalidEmailDescription
                });
            } else if (await manager.FindByEmailAsync(user.Email) != null)
            {
                errors.Add(new IdentityError
                {
                    Code = Shared.DuplicateEmail,
                    Description = Shared.DuplicateEmailDescription
                });
            }
        }

        return await Task.FromResult(errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
    }
}
