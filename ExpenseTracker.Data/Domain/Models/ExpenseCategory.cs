using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Data.Domain.Models
{
    public class ExpenseCategory
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        public DateTime DateCreated { get; set; }


        //Navigation Property
        public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
