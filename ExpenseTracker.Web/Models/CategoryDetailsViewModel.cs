using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Models
{
    public class CategoryDetailsViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public decimal CostOfTOtalExpenses { get; set; }
        public decimal Budget { get; set; }
    }
}
