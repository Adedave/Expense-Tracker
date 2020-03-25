using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Web.Models;
using ExpenseTracker.Common;
using ExpenseTracker.Biz.IServices;
using Hangfire;
using System.Linq;
using ExpenseTracker.Web.Configuration;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly OAuthConfig _oAuthConfig;
        private readonly IUserService userService;
        private SignInManager<AppUser> _signInManager;
        private readonly IViewRenderService _viewRenderService;

        public AccountController(
            UserManager<AppUser> userMgr,
            IEmailService emailService,
            SignInManager<AppUser> signinMgr,
            IViewRenderService viewRenderService,
            OAuthConfig oAuthConfig,
            IUserService userService)
        {
            _userManager = userMgr;
            _emailService = emailService;
            _signInManager = signinMgr;
            _viewRenderService = viewRenderService;
            _oAuthConfig = oAuthConfig;
            this.userService = userService;
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

        
        public async Task<IActionResult> CompleteProfile(string email)
        {
            var appUser = await _userManager.FindByEmailAsync(email);

            RegisterViewModel registerViewModel = new RegisterViewModel()
            {
                Email = appUser.Email,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName                
            };

            return View(registerViewModel);
        }

        //[HttpPost]
        //public async Task<IActionResult> CompleteProfile(RegisterViewModel registerViewModel)
        //{
        //    var appUser = await _userManager.FindByEmailAsync(registerViewModel.Email);
        //    appUser.PhoneNumber = re
        //    RegisterViewModel registerViewModel = new RegisterViewModel()
        //    {
        //        Email = appUser.Email,
        //        FirstName = appUser.FirstName,
        //        LastName = appUser.LastName                
        //    };

        //    return View(registerViewModel);
        //}

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
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

                    await AddUserToRoleAsync(user.Email, "Users");

                    string cTokenLink = await GenerateEmailTokenAsync(user.Email);

                    await SendConfirmationEmailAsync(user.Email, cTokenLink);

                    //ViewBag.ConfirmEmail = "Registration successful, kindly check your email to confirm your registration"; /*: "";*/
                    return RedirectToAction("Activate", new { email = user.Email });
                    //TempData["Message"] = $"{expenses.NameOfExpense} expense added successfully!";
                    //return RedirectToAction("Register");
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
        private async Task AddUserToRoleAsync(string userEmail, string role)
        {
            AppUser registeredUser = await _userManager.FindByEmailAsync(userEmail);
            if (registeredUser != null)
            {
                IdentityResult result = await _userManager.AddToRoleAsync(registeredUser, role);

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
            TempData["Message"] = $"Email sent to {email}! Kindly check your inbox or spam to confirm your registration";
            return RedirectToAction("Login");
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
            if (result.Succeeded)
            {
                TempData["Message"] = "This account has been activated. Kindly log in below";
                return RedirectToAction("Login");
            }
            return View("Error", new ErrorViewModel());
        }

        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            var c = TempData["Message"];
            return View(loginViewModel);
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

                if (user != null && !user.EmailConfirmed && user.IsActive == true)
                {
                    //return RedirectToAction("Activate", new { email = user.Email });
                    ViewBag.NotVerified =  "You have not yet verified your account. Please check your mailbox for instructions on verifying your registration in order to log in";
                    return View(details);
                }
                if (user != null && user.IsActive == true)
                {
                    await _signInManager.SignOutAsync();
                    Microsoft.AspNetCore.Identity.SignInResult result =
                        await _signInManager.PasswordSignInAsync(
                    user, details.Password, details.RememberMe, false);
                    var xc = HttpContext.User;
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
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            string redirectUrl = Url.Action("ExternalLoginCallback", "Account",
            new { ReturnUrl = returnUrl });
            var properties = _signInManager
            .ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
        
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = "/", string remoteError = null)
        {
            LoginViewModel loginViewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            if (remoteError != null)
            {
                ModelState.AddModelError("",$"Error from external provider: {remoteError}");
                return View("Login",loginViewModel);
            }
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError("", $"Error loading external login information");
                return View("Login", loginViewModel);
            }
            var result = await _signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, false);
            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                var success = await ExternallRegister(info);
                if (success)
                {

                    return LocalRedirect(returnUrl);
                }
                ModelState.AddModelError("", $"Email claim not received from {info.LoginProvider}");
                ModelState.AddModelError("", $"Please contact support on support@expensetracker.com");
                return View("Login",loginViewModel);
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalRegister()
        {
            ExternalLoginInfo info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError("", $"Error loading external login information");
                return View("Register", new RegisterViewModel());
            }
            var email = info.Principal.FindFirst(ClaimTypes.Email).Value;
            var firstName = info.Principal.FindFirst(ClaimTypes.GivenName).Value;
            var lastName = info.Principal.FindFirst(ClaimTypes.Surname).Value;

            RegisterViewModel registerViewModel = new RegisterViewModel
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
            ModelState.AddModelError("", $"Email claim not received from {info.LoginProvider}");
            ModelState.AddModelError("", $"Please contact support on support@expensetracker.com");
            return View("Register", registerViewModel);
        }

        private async Task<bool> ExternallRegister(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirst(ClaimTypes.Email).Value;
            var firstName = info.Principal.FindFirst(ClaimTypes.GivenName).Value;
            var lastName = info.Principal.FindFirst(ClaimTypes.Surname).Value;
            if (email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    IdentityResult identResult = await _userManager.AddLoginAsync(user, info);
                    if (identResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, false);
                        return true;
                    }
                }
                else
                {
                    user = new AppUser
                    {
                        Email = email,
                        UserName = email,
                        FirstName = firstName,
                        LastName = lastName,
                        IsActive = true
                    };
                    IdentityResult identityResult = await _userManager.CreateAsync(user);
                    if (identityResult.Succeeded)
                    {
                        await AddUserToRoleAsync(user.Email, "Users");
                        identityResult = await _userManager.AddLoginAsync(user, info);
                        if (identityResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, false);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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
            return RedirectToAction("Index","Home");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
