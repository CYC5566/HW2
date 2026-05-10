using System.ComponentModel.DataAnnotations;
using USUN2.Common.Validation;

namespace USUN2.Common.Models;

public sealed class LoginInput : IValidatableObject
{
    [Required(ErrorMessage = "請輸入身分證字號")]
    [Display(Name = "身分證字號（UserID）")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "身分證字號須為 10 碼")]
    public string UserId { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var n = TaiwanNationalIdValidator.Normalize(UserId);
        if (n.Length == 10 && !TaiwanNationalIdValidator.IsValid(n))
            yield return new ValidationResult("身分證字號檢核不正確。", [nameof(UserId)]);
    }
}
