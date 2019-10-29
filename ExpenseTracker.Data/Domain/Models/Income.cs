using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class Income
    {
        public int Id { get; set; }
        public string IncomeSource { get; set; }
        public decimal Amount { get; set; }
        public int IncomeCategoryId { get; set; }
        public DateTime DateOfIncome { get; set; }
        public string MoreDescription { get; set; }

        //Navigation Property
        public IncomeCategory Category { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
