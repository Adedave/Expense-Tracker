using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Web.Models;
using ExpenseTracker.Common;
using ExpenseTracker.Biz.IServices;
using System;
using Hangfire;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private SignInManager<AppUser> _signInManager;
        private readonly IViewRenderService _viewRenderService;

        public AccountController(UserManager<AppUser> userMgr, IEmailService emailService,
        SignInManager<AppUser> signinMgr, IViewRenderService viewRenderService)
        {
            _userManager = userMgr;
            _emailService = emailService;
            _signInManager = signinMgr;
            _viewRenderService = viewRenderService;
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = true
                };

                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {

                    await AddUserToARoleAsync(user.Email);

                    string cTokenLink = await GenerateEmailTokenAsync(user.Email);

                    await SendConfirmationEmailAsync(user.Email, cTokenLink);
                    
                    //ViewBag.ConfirmEmail = "Registration successful, kindly check your email to confirm your registration"; /*: "";*/
                    return RedirectToAction("Activate", new { email = user.Email });
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Activate(string email)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            ViewBag.Email = appUser!=null ? email : "<Email Not Found>";
            return View();
        }
        private async Task AddUserToARoleAsync(string userEmail)
        {
            AppUser registeredUser = await _userManager.FindByEmailAsync(userEmail);
            if (registeredUser != null)
            {
                IdentityResult result = await _userManager.AddToRoleAsync(registeredUser, "Users");

                if (!result.Succeeded)
                {
                    AddErrorsFromResult(result);
                }
            }
        }

        private async Task SendConfirmationEmailAsync(string email, string cTokenLink)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            string message = await _viewRenderService.RenderToStringAsync("ConfirmEmailTemplate",email);
            message = message.Replace("{username}", appUser.UserName);
            message = message.Replace("{email}", appUser.Email);
            message = message.Replace("{confirmLink}", cTokenLink);
            var jobId = BackgroundJob.Enqueue(
                () => _emailService.ConfirmEmail(appUser.Email, message));
        }

        [AllowAnonymous]
        public async Task<IActionResult> ResendConfirmationEmailAsync(string email)
        {
            string confirmEmailTokenLink = await GenerateEmailTokenAsync(email);
            //bool success = await SendConfirmationEmailAsync(email, confirmEmailTokenLink);
            await SendConfirmationEmailAsync(email, confirmEmailTokenLink);
            return View("ConfirmEmail");
        }

        private async Task<string> GenerateEmailTokenAsync(string userEmail)
        {
            AppUser user = await _userManager.FindByEmailAsync(userEmail);

            string cToken = _userManager.GenerateEmailConfirmationTokenAsync(user).Result;

            string cTokenLink = Url.Action("ConfirmEmail", "Account",
                values: new { userId = user.Id, token = cToken },
                protocol: HttpContext.Request.Scheme);
            return cTokenLink;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return View("Error", new ErrorViewModel());
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View("Error", new ErrorViewModel());
            }
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return View(result.Succeeded ? "EmailConfirmed" : "Error");
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.returnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel details, string returnUrl)
        {
            if(User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByEmailAsync(details.Email);
                if (!user.EmailConfirmed && user.IsActive == true)
                {
                    return RedirectToAction("Activate", new { email = user.Email });
                }
                if (user != null && user.IsActive == true)
                {
                    await _signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result =
                        await _signInManager.PasswordSignInAsync(
                    user, details.Password, details.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return LocalRedirect(returnUrl ?? "/");
                    }
                }
                ModelState.AddModelError(nameof(LoginViewModel.Email), "Invalid user or password");
            }
            return View(details);
        }

        [AllowAnonymous]
        public IActionResult GoogleLogin(string returnUrl)
        {
            string redirectUrl = Url.Action("GoogleResponse", "Account",
            new { ReturnUrl = returnUrl });
            var properties = _signInManager
            .ConfigureExternalAuthenticationProperties("Google", redirectUrl);
            return new ChallengeResult("Google", properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse(string returnUrl = "/")
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }
            var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, false);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                AppUser user = new AppUser
                {
                    Email = info.Principal.FindFirst(ClaimTypes.Email).Value,
                    UserName =
                info.Principal.FindFirst(ClaimTypes.Email).Value
                };
                IdentityResult identResult = await _userManager.CreateAsync(user);
                if (identResult.Succeeded)
                {
                    identResult = await _userManager.AddLoginAsync(user, info);
                    if (identResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return LocalRedirect(returnUrl);
                    }
                }
                return AccessDenied();
            }
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);
                
                //non-registered user, send no account registered email
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                //active users
                else if (user.IsActive == true)
                {

                    // For more information on how to enable account confirmation and password reset please 
                    // visit https://go.microsoft.com/fwlink/?LinkID=532713
                    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var resetPasswordLink = Url.Action(
                         "ResetPassword", "Account",
                        values: new { email, token = resetToken },
                        protocol: Request.Scheme);

                    await SendResetPasswordEmail(email,resetPasswordLink);

                    return View("ForgotPasswordConfirmation");
                }

                //non-active users, send no account registered email
                return View("ForgotPasswordConfirmation");
            }
            //ModelState.AddModelError(nameof(email), "Email not found!");
            return View();
        }

        private async Task SendResetPasswordEmail(string email, string resetPasswordLink)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            string message = await _viewRenderService.RenderToStringAsync("ResetPasswordTemplate", email);
            message = message.Replace("{username}", appUser.UserName);
            message = message.Replace("{email}", appUser.Email);
            message = message.Replace("{resetPasswordLink}", resetPasswordLink);
            var jobId = BackgroundJob.Enqueue(
               () => _emailService.ResetPassword(appUser.Email, message));
        }

        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token = null)
        {
            if (token == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                ResetPasswordViewModel resetPassword = new ResetPasswordViewModel
                {
                    ResetPasswordToken = token,
                    Email = email
                };
                return View(resetPassword);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPassword)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(resetPassword.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                var result = await _userManager.ResetPasswordAsync(user, resetPassword.ResetPasswordToken, resetPassword.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("ResetPasswordConfirmation");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
