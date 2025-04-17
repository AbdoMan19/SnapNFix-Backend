using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

public class OptionalEmailValidator<TUser> : IUserValidator<TUser> where TUser : IdentityUser<Guid>
{
    public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
    {
        var errors = new List<IdentityError>();

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            var emailValidator = new EmailAddressAttribute();
            if (!emailValidator.IsValid(user.Email))
            {
                errors.Add(new IdentityError
                {
                    Code = "InvalidEmail",
                    Description = "The email address is not valid."
                });
            }
        }

        return Task.FromResult(errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
    }
}
