using System.ComponentModel.DataAnnotations;

namespace USUN2.Common.Models;

public sealed class LikeListUpdateRequest : LikeListMutationRequest
{
    [Required]
    public int Sn { get; set; }
}
