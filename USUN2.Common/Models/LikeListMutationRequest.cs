using System.ComponentModel.DataAnnotations;

namespace USUN2.Common.Models;

public class LikeListMutationRequest
{
    [Required(ErrorMessage = "請輸入身分證字號")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "身分證字號須為 10 碼")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入產品名稱")]
    [StringLength(200)]
    public string ProductName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "999999999999", ErrorMessage = "產品價格需大於 0")]
    public decimal Price { get; set; }

    [Range(typeof(decimal), "0", "1", ErrorMessage = "手續費率請介於 0 與 1(例如 0.1 表示 10%)")]
    public decimal FeeRate { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "購買數量至少為 1")]
    public int OrderQty { get; set; } = 1;

    [Required(ErrorMessage = "請輸入扣款帳號")]
    [StringLength(50)]
    public string DebitAccount { get; set; } = string.Empty;
}
