using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USUN2.Business.Abstractions;
using USUN2.Common.Models;
using USUN2.Common.Validation;
using USUN2.Models;

namespace USUN2.Controllers;

public sealed class AccountController(IAppUserManagementService appUserManagementService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginInput());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInput model, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(model);

        var userId = TaiwanNationalIdValidator.Normalize(model.UserId);
        if (!TaiwanNationalIdValidator.IsValid(userId))
        {
            ModelState.AddModelError(nameof(model.UserId), "身分證字號格式或檢核不正確。");
            return View(model);
        }

        var user = await appUserManagementService.GetDetailAsync(userId, cancellationToken);
        if (user is null)
        {
            ModelState.AddModelError(nameof(model.UserId), "查無此身分證字號，請先註冊。");
            return View(model);
        }

        await SignInAsync(user);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register() => View(new UserCreateInput());

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(UserCreateInput model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = TaiwanNationalIdValidator.Normalize(model.UserId);
        if (await appUserManagementService.GetDetailAsync(userId, cancellationToken) is not null)
        {
            ModelState.AddModelError(nameof(model.UserId), "此身分證字號已註冊，請改為登入。");
            return View(model);
        }

        try
        {
            await appUserManagementService.CreateAsync(model, cancellationToken);
            var created = await appUserManagementService.GetDetailAsync(userId, cancellationToken);
            if (created is null)
            {
                ModelState.AddModelError(string.Empty, "註冊後讀取資料失敗，請改以登入頁登入。");
                return View(model);
            }

            await SignInAsync(created);
            TempData["Message"] = "註冊成功，已自動登入。";
            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"註冊失敗：{ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile(CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        var detail = await appUserManagementService.GetDetailAsync(uid, cancellationToken);
        if (detail is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        var page = new UserEditPageModel
        {
            Profile = new UserEditInput
            {
                UserId = detail.UserId,
                UserName = detail.UserName,
                Email = detail.Email,
                Account = detail.Account
            }
        };

        return View(page);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile([Bind(Prefix = "Profile")] UserEditInput profile, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        if (!string.Equals(TaiwanNationalIdValidator.Normalize(profile.UserId), uid, StringComparison.Ordinal))
            return Forbid();

        if (!ModelState.IsValid)
        {
            return View(new UserEditPageModel
            {
                Profile = profile
            });
        }

        try
        {
            await appUserManagementService.UpdateAsync(profile, cancellationToken);
            var updated = await appUserManagementService.GetDetailAsync(uid, cancellationToken);
            if (updated is null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            await SignInAsync(updated);
            TempData["Message"] = "已更新您的資料。";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"更新失敗：{ex.Message}");
            return View(new UserEditPageModel
            {
                Profile = profile
            });
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMyAccount(CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        try
        {
            await appUserManagementService.DeleteAsync(uid, cancellationToken);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Message"] = "您的帳號已刪除。";
            return RedirectToAction(nameof(Login));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"刪除失敗：{ex.Message}";
            return RedirectToAction(nameof(Profile));
        }
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private async Task SignInAsync(AppUserDetailDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true
            });
    }
}
