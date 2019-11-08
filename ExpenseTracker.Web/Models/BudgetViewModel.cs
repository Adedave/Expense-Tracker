using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Models
{
    public class BudgetViewModel
    {
        public int Id { get; set; }

        [Required]
        public decimal BudgetAmount { get; set; }

        public decimal TotalExpenses { get; set; }


        [Required]
        public string Month { get; set; }

        [Required]
        public string Year { get; set; }

        [Required]
        public string BudgetCategory { get; set; }

    }
}
