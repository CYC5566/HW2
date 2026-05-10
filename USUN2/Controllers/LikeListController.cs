using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using USUN2;
using USUN2.Business.Abstractions;
using USUN2.Common.Models;
using USUN2.Data.Abstractions;

namespace USUN2.Controllers;

[Authorize]
public sealed class LikeListController(ILikeListService likeListService, IUserRepository userRepository) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? q, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        ViewBag.ProductNameQuery = q ?? string.Empty;
        var items = await likeListService.GetLikeListForUserAsync(uid, q, cancellationToken);
        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        var user = await userRepository.GetByIdAsync(uid, cancellationToken);
        if (user is null) return Forbid();
        return View(new LikeListMutationRequest { UserId = uid, DebitAccount = user.Account });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LikeListMutationRequest model, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        model.UserId = uid;
        ModelState.Remove(nameof(LikeListMutationRequest.UserId));
        var user = await userRepository.GetByIdAsync(uid, cancellationToken);
        if (user is null) return Forbid();
        model.DebitAccount = user.Account;
        ModelState.Remove(nameof(LikeListMutationRequest.DebitAccount));

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await likeListService.AddAsync(model, cancellationToken);
            TempData["Message"] = "新增成功。";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"儲存失敗：{ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        var item = await likeListService.GetLikeListItemAsync(id, uid, cancellationToken);
        if (item is null)
            return NotFound();
        var user = await userRepository.GetByIdAsync(uid, cancellationToken);
        if (user is null) return Forbid();

        var model = new LikeListUpdateRequest
        {
            Sn = item.Sn,
            UserId = item.UserId,
            ProductName = item.ProductName,
            Price = item.Price,
            FeeRate = item.FeeRate,
            OrderQty = item.OrderQty,
            DebitAccount = user.Account
        };

        ViewBag.UserDisplay = $"{item.UserId} — {item.UserName}";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(LikeListUpdateRequest model, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        model.UserId = uid;
        ModelState.Remove(nameof(LikeListUpdateRequest.UserId));
        var user = await userRepository.GetByIdAsync(uid, cancellationToken);
        if (user is null) return Forbid();
        model.DebitAccount = user.Account;
        ModelState.Remove(nameof(LikeListUpdateRequest.DebitAccount));

        if (!ModelState.IsValid)
        {
            var again = await likeListService.GetLikeListItemAsync(model.Sn, uid, cancellationToken);
            ViewBag.UserDisplay = again is null ? model.UserId : $"{again.UserId} — {again.UserName}";
            return View(model);
        }

        try
        {
            await likeListService.UpdateAsync(model, uid, cancellationToken);
            TempData["Message"] = "更新成功。";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"更新失敗：{ex.Message}");
            var again = await likeListService.GetLikeListItemAsync(model.Sn, uid, cancellationToken);
            ViewBag.UserDisplay = again is null ? model.UserId : $"{again.UserId} — {again.UserName}";
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var uid = User.GetUserId()!;
        try
        {
            await likeListService.DeleteAsync(id, uid, cancellationToken);
            TempData["Message"] = "刪除成功。";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"刪除失敗：{ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }

}
