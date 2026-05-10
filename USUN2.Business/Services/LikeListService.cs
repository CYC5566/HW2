using USUN2.Business.Abstractions;
using USUN2.Common.Models;
using USUN2.Data.Abstractions;

namespace USUN2.Business.Services;

public sealed class LikeListService(ILikeListRepository likeListRepository) : ILikeListService
{
    public Task<IReadOnlyList<LikeListListItemDto>> GetLikeListForUserAsync(
        string userId,
        string? productNameContains = null,
        CancellationToken cancellationToken = default) =>
        likeListRepository.GetByUserAsync(userId, productNameContains, cancellationToken);

    public Task<LikeListListItemDto?> GetLikeListItemAsync(int sn, string userId, CancellationToken cancellationToken = default) =>
        likeListRepository.GetBySnAsync(sn, userId, cancellationToken);

    public Task<int> AddAsync(LikeListMutationRequest request, CancellationToken cancellationToken = default) =>
        likeListRepository.InsertWithProductAsync(request, cancellationToken);

    public Task UpdateAsync(LikeListUpdateRequest request, string userId, CancellationToken cancellationToken = default) =>
        likeListRepository.UpdateWithProductAsync(request.Sn, userId, request, cancellationToken);

    public Task DeleteAsync(int sn, string userId, CancellationToken cancellationToken = default) =>
        likeListRepository.DeleteAsync(sn, userId, cancellationToken);
}
