namespace USUN2.Common.Models;

/// <summary>喜好清單查詢列（含使用者與產品彙總欄位）。</summary>
public sealed class LikeListListItemDto
{
    public int Sn { get; init; }
    public string UserId { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal FeeRate { get; init; }
    public int OrderQty { get; init; }
    public string DebitAccount { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public decimal TotalFee { get; init; }
}
