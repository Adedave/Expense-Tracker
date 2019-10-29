using ExpenseTracker.Biz.IServices;
using ExpenseTracker.Common;
using ExpenseTracker.Data.Domain.Models;
using ExpenseTracker.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpenseTracker.Biz.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IReminderRepository _reminderRepository;
        private readonly IEmailService _emailService;

        public ReminderService(IReminderRepository reminderRepository, IEmailService emailService)
        {
            _reminderRepository = reminderRepository;
            _emailService = emailService;
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

        public void SendReminderEmail()
        {
            var reminder = _reminderRepository.GetAllReminders().ToList();
            foreach (var item in reminder)
            {
                _emailService.SendReminderEmail(item.AppUser.Email,item.ReminderMessage);
            }
        }
    }
}
