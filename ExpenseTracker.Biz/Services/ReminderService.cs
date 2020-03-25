using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Common;
using ExpenseTracker.Common.EmailModels;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Biz.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IViewRenderService _viewRenderService;

        public ReminderService(
            IReminderRepository reminderRepository,
            UserManager<AppUser> userManager,
            IEmailService emailService, 
            IViewRenderService viewRenderService)
        {
            _reminderRepository = reminderRepository;
            _userManager = userManager;
            _emailService = emailService;
            _viewRenderService = viewRenderService;
        }
        public List<Reminder> GetReminders(string userId)
        {
            return _reminderRepository.GetAll(userId).ToList();
        }


        public void Addreminder(Reminder reminder)
        {
            _reminderRepository.Insert(reminder);
        }

        public void Deletereminder(int id)
        {
            var reminder = _reminderRepository.GetById(id);
            if (reminder != null)
            {
                _reminderRepository.Delete(reminder);
            }
        }

        public Reminder GetById(int id)
        {
            return _reminderRepository.GetById(id);
        }

        public void Updatereminder(Reminder reminder)
        {
            _reminderRepository.Update(reminder);
        }

        public async Task SendReminderEmail()
        {
            try
            {
                string message = "";

                //it includes appUser object
                var reminder = _reminderRepository.GetAllReminders().ToList();

                foreach (var item in reminder)
                {
                    var currentTime = DateTime.Now;
                    //int comparer = TimeSpan.Compare(item.ReminderTime.TimeOfDay, currentTime.TimeOfDay);
                    bool comparer = false;
                    if (item.ReminderTime.Hour <= currentTime.Hour)
                    {
                        if (item.ReminderTime.Minute < currentTime.Minute)
                        {
                            comparer = true;
                        }
                    }
                          
                    if ( comparer)
                    {
                        continue;
                    }
                    if (item.ReminderTime.Hour == currentTime.Hour && item.ReminderTime.Minute == currentTime.Minute)
                    {
                        //find a way to check if email is valid
                        // you can employ some email validation service
                        //var appUser = await _userManager.FindByIdAsync(item.AppUserId);
                        ReminderEmailModel budgetEmailModel = new ReminderEmailModel()
                        {
                            UserName = item.AppUser.UserName,
                            Heading = "Remember to Record your Expenses",
                            Message = item.ReminderMessage
                        };

                        message = await _viewRenderService.RenderToStringAsync("ReminderEmail", budgetEmailModel);

                        await _emailService.SendReminderEmail(item.AppUser.Email, message);
                    }
                   
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            
        }
    }
}
