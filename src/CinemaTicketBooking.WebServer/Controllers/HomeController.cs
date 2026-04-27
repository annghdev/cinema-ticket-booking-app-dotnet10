using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CinemaTicketBooking.WebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using CinemaTicketBooking.Application.Common.Auth;

namespace CinemaTicketBooking.WebServer.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public class HomeController(IAuthorizationService authorizationService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            // 1. Full Dashboard access for Management
            if (User.IsInRole(RoleNames.SystemAdmin) || 
                User.IsInRole(RoleNames.Admin) || 
                User.IsInRole(RoleNames.Manager))
            {
                return View();
            }

            // 2. Specific operational landing zones per Role
            if (User.IsInRole(RoleNames.MovieCoordinator))
            {
                return RedirectToAction("Index", "Movie");
            }

            if (User.IsInRole(RoleNames.TicketStaff))
            {
                return RedirectToAction("Index", "ShowTime"); 
            }

            // Mặc định cho Customer hoặc Role không xác định
            if (User.IsInRole(RoleNames.Customer))
            {
                return RedirectToAction("Logout", "Account");
            }

            // Absolute fallback (No operations allowed)
            return Forbid(); 
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
