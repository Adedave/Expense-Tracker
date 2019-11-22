using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class IncomeCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }


        //Navigation Property
        public List<Income> Incomes { get; set; } = new List<Income>();

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
