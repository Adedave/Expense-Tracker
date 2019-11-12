using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Models
{
    public class BankAccountViewModel
    {
        public int BankAccountId { get; set; }

        [Required(ErrorMessage = "Account Number field is required")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Account Number must be a 10 digit NUBAN account number.")]
        //[RegularExpression("[0,1,2,3,4,5,6,7,8,9]", ErrorMessage = "Account Number must be a 10 digit NUBAN account number.")]
        public string AccountNumber { get; set; }
        public string PreviousAccountNumber { get; set; }

        [Required(ErrorMessage = "Bank Name field is required")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "Alert Email field is required")]
        //[DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string AlertEmail { get; set; }
        public bool IsConnnected { get; set; }
        public string PreviousAlertEmail { get; set; }

        public string AppUserId { get; set; }
    }
}
