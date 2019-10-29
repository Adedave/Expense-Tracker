using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class BankTransaction
    {
        public int BankTransactionId { get; set; }
        public string Subject { get; set; }

        //Maps to Name of Expense in Expense class
        public string Description { get; set; }
        public string AccountNumber { get; set; }
        public string TransactionType { get; set; }
        //Maps to DateOfExpense in Expenses class
        public string Remarks { get; set; }

        //Maps to CostOfExpense in Expenses class
        public decimal TransactionAmount { get; set; }

        //Maps to DateOfExpense in Expenses class
        public DateTime TransactionDate { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsRecorded { get; set; }

        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
