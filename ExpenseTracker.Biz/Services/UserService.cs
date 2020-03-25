using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using Hangfire;
using System.Threading.Tasks;
using ExpenseTracker.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Policy;

namespace ExpenseTracker.Biz.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IViewRenderService viewRenderService;
        private readonly IEmailService emailService;

        public UserService(
            UserManager<AppUser> userManager,
            IViewRenderService viewRenderService,
            IEmailService emailService)
        {
            this.userManager = userManager;
            this.viewRenderService = viewRenderService;
            this.emailService = emailService;
        }

        public async Task<IdentityResult> AddUserToRoleAsync(string userEmail, string role)
        {
            IdentityResult result = null;
            AppUser registeredUser = await userManager.FindByEmailAsync(userEmail);
            if (registeredUser != null)
            {
                result = await userManager.AddToRoleAsync(registeredUser, role);
            }
            return result;
        }

        public async Task SendConfirmationEmailAsync(string email, string cTokenLink)
        {
            var appUser = await userManager.FindByEmailAsync(email);
            string message = await viewRenderService.RenderToStringAsync("ConfirmEmailTemplate", email);
            message = message.Replace("{username}", appUser.UserName);
            message = message.Replace("{email}", appUser.Email);
            message = message.Replace("{confirmLink}", cTokenLink);
            var jobId = BackgroundJob.Enqueue(
                () => emailService.ConfirmEmail(appUser.Email, message));
        }

        //public async Task<string> GenerateEmailTokenAsync(string userEmail)
        //{
        //    AppUser user = await userManager.FindByEmailAsync(userEmail);

        //    string cToken = userManager.GenerateEmailConfirmationTokenAsync(user).Result;

        //    string cTokenLink = Url.Action("ConfirmEmail", "Account",
        //        values: new { userId = user.Id, token = cToken },
        //        protocol: HttpContext.Request.Scheme);
        //    return cTokenLink;
        //}
    }
}
