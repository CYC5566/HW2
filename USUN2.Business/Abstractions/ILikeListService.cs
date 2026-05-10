using USUN2.Common.Models;

namespace USUN2.Business.Abstractions;

public interface ILikeListService
{
    Task<IReadOnlyList<LikeListListItemDto>> GetLikeListForUserAsync(
        string userId,
        string? productNameContains = null,
        CancellationToken cancellationToken = default);
    Task<LikeListListItemDto?> GetLikeListItemAsync(int sn, string userId, CancellationToken cancellationToken = default);
    Task<int> AddAsync(LikeListMutationRequest request, CancellationToken cancellationToken = default);
    Task UpdateAsync(LikeListUpdateRequest request, string userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int sn, string userId, CancellationToken cancellationToken = default);
}
