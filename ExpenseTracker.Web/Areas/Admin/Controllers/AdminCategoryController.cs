using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admins")]
    [Area("Admin")]
    public class AdminCategoryController : Controller
    {
        private readonly IAdminCategoryService _adminCategoryService;
        private readonly UserManager<AppUser> _userManager;

        public AdminCategoryController(IAdminCategoryService adminCategoryService,
            UserManager<AppUser> userManager)
        {
            _adminCategoryService = adminCategoryService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            List<AdminExpenseCategory> categoryList = new List<AdminExpenseCategory>();
            categoryList = _adminCategoryService.GetCategories();
            return View(categoryList);
        }

        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //what happens when the user is null
            var user = await GetCurrentUser();

            await _adminCategoryService.AddCategory(name, user.Id);

            TempData["Message"] = $"Category \"{name}\" created successfully!";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult UpdateCategory(int id)
        {
            var category = _adminCategoryService.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(AdminExpenseCategory category)
        {
            if (category == null || !ModelState.IsValid)
            {
                return View(category);
            }
            var user = await GetCurrentUser();
            //Which is a better practice, send the appUserId to the view or
            // set here in the controller with the HttpContext property
            //category.AppUserId = user.Id;
            _adminCategoryService.UpdateCategory(category, user.Id);

            TempData["Message"] = $"Category \"{category.Name}\" was updated successfully!";

            return RedirectToAction("Index");
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _adminCategoryService.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }
            _adminCategoryService.DeleteCategory(category);

            TempData["Message"] = $"Category \"{category.Name}\" was deleted successfully!";

            return RedirectToAction("Index");
        }

        private async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return user;
        }
    }
}
