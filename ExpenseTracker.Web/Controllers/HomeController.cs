using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics;
using ExpenseTracker.Common;
using Microsoft.Extensions.Configuration;

namespace ExpenseTracker.Web.Controllers
{
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<AppUser> userManager;
        private readonly IEmailService emailService;
        private readonly IConfiguration configuration;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _logger = logger;
            this.userManager = userManager;
            this.emailService = emailService;
            this.configuration = configuration;
        }
        public IActionResult Index()
        {
            _logger.LogInformation("HomeController.Index method called!!!");
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public async Task<IActionResult> Error()
        {
            var exceptionHandlerPathFeature =
            HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            _logger.LogError($"The path {exceptionHandlerPathFeature?.Path} " +
                $"threw an exception {exceptionHandlerPathFeature?.Error}");
            var user = await GetCurrentUser();
            _logger.LogInformation($"Logged on user: {user?.LastName} {user?.FirstName}, userId: {user?.Id}");
            Exception ex = exceptionHandlerPathFeature?.Error;
            await SendEmailToDevelopers(ex, user);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<AppUser> GetCurrentUser()
        {
            var user = await userManager.GetUserAsync(User);
            return user;
        }

        private async Task SendEmailToDevelopers(Exception ex, AppUser loggedOnUser)
        {
            if (ex != null)
            {
                string recipientName = configuration["Developer:FirstName"];
                string receiver = configuration["Developer:Email"];
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production")
                {
                    recipientName = Environment.GetEnvironmentVariable("SUPERADMIN_FIRSTNAME");
                    receiver = Environment.GetEnvironmentVariable("SUPERADMIN_EMAIL");

                }
                else if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {

                    recipientName = configuration["Developer:FirstName"];
                    receiver = configuration["Developer:Email"];
                }
                string messageBody = "Kindly check, <br/> <br/>" + $" {ex?.Message} <br/> <br/> {ex?.StackTrace}"
                   + $"<br/> <br/> Logged on user: {loggedOnUser?.LastName} {loggedOnUser?.FirstName}, userId: {loggedOnUser?.Id}" +
                   $"<br/> <br/> Time: {DateTime.Now.ToString()}";
                string subject = "AN ERROR OCCURED IN YOUR EXPENSE TRACKER APPLICATION";
                await emailService.SendMessage(subject, messageBody,receiver);
               
            }
        }
    }
}
