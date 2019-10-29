using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Controllers
{
    [Authorize(Roles = "Admins")]
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
        public async Task<IActionResult> AddCategory(string name)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //what happens when the user is null
            var user = await GetCurrentUser();

            _adminCategoryService.AddCategory(name, user.Id);

            ViewBag.Added = $"A new Category {name} has been added successfully";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult UpdateCategory(int id)
        {
            var category = _adminCategoryService.GetCategoryById(id);
            if (category == null)
            {
                return BadRequest();
            }
            return View(category);
        }

        [HttpPost]
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

            ViewBag.Updated = $"Category {category.Name} has been updated successfully";
            return RedirectToAction("Index");
        }

        public IActionResult DeleteCategory(int id)
        {
            var category = _adminCategoryService.GetCategoryById(id);
            if (category == null)
            {
                return BadRequest();
            }
            _adminCategoryService.DeleteCategory(category);
            return RedirectToAction("Index");
        }

        private async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            return user;
        }
    }
}
