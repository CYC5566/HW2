using System.ComponentModel.DataAnnotations;
using USUN2.Common.Validation;

namespace USUN2.Common.Models;

public sealed class UserCreateInput : IValidatableObject
{
    [Required(ErrorMessage = "請輸入身分證字號")]
    [Display(Name = "身分證字號")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "身分證字號須為 10 碼")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入姓名")]
    [StringLength(100)]
    [Display(Name = "姓名")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入電子郵件")]
    [EmailAddress]
    [StringLength(256)]
    [Display(Name = "電子郵件")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入扣款帳號")]
    [StringLength(50)]
    [Display(Name = "扣款帳號")]
    public string Account { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var n = TaiwanNationalIdValidator.Normalize(UserId);
        if (n.Length == 10 && !TaiwanNationalIdValidator.IsValid(n))
            yield return new ValidationResult("身分證字號檢核不正確。", [nameof(UserId)]);

        if (string.IsNullOrWhiteSpace(Account))
            yield return new ValidationResult("請輸入扣款帳號。", [nameof(Account)]);
    }
}
