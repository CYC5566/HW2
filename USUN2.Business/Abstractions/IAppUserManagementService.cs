using USUN2.Common.Models;

namespace USUN2.Business.Abstractions;

public interface IAppUserManagementService
{
    Task<AppUserDetailDto?> GetDetailAsync(string userId, CancellationToken cancellationToken = default);
    Task CreateAsync(UserCreateInput input, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserEditInput input, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userId, CancellationToken cancellationToken = default);
}
