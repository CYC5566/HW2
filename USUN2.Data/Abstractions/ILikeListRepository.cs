using USUN2.Common.Models;

namespace USUN2.Data.Abstractions;

public interface ILikeListRepository
{
    Task<LikeListListItemDto?> GetBySnAsync(int sn, string userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LikeListListItemDto>> GetByUserAsync(string userId, string? productNameContains = null, CancellationToken cancellationToken = default);
    Task<int> InsertWithProductAsync(LikeListMutationRequest request, CancellationToken cancellationToken = default);
    Task UpdateWithProductAsync(int sn, string userId, LikeListMutationRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int sn, string userId, CancellationToken cancellationToken = default);
}
