using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public DateTime ReminderTime { get; set; }
        public string ReminderMessage { get; set; }

        //probably should be an enum
        public string ReminderInterval { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
