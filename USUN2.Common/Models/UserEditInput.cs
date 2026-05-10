using System.ComponentModel.DataAnnotations;

namespace USUN2.Common.Models;

public sealed class UserEditInput
{
    [Required]
    [StringLength(10, MinimumLength = 10)]
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
}
