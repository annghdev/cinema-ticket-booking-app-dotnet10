using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using CinemaTicketBooking.WebServer.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace CinemaTicketBooking.WebServer.Controllers
{
    [Authorize(AuthenticationSchemes = "Identity.Application")]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
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
