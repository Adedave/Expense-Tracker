using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
        public int ExpenseCategoryId { get; set; }

        //Navigation property
        public ExpenseCategory Category { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
