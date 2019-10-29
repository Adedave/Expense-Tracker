using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Data.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ExpenseTracker.Web.Controllers
{
    public class ReminderController : Controller
    {
        private readonly IReminderService _reminderService;
        private readonly UserManager<AppUser> _userManager;

        public ReminderController(IReminderService reminderService,
             UserManager<AppUser> userManager)
        {
            _reminderService = reminderService;
            _userManager = userManager;
        }
        // GET: /<controller>/
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUser();
            List<Reminder> reminders = new List<Reminder>();
            reminders = _reminderService.GetReminders(user.Id);
            return View(reminders);
        }

        public async Task<IActionResult> Create()
        {
            var user = await GetCurrentUser();
            Reminder reminder = new Reminder
            {
                AppUserId = user.Id
            };
            return View(reminder);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Reminder reminder)
        {
            var user = await GetCurrentUser();
            reminder.AppUserId = user.Id;
            _reminderService.Addreminder(reminder);
            ViewBag.Added = "Reminder created successfully";
            return View(reminder);
        }

        [HttpGet]
        public async Task<IActionResult> Update(int id)
        {
            var user = await GetCurrentUser();
            var reminder = _reminderService.GetById(id);
            if (reminder == null)
            {
                return BadRequest();
            }
            return View(reminder);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Reminder reminder)
        {
            var user = await GetCurrentUser();
            reminder.AppUserId = user.Id;
            _reminderService.Updatereminder(reminder);
            return View(reminder);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            _reminderService.Deletereminder(id);
            return RedirectToAction("Index");
        }

        public async Task<AppUser> GetCurrentUser()
        {
            var user = await _userManager.GetUserAsync(User);
            return user;
        }
    }
}
