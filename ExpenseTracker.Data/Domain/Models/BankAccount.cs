using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class BankAccount
    {
        public int BankAccountId { get; set; }
        
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        
        public string AlertEmail { get; set; }
        //To know which bank account has already been connected to googleOauth
        //This is set in /Email/Index
        public bool IsConnnected { get; set; }

        //To help keep track of which account I am working on with googleOAuth
        //This is set in /Email/GoogleOAuth
        public bool AboutToConnect { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }
        public GoogleAuth GoogleAuth { get; set; }
    }
}
