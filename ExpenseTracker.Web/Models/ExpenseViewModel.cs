using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Models
{
    public class ExpensesViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Expense { get; set; }

        [Required]
        public decimal CostOfExpense { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public DateTime DateOfExpense { get; set; }

        public string MoreDescription { get; set; }
    }
}
