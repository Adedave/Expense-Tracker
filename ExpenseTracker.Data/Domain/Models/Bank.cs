using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class Bank
    {
        public int BankId { get; set; }
        public string Name { get; set; }
        public string AlertEmail { get; set; }

        //public List<BankAccount> BankAccounts { get; set; }
    }
}
