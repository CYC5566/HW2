using USUN2.Business.Abstractions;
using USUN2.Common.Models;
using USUN2.Common.Validation;
using USUN2.Data.Abstractions;

namespace USUN2.Business.Services;

public sealed class AppUserManagementService(IUserRepository userRepository) : IAppUserManagementService
{
    public async Task<AppUserDetailDto?> GetDetailAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return null;
        return new AppUserDetailDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            Account = user.Account
        };
    }

    public async Task CreateAsync(UserCreateInput input, CancellationToken cancellationToken = default)
    {
        var id = TaiwanNationalIdValidator.Normalize(input.UserId);
        var row = new AppUserRow
        {
            UserId = id,
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim(),
            Account = input.Account.Trim()
        };
        await userRepository.InsertAsync(row, cancellationToken);
    }

    public async Task UpdateAsync(UserEditInput input, CancellationToken cancellationToken = default)
    {
        var row = new AppUserRow
        {
            UserId = TaiwanNationalIdValidator.Normalize(input.UserId),
            UserName = input.UserName.Trim(),
            Email = input.Email.Trim(),
            Account = input.Account.Trim()
        };
        await userRepository.UpdateAsync(row, cancellationToken);
    }

    public Task DeleteAsync(string userId, CancellationToken cancellationToken = default) =>
        userRepository.DeleteAsync(TaiwanNationalIdValidator.Normalize(userId), cancellationToken);
}
