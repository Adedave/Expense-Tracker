using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class ExpenseCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DateTime DateCreated { get; set; }


        //Navigation Property
        public List<Expense> Expenses { get; set; } = new List<Expense>();

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
