using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

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
                    Code = "InvalidEmail",
                    Description = "The email address is not valid."
                });
            } else if (await manager.FindByEmailAsync(user.Email) != null)
            {
                errors.Add(new IdentityError
                {
                    Code = "DuplicateEmail",
                    Description = "The email address is already exist."
                });
            }
        }

        return await Task.FromResult(errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
    }
}
