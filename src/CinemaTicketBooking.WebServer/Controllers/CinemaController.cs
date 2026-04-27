using CinemaTicketBooking.Application;
using CinemaTicketBooking.Application.Features;
using CinemaTicketBooking.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CinemaTicketBooking.WebServer.Controllers;

/// <summary>
/// Handles cinema management server-side rendered pages.
/// </summary>
[Authorize(AuthenticationSchemes = "Identity.Application")]
public class CinemaController(IMessageBus bus) : Controller
{
    /// <summary>
    /// Displays a paged list of cinemas.
    /// </summary>
    public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        ViewData["Title"] = "Quản lý rạp";

        var query = new GetPagedCinemasQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await bus.InvokeAsync<PagedResult<CinemaDto>>(query);

        return View(result);
    }

    /// <summary>
    /// Creates a new cinema via AJAX.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCinemaCommand command)
    {
        try
        {
            var id = await bus.InvokeAsync<Guid>(command);
            return Json(new { success = true, message = "Thêm rạp mới thành công!", id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Updates cinema basic info via AJAX.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Update([FromBody] UpdateCinemaBasicInfoCommand command)
    {
        try
        {
            await bus.InvokeAsync(command);
            return Json(new { success = true, message = "Cập nhật rạp thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes a cinema via AJAX.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await bus.InvokeAsync(new DeleteCinemaCommand { Id = id });
            return Json(new { success = true, message = "Xóa rạp thành công!" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }
}
