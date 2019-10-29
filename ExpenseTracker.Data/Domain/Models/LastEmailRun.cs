using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Data.Domain.Models
{
    public class LastEmailRun
    {
        public long UIDValidity { get; set; }

        public long LargestUID { get; set; }
    }
}
