using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string NameOfExpense { get; set; }
        public decimal CostOfExpense { get; set; }
        public int ExpenseCategoryId { get; set; }
        public DateTime DateOfExpense { get; set; }
        public string MoreDescription { get; set; }

        //Navigation Property
        public ExpenseCategory ExpenseCategory { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
