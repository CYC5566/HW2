using USUN2.Common.Models;

namespace USUN2.Data.Abstractions;

public interface IUserRepository
{
    Task<AppUserRow?> GetByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task InsertAsync(AppUserRow user, CancellationToken cancellationToken = default);
    Task UpdateAsync(AppUserRow user, CancellationToken cancellationToken = default);
    Task DeleteAsync(string userId, CancellationToken cancellationToken = default);
}
