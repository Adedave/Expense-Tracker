using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Web.Models;
using Hangfire;
using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Common;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admins")]
    [Area("Admin")]
    public class AdminController : Controller
    {
        #region Private fields
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserValidator<AppUser> _userValidator;
        private readonly IPasswordValidator<AppUser> _passwordValidator;
        private readonly IPasswordHasher<AppUser> _passwordHasher;
        private readonly IViewRenderService viewRenderService;
        private readonly IEmailService emailService;

        #endregion

        public AdminController(
            UserManager<AppUser> userManager, 
            IUserValidator<AppUser> userValidator,
            IPasswordValidator<AppUser> passwordValidator,
            IPasswordHasher<AppUser> passwordHasher,
            IViewRenderService viewRenderService,
            IEmailService emailService)
        {
            _userManager = userManager;
            _userValidator = userValidator;
            _passwordValidator = passwordValidator;
            _passwordHasher = passwordHasher;
            this.viewRenderService = viewRenderService;
            this.emailService = emailService;
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            var allUsers =  _userManager.Users.ToList();
            var activeUsers = allUsers.RemoveAll(x => x.IsActive == false);
            return View(allUsers);
        }

        public IActionResult Deactivated()
        {
            var allUsers = _userManager.Users.ToList();
            var noOfDeactivatedUsers = allUsers.RemoveAll(x => x.IsActive == true);
            return View(allUsers);
        }

        public async Task<IActionResult> Restore(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = true;
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            var allUsers = _userManager.Users.ToList();
            var activeUsers = allUsers.RemoveAll(x => x.IsActive == true);
            return View("Deactivated", allUsers);
        }
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser
                {
                    UserName = model.Name,
                    Email = model.Email
                };
                IdentityResult result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    TempData["Message"] = $"User \"{model.Name}\" was created successfully!";
                    await AddUserToRoleAsync(user.Email, "Users");

                    string cTokenLink = await GenerateEmailTokenAsync(user.Email);

                    await SendConfirmationEmailAsync(user.Email, cTokenLink);
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
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

        private async Task SendConfirmationEmailAsync(string email, string cTokenLink)
        {
            var appUser = await _userManager.FindByEmailAsync(email);
            string message = await viewRenderService.RenderToStringAsync("ConfirmEmailTemplate", email);
            message = message.Replace("{username}", appUser.UserName);
            message = message.Replace("{email}", appUser.Email);
            message = message.Replace("{confirmLink}", cTokenLink);
            var jobId = BackgroundJob.Enqueue(
                () => emailService.ConfirmEmail(appUser.Email, message));
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

        #region Edit Action Methods
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                return View(user);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string email, string password)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.Email = email;
                IdentityResult validEmail = await _userValidator.ValidateAsync(_userManager, user);
                if (!validEmail.Succeeded)
                {
                    AddErrorsFromResult(validEmail);
                }
                IdentityResult validPassword = null;
                if (!string.IsNullOrEmpty(password) || !string.IsNullOrWhiteSpace(password))
                {
                    validPassword = await _passwordValidator.ValidateAsync(_userManager,user, password);
                    if (validPassword.Succeeded)
                    {
                        user.PasswordHash = _passwordHasher.HashPassword(user, password);
                    }
                    else
                    {
                        AddErrorsFromResult(validPassword);
                    }
                }
                if ((validEmail.Succeeded && validPassword == null) || (validEmail.Succeeded
               && password != string.Empty && validPassword.Succeeded))
                {
                    IdentityResult result = await _userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["Message"] = $"User \"{user?.UserName}\" was updated successfully!";

                        return RedirectToAction("Index");
                    }
                    else
                    {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            return View(user);
        }
        #endregion

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                IdentityResult result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    TempData["Message"] = $"User \"{user.UserName}\" was deactivated successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            var allUsers = _userManager.Users.ToList();
            var activeUsers = allUsers.RemoveAll(x => x.IsActive == false);
            return View("Index", allUsers);
        }

        public async Task<IActionResult> Delete(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = false;
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["Message"] = $"User \"{user.UserName}\" was deleted successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    AddErrorsFromResult(result);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
            }
            var allUsers = _userManager.Users.ToList();
            var activeUsers = allUsers.RemoveAll(x => x.IsActive == false);
            return View("Index", allUsers);
        }
        private void AddErrorsFromResult(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }
    }
}