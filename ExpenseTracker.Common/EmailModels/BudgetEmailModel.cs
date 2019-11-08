using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Common.EmailModels
{
    public class BudgetEmailModel
    {
        public string UserName { get; set; }

        //first line heading
        public string HeadingA { get; set; }
        
        //second line heading
        public string HeadingB { get; set; }

        public string Message { get; set; }
    }
}
